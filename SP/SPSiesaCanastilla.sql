-- =============================================
-- Store Procedures para Siesa - Canastilla
-- =============================================

-- Agregar columna EnviadaSiesa a FacturasCanastilla si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'EnviadaSiesa' AND Object_ID = Object_ID(N'dbo.FacturasCanastilla'))
BEGIN
    ALTER TABLE dbo.FacturasCanastilla ADD EnviadaSiesa bit default 0;
END
GO

-- =============================================
-- Marcar facturas canastilla como enviadas a Siesa
-- =============================================
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ActualizarEnviadasSiesaCanastilla')
	DROP PROCEDURE [dbo].[ActualizarEnviadasSiesaCanastilla]
GO
CREATE PROCEDURE [dbo].[ActualizarEnviadasSiesaCanastilla]
(
	@facturas [ventasIds] READONLY
)
AS
BEGIN TRY
    SET NOCOUNT ON;
	UPDATE FacturasCanastilla SET EnviadaSiesa = 1
    FROM FacturasCanastilla
    INNER JOIN @facturas f ON f.ventaId = FacturasCanastilla.FacturasCanastillaId
END TRY
BEGIN CATCH
    DECLARE 
        @errorMessage VARCHAR(2000),
        @errorProcedure VARCHAR(255),
        @errorLine INT;

    SELECT  
        @errorMessage = ERROR_MESSAGE(),
        @errorProcedure = ERROR_PROCEDURE(),
        @errorLine = ERROR_LINE();

    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH;
GO

-- =============================================
-- Obtener facturas canastilla no enviadas a Siesa
-- =============================================
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaCanastillaSinEnviarSiesa')
	DROP PROCEDURE [dbo].[getFacturaCanastillaSinEnviarSiesa]
GO
CREATE PROCEDURE [dbo].[getFacturaCanastillaSinEnviarSiesa]
AS
BEGIN TRY
    SET NOCOUNT ON;

    DECLARE @facturasTemp AS TABLE(id INT)
    DECLARE @terceroId INT, @tipoIdentificacion INT

    -- Obtener las facturas canastilla enviadas pero no enviadas a Siesa
    INSERT INTO @facturasTemp (id)
    SELECT TOP(10) FacturasCanastillaId
    FROM FacturasCanastilla
    WHERE (enviada = 1 AND (EnviadaSiesa = 0 OR EnviadaSiesa IS NULL))
    ORDER BY FacturasCanastillaId DESC

    -- Obtener tipo de identificación 'No especificada'
    SELECT @tipoIdentificacion = TipoIdentificacionId 
    FROM dbo.TipoIdentificaciones ti
    WHERE ti.descripcion = 'No especificada'

    -- Buscar o crear tercero 'CONSUMIDOR FINAL'
    SELECT @terceroId = t.terceroId 
    FROM dbo.terceros t
    WHERE t.nombre LIKE '%CONSUMIDOR FINAL%'
    
    IF @terceroId IS NULL
    BEGIN
        INSERT INTO dbo.terceros(COD_CLI, correo, direccion, estado, identificacion, nombre, telefono, tipoIdentificacion)
        VALUES(NULL, 'no informado', 'no informado', 'AC', '222222222222', 'CONSUMIDOR FINAL', 'no informado', @tipoIdentificacion)

        SELECT @terceroId = SCOPE_IDENTITY()
    END

    -- Actualizar terceros sin identificación
    UPDATE FacturasCanastilla SET terceroId = @terceroId
    FROM FacturasCanastilla
    INNER JOIN terceros ON FacturasCanastilla.terceroId = terceros.terceroId
    WHERE terceros.identificacion IS NULL

    -- Retornar facturas con información completa
    SELECT 
        r.descripcion AS descripcionRes, 
        r.autorizacion, 
        r.consecutivoActual,
        r.consecutivoFinal, 
        r.consecutivoInicio, 
        r.esPOS, 
        r.estado,
        r.fechafinal, 
        r.fechaInicio, 
        r.ResolucionId, 
        r.habilitada, 
        fc.FacturasCanastillaId,
        fc.fecha,
        fc.resolucionId,
        fc.consecutivo,
        fc.estado,
        fc.terceroId,
        fc.impresa,
        fc.enviada,
        fc.codigoFormaPago,
        fc.EnviadaSiesa,
        fc.Vendedor,
        fc.isla,
        fc.fechaturno,
        fc.turno,
        t.*, 
        ti.*
    FROM FacturasCanastilla fc
    INNER JOIN @facturasTemp tmp ON tmp.id = fc.FacturasCanastillaId 
    LEFT JOIN dbo.Resoluciones r ON fc.resolucionId = r.ResolucionId
    LEFT JOIN dbo.terceros t ON fc.terceroId = t.terceroId
    LEFT JOIN dbo.TipoIdentificaciones ti ON t.tipoIdentificacion = ti.TipoIdentificacionId
    ORDER BY fc.FacturasCanastillaId DESC
END TRY
BEGIN CATCH
    DECLARE 
        @errorMessage VARCHAR(2000),
        @errorProcedure VARCHAR(255),
        @errorLine INT;

    SELECT  
        @errorMessage = ERROR_MESSAGE(),
        @errorProcedure = ERROR_PROCEDURE(),
        @errorLine = ERROR_LINE();

    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH;
GO
