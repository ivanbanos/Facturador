/*
    Limpieza de ventas duplicadas en OrdenesDeDespacho + indice unico por ventaId.

    Causa: CrearFactura verificaba y luego insertaba sin bloqueo, y varios procesos lo llaman
    a la vez para la misma venta (loop del Enviador, API, SigesServicio).

    Uso:
      1. Ejecutar con @aplicar = 0 (por defecto): solo lista los duplicados y la fila que se conservaria.
      2. Revisar el resultado (consecutivos, impresiones, enviados a la web).
      3. Ejecutar con @aplicar = 1: borra los duplicados y crea el indice unico.
    Ejecutar en Facturacion_Electronica de cada estacion (Juanita y San Jose), con respaldo previo.
*/
USE Facturacion_Electronica
GO
SET NOCOUNT ON;

DECLARE @aplicar BIT = 0;

-- Se conserva por venta: primero la que tiene consecutivo de factura (> 0), luego la mas impresa,
-- luego la primera creada.
IF OBJECT_ID('tempdb..#ordenes') IS NOT NULL DROP TABLE #ordenes;

SELECT o.facturaPOSId, o.ventaId, o.consecutivo, o.impresa, o.enviada, o.terceroId, o.codigoFormaPago, o.fecha,
       ROW_NUMBER() OVER (
           PARTITION BY o.ventaId
           ORDER BY CASE WHEN o.consecutivo > 0 THEN 0 ELSE 1 END,
                    ISNULL(o.impresa, 0) DESC,
                    o.facturaPOSId ASC) AS orden
INTO #ordenes
FROM dbo.OrdenesDeDespacho o
WHERE o.ventaId IN (
    SELECT ventaId FROM dbo.OrdenesDeDespacho GROUP BY ventaId HAVING COUNT(*) > 1
);

SELECT ventaId, facturaPOSId, consecutivo, impresa, enviada, terceroId, codigoFormaPago, fecha,
       CASE WHEN orden = 1 THEN 'CONSERVAR' ELSE 'BORRAR' END AS accion
FROM #ordenes
ORDER BY ventaId, orden;

SELECT COUNT(DISTINCT ventaId) AS ventasDuplicadas, SUM(CASE WHEN orden > 1 THEN 1 ELSE 0 END) AS filasABorrar
FROM #ordenes;

IF @aplicar = 1
BEGIN
    BEGIN TRAN;

    DELETE o
    FROM dbo.OrdenesDeDespacho o
    INNER JOIN #ordenes d ON d.facturaPOSId = o.facturaPOSId
    WHERE d.orden > 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.OrdenesDeDespacho GROUP BY ventaId HAVING COUNT(*) > 1)
       AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_OrdenesDeDespacho_ventaId'
                       AND object_id = OBJECT_ID('dbo.OrdenesDeDespacho'))
    BEGIN
        CREATE UNIQUE INDEX UX_OrdenesDeDespacho_ventaId ON dbo.OrdenesDeDespacho(ventaId);
    END

    COMMIT TRAN;
    PRINT 'Duplicados eliminados e indice unico creado.';
END
GO
