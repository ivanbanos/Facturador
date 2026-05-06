/* =============================================================================
   MIGRACION: Realinear terceros.terceroId de la BD nueva con los IDs de la BD antigua

   CONTEXTO:
   - Tras la migración a Dataico, los terceros en la BD nueva quedaron con IDs
     distintos a los que tienen las facturas/ordenes (que siguen apuntando al
     terceroId "viejo"). El JOIN por terceroId en getFacturaSinEnviarSiesaSiges
     devuelve NULL en identificación → Siesa falla con "identificación vacía".

   ESTRATEGIA:
   1. Obtener MAX(terceroId) de la BD antigua = @maxOldId.
   2. Reubicar (mover a un id > @maxOldId) los terceros de la BD nueva cuyo
      terceroId actual ESTÁ ocupando un slot que debería ser de un tercero con
      id antiguo. Esto libera los slots 1..@maxOldId para el paso 3.
   3. Para cada tercero de la BD nueva cuya `identificacion` exista en la BD
      antigua, mover su fila al terceroId antiguo. Se mantienen las FK en
      FacturasPOS, OrdenesDeDespacho y FacturasCanastilla.
   4. Reseed identity de dbo.terceros.

   RESTRICCIONES / CONSIDERACIONES:
   - No se puede UPDATE directamente la columna IDENTITY (terceroId). Se usa el
     patrón INSERT-copy + UPDATE de FKs + DELETE original, con IDENTITY_INSERT ON.
   - Existe FK: FacturasPOS.terceroId → terceros.terceroId (se respeta orden).
   - OrdenesDeDespacho.terceroId y FacturasCanastilla.terceroId son columnas
     "sueltas" (sin FK formal) pero deben actualizarse igualmente.
   - Se trabaja en una ÚNICA TRANSACCIÓN con XACT_ABORT ON.
   - Se emiten REPORTES previos para revisión manual antes de aplicar cambios.

   EDGE CASES DETECTADOS (revisar reportes antes de commit):
   (A) Identificaciones duplicadas en la BD antigua → se toma MIN(terceroId).
   (B) Identificaciones duplicadas en la BD nueva → requiere consolidación manual.
   (C) Identificación existe en nueva pero NO en antigua → se deja tal cual
       (puede quedar con id > @maxOldId tras la reubicación).
   (D) Tercero en nueva con misma identificación pero DIFERENTE nombre/tipo que
       en la antigua → probable cliente distinto con mismo documento; se
       realinea igual pero se reporta.
   (E) Facturas huérfanas: terceroId en factura que no existe en ninguna BD →
       no se pueden reparar sin info externa; se reportan.

   PARÁMETROS A CONFIGURAR:
   - @OldDbName: nombre de la base de datos antigua (asume misma instancia).
   - @DryRun:    1 = solo reportes (sin cambios), 0 = aplicar cambios.

   PRE-REQUISITOS:
   - Backup completo de la BD nueva (mandatorio).
   - Detener los servicios EnviadorInformacionService y SigesServicio durante
     la ejecución para evitar escrituras concurrentes.
   - Verificar que el usuario SQL tenga SELECT sobre la BD antigua.
   ============================================================================= */

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @OldDbName SYSNAME = N'FacturadorAntigua';  -- <<< AJUSTAR
DECLARE @DryRun    BIT     = 1;                      -- <<< 1 = solo reportes

-- ---------------------------------------------------------------------------
-- 0. Validar parámetros y existencia de BD antigua
-- ---------------------------------------------------------------------------
IF DB_ID(@OldDbName) IS NULL
BEGIN
    RAISERROR('La base de datos antigua "%s" no existe en esta instancia.', 16, 1, @OldDbName);
    RETURN;
END

DECLARE @sql NVARCHAR(MAX);

-- ---------------------------------------------------------------------------
-- 1. Cargar snapshot de terceros de la BD antigua en una tabla temporal
-- ---------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#TercerosAntiguos') IS NOT NULL DROP TABLE #TercerosAntiguos;
CREATE TABLE #TercerosAntiguos (
    oldTerceroId    INT           NOT NULL,
    identificacion  VARCHAR(50) COLLATE DATABASE_DEFAULT NULL,
    nombre          VARCHAR(50) COLLATE DATABASE_DEFAULT NULL,
    tipoIdent       INT           NULL
);

SET @sql = N'
    INSERT INTO #TercerosAntiguos (oldTerceroId, identificacion, nombre, tipoIdent)
    SELECT terceroId, LTRIM(RTRIM(identificacion)), nombre, tipoIdentificacion
    FROM ' + QUOTENAME(@OldDbName) + N'.dbo.terceros
    WHERE identificacion IS NOT NULL
      AND LTRIM(RTRIM(identificacion)) <> '''';';
EXEC sp_executesql @sql;

DECLARE @maxOldId INT;
SELECT @maxOldId = ISNULL(MAX(oldTerceroId), 0) FROM #TercerosAntiguos;
PRINT CONCAT('>>> MAX terceroId en BD antigua: ', @maxOldId);

IF @maxOldId = 0
BEGIN
    RAISERROR('La BD antigua no contiene terceros válidos. Abortando.', 16, 1);
    RETURN;
END

-- ---------------------------------------------------------------------------
-- 2. REPORTE (A): duplicados por identificación en BD antigua
-- ---------------------------------------------------------------------------
PRINT '=== REPORTE (A): identificaciones duplicadas en BD antigua ===';
SELECT identificacion, COUNT(*) AS veces, MIN(oldTerceroId) AS idElegido
FROM #TercerosAntiguos
GROUP BY identificacion
HAVING COUNT(*) > 1
ORDER BY veces DESC;

-- ---------------------------------------------------------------------------
-- 3. REPORTE (B): duplicados por identificación en BD nueva
-- ---------------------------------------------------------------------------
PRINT '=== REPORTE (B): identificaciones duplicadas en BD nueva ===';
SELECT LTRIM(RTRIM(identificacion)) AS identificacion, COUNT(*) AS veces
FROM dbo.terceros
WHERE identificacion IS NOT NULL AND LTRIM(RTRIM(identificacion)) <> ''
GROUP BY LTRIM(RTRIM(identificacion))
HAVING COUNT(*) > 1
ORDER BY veces DESC;

-- ---------------------------------------------------------------------------
-- 4. Mapa "identificacion → oldTerceroId" (1 por identificación)
-- ---------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#MapaPorIdentificacion') IS NOT NULL DROP TABLE #MapaPorIdentificacion;
CREATE TABLE #MapaPorIdentificacion (
    identificacion VARCHAR(50) COLLATE DATABASE_DEFAULT NOT NULL PRIMARY KEY,
    oldTerceroId   INT         NOT NULL
);
INSERT INTO #MapaPorIdentificacion (identificacion, oldTerceroId)
SELECT identificacion, MIN(oldTerceroId)
FROM #TercerosAntiguos
GROUP BY identificacion;

-- ---------------------------------------------------------------------------
-- 5. Construir plan de cambios: nuevo id actual → id antiguo objetivo
--
-- Si en la BD nueva hay VARIOS terceros con la misma identificación, sólo
-- UNO puede quedarse con el targetId antiguo. Elegimos un "ganador" por
-- identificación usando el criterio:
--   1) mayor cantidad de facturas (POS + OrdenesDeDespacho + Canastilla)
--   2) menor terceroId en caso de empate
-- Los perdedores conservan su terceroId actual y se reportan.
-- ---------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#Cambios') IS NOT NULL DROP TABLE #Cambios;
CREATE TABLE #Cambios (
    currentId      INT         NOT NULL PRIMARY KEY,
    targetId       INT         NOT NULL,
    identificacion VARCHAR(50) COLLATE DATABASE_DEFAULT NOT NULL
);

IF OBJECT_ID('tempdb..#DuplicadosNueva') IS NOT NULL DROP TABLE #DuplicadosNueva;
CREATE TABLE #DuplicadosNueva (
    identificacion VARCHAR(50) COLLATE DATABASE_DEFAULT NOT NULL,
    terceroId      INT         NOT NULL,
    esGanador      BIT         NOT NULL,
    referencias    INT         NOT NULL
);

-- Conteo de referencias por tercero (una sola pasada por tabla)
;WITH RefCount AS (
    SELECT terceroId, COUNT(*) AS cnt FROM dbo.OrdenesDeDespacho WHERE terceroId IS NOT NULL GROUP BY terceroId
    UNION ALL
    SELECT terceroId, COUNT(*) FROM dbo.FacturasPOS WHERE terceroId IS NOT NULL GROUP BY terceroId
    UNION ALL
    SELECT terceroId, COUNT(*) FROM dbo.FacturasCanastilla WHERE terceroId IS NOT NULL GROUP BY terceroId
),
RefAgg AS (
    SELECT terceroId, SUM(cnt) AS referencias FROM RefCount GROUP BY terceroId
),
Candidatos AS (
    SELECT
        LTRIM(RTRIM(t.identificacion)) AS identificacion,
        t.terceroId,
        ISNULL(r.referencias, 0)       AS referencias,
        ROW_NUMBER() OVER (
            PARTITION BY LTRIM(RTRIM(t.identificacion))
            ORDER BY ISNULL(r.referencias, 0) DESC, t.terceroId ASC
        ) AS rn
    FROM dbo.terceros t
    LEFT JOIN RefAgg r ON r.terceroId = t.terceroId
    WHERE t.identificacion IS NOT NULL
      AND LTRIM(RTRIM(t.identificacion)) <> ''
)
INSERT INTO #DuplicadosNueva (identificacion, terceroId, esGanador, referencias)
SELECT identificacion, terceroId, CASE WHEN rn = 1 THEN 1 ELSE 0 END, referencias
FROM Candidatos
WHERE identificacion IN (
    SELECT identificacion FROM Candidatos GROUP BY identificacion HAVING COUNT(*) > 1
);

-- Sólo los ganadores pueden entrar a #Cambios (los no-duplicados entran todos)
INSERT INTO #Cambios (currentId, targetId, identificacion)
SELECT t.terceroId, m.oldTerceroId, LTRIM(RTRIM(t.identificacion))
FROM dbo.terceros t
INNER JOIN #MapaPorIdentificacion m
    ON m.identificacion = LTRIM(RTRIM(t.identificacion)) COLLATE DATABASE_DEFAULT
WHERE t.terceroId <> m.oldTerceroId
  AND NOT EXISTS (
      -- Hay otro tercero en nueva con el targetId exacto y la misma identificación → ya está alineado ese
      SELECT 1 FROM dbo.terceros t2
      WHERE t2.terceroId = m.oldTerceroId
        AND t2.terceroId <> t.terceroId
        AND LTRIM(RTRIM(t2.identificacion)) = LTRIM(RTRIM(t.identificacion))
  )
  -- Si esta identificación está duplicada en la BD nueva, sólo pasa el ganador
  AND NOT EXISTS (
      SELECT 1 FROM #DuplicadosNueva d
      WHERE d.identificacion = LTRIM(RTRIM(t.identificacion)) COLLATE DATABASE_DEFAULT
        AND d.terceroId = t.terceroId
        AND d.esGanador = 0
  );

DECLARE @totalCambios INT = (SELECT COUNT(*) FROM #Cambios);
DECLARE @totalDuplicados INT = (SELECT COUNT(*) FROM #DuplicadosNueva);
DECLARE @totalPerdedores INT = (SELECT COUNT(*) FROM #DuplicadosNueva WHERE esGanador = 0);
PRINT CONCAT('>>> Terceros a realinear: ', @totalCambios);
PRINT CONCAT('>>> Filas involucradas en duplicados de identificacion: ', @totalDuplicados, ' (perdedores no realineados: ', @totalPerdedores, ')');

-- Reporte de duplicados con decisión tomada
PRINT '=== REPORTE (B2): resolución de duplicados en BD nueva ===';
SELECT
    d.identificacion,
    d.terceroId,
    d.referencias,
    CASE WHEN d.esGanador = 1 THEN 'GANADOR (se realinea)' ELSE 'PERDEDOR (se queda en su id actual)' END AS decision
FROM #DuplicadosNueva d
ORDER BY d.identificacion, d.esGanador DESC, d.referencias DESC, d.terceroId;

-- ---------------------------------------------------------------------------
-- 6. Detectar terceros que ocupan un slot targetId pero NO deben estar ahí
--    → deben ser reubicados a id > @maxOldId
-- ---------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#Reubicaciones') IS NOT NULL DROP TABLE #Reubicaciones;
CREATE TABLE #Reubicaciones (
    currentId INT NOT NULL PRIMARY KEY,
    nuevoId   INT NOT NULL
);

DECLARE @offsetBase INT;
SELECT @offsetBase = CASE
    WHEN ISNULL((SELECT MAX(terceroId) FROM dbo.terceros), 0) > @maxOldId
        THEN (SELECT MAX(terceroId) FROM dbo.terceros)
    ELSE @maxOldId
END;

INSERT INTO #Reubicaciones (currentId, nuevoId)
SELECT
    t.terceroId,
    @offsetBase + ROW_NUMBER() OVER (ORDER BY t.terceroId)
FROM dbo.terceros t
WHERE t.terceroId IN (SELECT targetId FROM #Cambios)
  AND t.terceroId NOT IN (SELECT currentId FROM #Cambios);

DECLARE @totalReubic INT = (SELECT COUNT(*) FROM #Reubicaciones);
PRINT CONCAT('>>> Terceros a reubicar (liberar slots antiguos): ', @totalReubic);

-- ---------------------------------------------------------------------------
-- 7. REPORTE (D): cambios con nombre/tipo distinto a la antigua
-- ---------------------------------------------------------------------------
PRINT '=== REPORTE (D): realineaciones con posible conflicto de datos (revisar) ===';
SELECT
    c.currentId      AS nuevoIdActual,
    c.targetId       AS antiguoIdDestino,
    c.identificacion,
    tn.nombre        AS nombreEnNueva,
    ta.nombre        AS nombreEnAntigua,
    tn.tipoIdentificacion AS tipoEnNueva,
    ta.tipoIdent     AS tipoEnAntigua
FROM #Cambios c
INNER JOIN dbo.terceros tn ON tn.terceroId = c.currentId
INNER JOIN #TercerosAntiguos ta ON ta.oldTerceroId = c.targetId
WHERE ISNULL(tn.nombre COLLATE DATABASE_DEFAULT,'') <> ISNULL(ta.nombre,'')
   OR ISNULL(tn.tipoIdentificacion, -1) <> ISNULL(ta.tipoIdent, -1);

-- ---------------------------------------------------------------------------
-- 8. REPORTE (E): facturas huérfanas (terceroId no existe en terceros)
-- ---------------------------------------------------------------------------
PRINT '=== REPORTE (E): facturas con terceroId huérfano ===';
SELECT 'OrdenesDeDespacho' AS tabla, o.facturaPOSId, o.ventaId, o.terceroId
FROM dbo.OrdenesDeDespacho o
WHERE o.terceroId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.terceros t WHERE t.terceroId = o.terceroId);

SELECT 'FacturasPOS' AS tabla, f.facturaPOSId, f.ventaId, f.terceroId
FROM dbo.FacturasPOS f
WHERE f.terceroId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.terceros t WHERE t.terceroId = f.terceroId);

IF OBJECT_ID('dbo.FacturasCanastilla', 'U') IS NOT NULL
BEGIN
    SELECT 'FacturasCanastilla' AS tabla, fc.FacturasCanastillaId, fc.terceroId
    FROM dbo.FacturasCanastilla fc
    WHERE fc.terceroId IS NOT NULL
      AND NOT EXISTS (SELECT 1 FROM dbo.terceros t WHERE t.terceroId = fc.terceroId);
END

-- ---------------------------------------------------------------------------
-- 9. REPORTE (C): terceros en nueva SIN match en antigua (quedan intactos)
-- ---------------------------------------------------------------------------
PRINT '=== REPORTE (C): terceros en BD nueva sin equivalente en antigua ===';
SELECT COUNT(*) AS totalNuevosSinEquivalente
FROM dbo.terceros t
WHERE NOT EXISTS (
    SELECT 1 FROM #MapaPorIdentificacion m
    WHERE m.identificacion = LTRIM(RTRIM(t.identificacion)) COLLATE DATABASE_DEFAULT
);

-- ---------------------------------------------------------------------------
-- 10. APLICACIÓN DE CAMBIOS (solo si @DryRun = 0)
-- ---------------------------------------------------------------------------
IF @DryRun = 1
BEGIN
    PRINT '>>> DRY RUN: no se aplicaron cambios. Revisar reportes y ejecutar con @DryRun = 0.';
    RETURN;
END

-- ---------------------------------------------------------------------------
-- 9b. Guardia: no debería quedar ningún targetId duplicado en #Cambios
--     (tras elegir ganadores en paso 5). Si ocurre, hay un caso edge no cubierto.
-- ---------------------------------------------------------------------------
IF EXISTS (SELECT 1 FROM #Cambios GROUP BY targetId HAVING COUNT(*) > 1)
BEGIN
    PRINT '>>> ERROR inesperado: targetId duplicados en #Cambios tras selección de ganadores.';
    SELECT targetId, COUNT(*) AS conflictos
    FROM #Cambios
    GROUP BY targetId
    HAVING COUNT(*) > 1;
    RAISERROR('Abortando: conflictos de targetId en #Cambios.', 16, 1);
    RETURN;
END

-- ---------------------------------------------------------------------------
-- 10. PLAN UNIFICADO (reubicaciones + realineaciones) con staging seguro
--
-- Estrategia anti-colisión: primero movemos TODO a ids altos únicos
-- (`stagingId`), así los targetId quedan 100% libres; después colocamos
-- los cambios en su targetId. Esto evita colisiones encadenadas dentro
-- de #Cambios (p.ej. A: 10→5 y B: 5→20).
-- ---------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#Plan') IS NOT NULL DROP TABLE #Plan;
CREATE TABLE #Plan (
    currentId INT NOT NULL PRIMARY KEY,
    stagingId INT NOT NULL UNIQUE,
    finalId   INT NOT NULL   -- = stagingId para reubicaciones; = targetId para cambios
);

-- Reubicaciones: su destino final ES el nuevoId alto (ya > @maxOldId)
INSERT INTO #Plan (currentId, stagingId, finalId)
SELECT currentId, nuevoId, nuevoId FROM #Reubicaciones;

-- Staging base para cambios: por encima del mayor id usado por reubicaciones
DECLARE @stagingBase INT;
SELECT @stagingBase = CASE
    WHEN @totalReubic > 0 THEN (SELECT MAX(nuevoId) FROM #Reubicaciones)
    ELSE @offsetBase
END;

INSERT INTO #Plan (currentId, stagingId, finalId)
SELECT
    c.currentId,
    @stagingBase + ROW_NUMBER() OVER (ORDER BY c.currentId),
    c.targetId
FROM #Cambios c;

DECLARE @totalPlan INT = (SELECT COUNT(*) FROM #Plan);
PRINT CONCAT('>>> Total filas en plan (reubicaciones + cambios): ', @totalPlan);

BEGIN TRANSACTION;

    -------------------------------------------------------------------------
    -- FASE A: mover cada fila de currentId → stagingId (id alto único)
    -------------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM #Plan WHERE stagingId <> currentId)
    BEGIN
        PRINT '>>> Fase A: moviendo a staging...';

        SET IDENTITY_INSERT dbo.terceros ON;

        INSERT INTO dbo.terceros
            (terceroId, tipoIdentificacion, identificacion, nombre,
             telefono, correo, direccion, estado, COD_CLI)
        SELECT
            p.stagingId, t.tipoIdentificacion, t.identificacion, t.nombre,
            t.telefono, t.correo, t.direccion, t.estado, t.COD_CLI
        FROM #Plan p
        INNER JOIN dbo.terceros t ON t.terceroId = p.currentId
        WHERE p.stagingId <> p.currentId;

        SET IDENTITY_INSERT dbo.terceros OFF;

        UPDATE o SET o.terceroId = p.stagingId
        FROM dbo.OrdenesDeDespacho o
        INNER JOIN #Plan p ON o.terceroId = p.currentId
        WHERE p.stagingId <> p.currentId;

        UPDATE f SET f.terceroId = p.stagingId
        FROM dbo.FacturasPOS f
        INNER JOIN #Plan p ON f.terceroId = p.currentId
        WHERE p.stagingId <> p.currentId;

        IF OBJECT_ID('dbo.FacturasCanastilla', 'U') IS NOT NULL
        BEGIN
            UPDATE fc SET fc.terceroId = p.stagingId
            FROM dbo.FacturasCanastilla fc
            INNER JOIN #Plan p ON fc.terceroId = p.currentId
            WHERE p.stagingId <> p.currentId;
        END

        DELETE t
        FROM dbo.terceros t
        INNER JOIN #Plan p ON t.terceroId = p.currentId
        WHERE p.stagingId <> p.currentId
          AND NOT EXISTS (SELECT 1 FROM #Plan p2 WHERE p2.stagingId = p.currentId);
        -- La última condición protege el caso raro en que un currentId sea a su vez el stagingId de otra fila.

        DECLARE @rcFaseA INT = @@ROWCOUNT;
        PRINT CONCAT('>>> Fase A completada. Filas base movidas: ', @rcFaseA);
    END

    -------------------------------------------------------------------------
    -- FASE B: mover los CAMBIOS desde stagingId → finalId (= targetId antiguo)
    -- (las reubicaciones ya están en su finalId y no se tocan)
    -------------------------------------------------------------------------
    IF EXISTS (SELECT 1 FROM #Plan WHERE stagingId <> finalId)
    BEGIN
        PRINT '>>> Fase B: realineando staging → targetId antiguo...';

        SET IDENTITY_INSERT dbo.terceros ON;

        INSERT INTO dbo.terceros
            (terceroId, tipoIdentificacion, identificacion, nombre,
             telefono, correo, direccion, estado, COD_CLI)
        SELECT
            p.finalId, t.tipoIdentificacion, t.identificacion, t.nombre,
            t.telefono, t.correo, t.direccion, t.estado, t.COD_CLI
        FROM #Plan p
        INNER JOIN dbo.terceros t ON t.terceroId = p.stagingId
        WHERE p.stagingId <> p.finalId;

        SET IDENTITY_INSERT dbo.terceros OFF;

        UPDATE o SET o.terceroId = p.finalId
        FROM dbo.OrdenesDeDespacho o
        INNER JOIN #Plan p ON o.terceroId = p.stagingId
        WHERE p.stagingId <> p.finalId;

        UPDATE f SET f.terceroId = p.finalId
        FROM dbo.FacturasPOS f
        INNER JOIN #Plan p ON f.terceroId = p.stagingId
        WHERE p.stagingId <> p.finalId;

        IF OBJECT_ID('dbo.FacturasCanastilla', 'U') IS NOT NULL
        BEGIN
            UPDATE fc SET fc.terceroId = p.finalId
            FROM dbo.FacturasCanastilla fc
            INNER JOIN #Plan p ON fc.terceroId = p.stagingId
            WHERE p.stagingId <> p.finalId;
        END

        DELETE t
        FROM dbo.terceros t
        INNER JOIN #Plan p ON t.terceroId = p.stagingId
        WHERE p.stagingId <> p.finalId;

        DECLARE @rcFaseB INT = @@ROWCOUNT;
        PRINT CONCAT('>>> Fase B completada. Realineaciones: ', @rcFaseB);
    END

    -------------------------------------------------------------------------
    -- Reseed identity
    -------------------------------------------------------------------------
    DECLARE @maxActual INT = (SELECT ISNULL(MAX(terceroId), 0) FROM dbo.terceros);
    DBCC CHECKIDENT ('dbo.terceros', RESEED, @maxActual);
    PRINT CONCAT('>>> Identity de terceros reseed a: ', @maxActual);

COMMIT TRANSACTION;

-- ---------------------------------------------------------------------------
-- 11. VERIFICACIÓN POST-MIGRACIÓN
-- ---------------------------------------------------------------------------
PRINT '=== VERIFICACIÓN ===';

-- 11a. Aún quedan facturas con terceroId que no resuelve a un tercero?
SELECT
    'OrdenesDeDespacho huérfanas' AS check_nombre,
    COUNT(*) AS total
FROM dbo.OrdenesDeDespacho o
WHERE o.terceroId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.terceros t WHERE t.terceroId = o.terceroId);

SELECT
    'FacturasPOS huérfanas' AS check_nombre,
    COUNT(*) AS total
FROM dbo.FacturasPOS f
WHERE f.terceroId IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM dbo.terceros t WHERE t.terceroId = f.terceroId);

-- 11b. Cuántos terceros coinciden ahora en id con la BD antigua
SELECT
    'Terceros alineados con BD antigua' AS check_nombre,
    COUNT(*) AS total
FROM dbo.terceros t
INNER JOIN #TercerosAntiguos ta
    ON ta.oldTerceroId = t.terceroId
   AND LTRIM(RTRIM(ta.identificacion)) = LTRIM(RTRIM(t.identificacion)) COLLATE DATABASE_DEFAULT;

PRINT '>>> Migración completada.';


