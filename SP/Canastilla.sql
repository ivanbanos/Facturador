-- =============================================
-- Canastilla.sql - Refactored
-- =============================================
USE Facturacion_Electronica
GO

-- =============================================
-- TABLES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Canastilla')
BEGIN
    CREATE TABLE dbo.Canastilla (
        CanastillaId INT PRIMARY KEY IDENTITY (1, 1),
        descripcion VARCHAR(50) NOT NULL,
        unidad VARCHAR(50) NOT NULL,
        precio FLOAT NOT NULL,
        deleted BIT NOT NULL DEFAULT 0,
        iva INT NOT NULL DEFAULT 0,
        [guid] UNIQUEIDENTIFIER
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'iva' AND Object_ID = Object_ID(N'dbo.Canastilla'))
BEGIN
    ALTER TABLE dbo.Canastilla ADD iva INT NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FacturasCanastilla')
BEGIN
    CREATE TABLE dbo.FacturasCanastilla (
        FacturasCanastillaId INT PRIMARY KEY IDENTITY (1, 1),
        fecha DATETIME NOT NULL,
        resolucionId INT NOT NULL,
        consecutivo INT NOT NULL,
        estado CHAR(2),
        terceroId INT,
        impresa INT DEFAULT 0,
        enviada BIT DEFAULT 0,
        codigoFormaPago INT NOT NULL DEFAULT 4,
        codigoFormaPago2 INT NULL,
        total1 FLOAT NULL,
        total2 FLOAT NULL,
        subtotal FLOAT NOT NULL,
        descuento FLOAT NOT NULL,
        iva FLOAT NOT NULL,
        total FLOAT NOT NULL,
        Vendedor VARCHAR(50) NULL,
        isla VARCHAR(50) NULL,
        fechaturno INT NULL,
        turno INT NULL,
        turnoguid VARCHAR(50) NULL,
        FOREIGN KEY (resolucionId) REFERENCES dbo.Resoluciones (ResolucionId)
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'placa' AND Object_ID = Object_ID(N'dbo.FacturasCanastilla'))
BEGIN
    ALTER TABLE dbo.FacturasCanastilla ADD placa VARCHAR(20) NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'codigoFormaPago2' AND Object_ID = Object_ID(N'dbo.FacturasCanastilla'))
BEGIN
    ALTER TABLE dbo.FacturasCanastilla ADD codigoFormaPago2 INT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'total1' AND Object_ID = Object_ID(N'dbo.FacturasCanastilla'))
BEGIN
    ALTER TABLE dbo.FacturasCanastilla ADD total1 FLOAT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'total2' AND Object_ID = Object_ID(N'dbo.FacturasCanastilla'))
BEGIN
    ALTER TABLE dbo.FacturasCanastilla ADD total2 FLOAT NULL;
END
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'turnoguid' AND Object_ID = Object_ID(N'dbo.FacturasCanastilla'))
BEGIN
    ALTER TABLE dbo.FacturasCanastilla ADD turnoguid VARCHAR(50) NULL;
END
GO
--ALTER TABLE FacturasCanastilla
--add Vendedor varchar(50) null;
-- ALTER TABLE FacturasCanastilla
--add isla varchar(50) null;
-- ALTER TABLE FacturasCanastilla
--add fechaturno int null;
-- ALTER TABLE FacturasCanastilla
--add turno int null;

GO
IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'canastillaId' AND Object_ID = Object_ID(N'dbo.FacturasCanastilla'))
BEGIN
    ALTER TABLE dbo.FacturasCanastilla DROP COLUMN canastillaId;
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FacturasCanastillaDetalle')
BEGIN
    CREATE TABLE dbo.FacturasCanastillaDetalle (
        FacturasCanastillaDetalleId INT PRIMARY KEY IDENTITY (1, 1),
        FacturasCanastillaId INT NOT NULL,
        canastillaId INT NOT NULL,
        cantidad FLOAT NOT NULL,
        precio FLOAT NOT NULL,
        subtotal FLOAT NOT NULL,
        iva FLOAT NOT NULL,
        total FLOAT NOT NULL,
        FOREIGN KEY (FacturasCanastillaId) REFERENCES dbo.FacturasCanastilla (FacturasCanastillaId)
    );
END
GO

-- =============================================
-- TYPES
-- =============================================
IF TYPE_ID(N'dbo.CanastillaType') IS NOT NULL
	drop procedure UpdateOrCreateCanastilla
	drop procedure CrearFacturaCanastilla
    DROP TYPE dbo.CanastillaType;
GO
CREATE TYPE dbo.CanastillaType AS TABLE (
    CanastillaId INT NULL,
    [Guid] UNIQUEIDENTIFIER NULL,
    descripcion VARCHAR(50) NOT NULL,
    unidad VARCHAR(50) NOT NULL,
    precio FLOAT NOT NULL,
    deleted BIT NOT NULL DEFAULT 0,
    iva INT NOT NULL DEFAULT 0,
    cantidad FLOAT NULL
);
GO

-- =============================================
-- PROCEDURES
-- =============================================
CREATE OR ALTER PROCEDURE dbo.UpdateOrCreateCanastilla
    @canastillas dbo.CanastillaType READONLY
AS
BEGIN
    SET NOCOUNT ON;
    -- Update existing
    UPDATE c
    SET c.descripcion = src.descripcion,
        c.unidad = src.unidad,
        c.precio = src.precio,
        c.deleted = src.deleted,
        c.iva = src.iva
    FROM dbo.Canastilla c
    INNER JOIN @canastillas src ON c.[Guid] = src.[Guid];

    -- Insert new
    INSERT INTO dbo.Canastilla ([Guid], descripcion, unidad, precio, deleted, iva)
    SELECT src.[Guid], src.descripcion, src.unidad, src.precio, 0, src.iva
    FROM @canastillas src
    LEFT JOIN dbo.Canastilla c ON c.[Guid] = src.[Guid]
    WHERE c.[Guid] IS NULL;

    -- Mark deleted
    UPDATE c
    SET c.deleted = 1
    FROM dbo.Canastilla c
    LEFT JOIN @canastillas src ON c.[Guid] = src.[Guid]
    WHERE src.[Guid] IS NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.SetFacturaCanastillaImpresa
    @facturaCanastillaId INT
AS
BEGIN TRY
    UPDATE dbo.FacturasCanastilla
    SET impresa = 1
    WHERE FacturasCanastillaId = @facturaCanastillaId;
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(2000), @errorProcedure NVARCHAR(255), @errorLine INT;
    SELECT @errorMessage = ERROR_MESSAGE(), @errorProcedure = ERROR_PROCEDURE(), @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH
GO

CREATE OR ALTER PROCEDURE dbo.MandarImprimirCanastilla
    @facturaCanastillaId INT
AS
BEGIN TRY
    DECLARE @impresa INT;
    SELECT @impresa = impresa FROM dbo.FacturasCanastilla WHERE FacturasCanastillaId = @facturaCanastillaId;
    IF @impresa >= 0
    BEGIN
        UPDATE dbo.FacturasCanastilla SET impresa = -1 WHERE FacturasCanastillaId = @facturaCanastillaId;
    END
    ELSE
    BEGIN
        UPDATE dbo.FacturasCanastilla SET impresa = impresa - 1 WHERE FacturasCanastillaId = @facturaCanastillaId;
    END
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(2000), @errorProcedure NVARCHAR(255), @errorLine INT;
    SELECT @errorMessage = ERROR_MESSAGE(), @errorProcedure = ERROR_PROCEDURE(), @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH
GO

CREATE OR ALTER PROCEDURE dbo.getFacturaImprimirCanastilla
AS
BEGIN TRY
    SET NOCOUNT ON;
    SELECT TOP(1)
        r.descripcion AS descripcionRes, r.autorizacion, r.consecutivoActual,
        r.consecutivoFinal, r.consecutivoInicio, r.esPOS, r.estado,
        r.fechafinal, r.fechaInicio, r.ResolucionId, r.habilitada,
        f.FacturasCanastillaId, f.*, t.*, ti.*
    FROM dbo.FacturasCanastilla f
    LEFT JOIN dbo.Resoluciones r ON f.resolucionId = r.ResolucionId
    LEFT JOIN dbo.terceros t ON f.terceroId = t.terceroId
    LEFT JOIN dbo.TipoIdentificaciones ti ON t.tipoIdentificacion = ti.TipoIdentificacionId
    WHERE f.impresa <= -1;
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(2000), @errorProcedure NVARCHAR(255), @errorLine INT;
    SELECT @errorMessage = ERROR_MESSAGE(), @errorProcedure = ERROR_PROCEDURE(), @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH
GO

CREATE OR ALTER PROCEDURE dbo.getFacturaCanatillaDetalle
    @FacturaCanastillaId INT
AS
BEGIN TRY
    SET NOCOUNT ON;
    SELECT *
    FROM dbo.FacturasCanastillaDetalle d
    LEFT JOIN dbo.Canastilla c ON d.canastillaId = c.canastillaId
    WHERE d.FacturasCanastillaId = @FacturaCanastillaId;
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(2000), @errorProcedure NVARCHAR(255), @errorLine INT;
    SELECT @errorMessage = ERROR_MESSAGE(), @errorProcedure = ERROR_PROCEDURE(), @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH
GO

CREATE OR ALTER PROCEDURE dbo.GetCanastilla
AS
BEGIN TRY
    SET NOCOUNT ON;
    SELECT * FROM dbo.Canastilla WHERE deleted = 0;
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(2000), @errorProcedure NVARCHAR(255), @errorLine INT;
    SELECT @errorMessage = ERROR_MESSAGE(), @errorProcedure = ERROR_PROCEDURE(), @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH
GO

CREATE OR ALTER PROCEDURE dbo.getFacturaEnviarCanastilla
AS
BEGIN TRY
    SET NOCOUNT ON;
    SELECT TOP(10)
        r.descripcion AS descripcionRes, r.autorizacion, r.consecutivoActual,
        r.consecutivoFinal, r.consecutivoInicio, r.esPOS, r.estado,
        r.fechafinal, r.fechaInicio, r.ResolucionId, r.habilitada,
        f.FacturasCanastillaId, f.*, t.*, ti.*
    FROM dbo.FacturasCanastilla f
    LEFT JOIN dbo.Resoluciones r ON f.resolucionId = r.ResolucionId
    LEFT JOIN dbo.terceros t ON f.terceroId = t.terceroId
    LEFT JOIN dbo.TipoIdentificaciones ti ON t.tipoIdentificacion = ti.TipoIdentificacionId
    WHERE f.enviada = 0
    ORDER BY f.FacturasCanastillaId DESC;
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(2000), @errorProcedure NVARCHAR(255), @errorLine INT;
    SELECT @errorMessage = ERROR_MESSAGE(), @errorProcedure = ERROR_PROCEDURE(), @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH
GO

CREATE OR ALTER PROCEDURE dbo.SetFacturaCanastillaEnviada
    @facturas dbo.ventasIds READONLY
AS
BEGIN TRY
    UPDATE f
    SET f.enviada = 1
    FROM dbo.FacturasCanastilla f
    INNER JOIN @facturas v ON v.ventaId = f.FacturasCanastillaId;
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(2000), @errorProcedure NVARCHAR(255), @errorLine INT;
    SELECT @errorMessage = ERROR_MESSAGE(), @errorProcedure = ERROR_PROCEDURE(), @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH
GO

CREATE OR ALTER PROCEDURE dbo.BuscarFacturaCanastillaPorConsecutivo
    @consecutivo INT
AS
BEGIN TRY
    SET NOCOUNT ON;
    SELECT r.descripcion AS descripcionRes, r.autorizacion, r.consecutivoActual,
        r.consecutivoFinal, r.consecutivoInicio, r.esPOS, r.estado,
        r.fechafinal, r.fechaInicio, r.ResolucionId, r.habilitada,
        f.FacturasCanastillaId, f.*, t.*, ti.*
    FROM dbo.FacturasCanastilla f
    LEFT JOIN dbo.Resoluciones r ON f.resolucionId = r.ResolucionId
    LEFT JOIN dbo.terceros t ON f.terceroId = t.terceroId
    LEFT JOIN dbo.TipoIdentificaciones ti ON t.tipoIdentificacion = ti.TipoIdentificacionId
    WHERE f.consecutivo = @consecutivo;
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(2000), @errorProcedure NVARCHAR(255), @errorLine INT;
    SELECT @errorMessage = ERROR_MESSAGE(), @errorProcedure = ERROR_PROCEDURE(), @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH
GO

CREATE OR ALTER PROCEDURE dbo.FacturasCanastillaPorImprimir
AS
BEGIN TRY
    SET NOCOUNT ON;
    SELECT FacturasCanastillaId FROM dbo.FacturasCanastilla WHERE impresa <= -1;
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(2000), @errorProcedure NVARCHAR(255), @errorLine INT;
    SELECT @errorMessage = ERROR_MESSAGE(), @errorProcedure = ERROR_PROCEDURE(), @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH
GO

CREATE OR ALTER PROCEDURE dbo.GetFacturasCanastillaIslaTurno
    @isla VARCHAR(50) = NULL,
    @fechaturno INT = NULL,
    @turno INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.descripcion AS descripcionRes, r.autorizacion, r.consecutivoActual,
        r.consecutivoFinal, r.consecutivoInicio, r.esPOS, r.estado,
        r.fechafinal, r.fechaInicio, r.ResolucionId, r.habilitada,
        f.FacturasCanastillaId, f.*, t.*, ti.*
    FROM dbo.FacturasCanastilla f
    LEFT JOIN dbo.Resoluciones r ON f.resolucionId = r.ResolucionId
    LEFT JOIN dbo.terceros t ON f.terceroId = t.terceroId
    LEFT JOIN dbo.TipoIdentificaciones ti ON t.tipoIdentificacion = ti.TipoIdentificacionId
    WHERE (@isla IS NULL OR f.isla = @isla)
      AND (@fechaturno IS NULL OR f.fechaturno = @fechaturno)
      AND (@turno IS NULL OR f.turno = @turno);
END
GO

-- =============================================
-- CLEANUP
-- =============================================
DELETE f
FROM dbo.FacturasCanastilla f
LEFT JOIN dbo.FacturasCanastillaDetalle d ON f.FacturasCanastillaId = d.FacturasCanastillaId
WHERE d.FacturasCanastillaDetalleId IS NULL;
GO

-- =============================================
-- Add back CrearFacturaCanastilla
-- =============================================
CREATE OR ALTER PROCEDURE dbo.CrearFacturaCanastilla
(
    @terceroId INT,
    @COD_FOR_PAG SMALLINT,
    @canastillaIds dbo.CanastillaType READONLY,
    @descuento FLOAT,
    @imprimir BIT = 1,
    @vendedor VARCHAR(50) = NULL,
    @isla VARCHAR(50) = NULL,
    @placa VARCHAR(20) = NULL,
    @COD_FOR_PAG_2 SMALLINT = NULL,
    @total1 FLOAT = NULL,
    @total2 FLOAT = NULL
)
AS
BEGIN TRY
    SET NOCOUNT ON;
    DECLARE @ResolucionId INT, @consecutivoActual INT, @fechafinal DATETIME, @facturaCanastillaId INT, @ConsecutivoFinal INT, @cantidadCanastillas INT, @mismaResolucion VARCHAR(50), @fecha INT, @turno INT;
    DECLARE @turnoGuid VARCHAR(50);
    DECLARE @subtotal FLOAT = 0, @totalIva FLOAT = 0, @total FLOAT = 0;
    DECLARE @ivaPorcentaje BIT = 0;
    DECLARE @codigoFormaPagoPrincipal SMALLINT;
    DECLARE @montoPago1 FLOAT;
    DECLARE @montoPago2 FLOAT;

    SET @codigoFormaPagoPrincipal = ISNULL(@COD_FOR_PAG, @COD_FOR_PAG_2);
    IF @codigoFormaPagoPrincipal IS NULL
    BEGIN
        SET @codigoFormaPagoPrincipal = 4;
    END

    -- Obtener información del turno
    SELECT @fecha = FECHA, @turno = NUM_TUR 
    FROM ventas.dbo.TURN_EST 
    WHERE TURN_EST.estado != 'C' AND COD_ISL = @isla
    ORDER BY FECHA DESC;

    IF (@fecha IS NULL OR @turno IS NULL) AND @isla IS NOT NULL
    BEGIN
        SELECT TOP(1) @fecha = FECHA, @turno = NUM_TUR
        FROM ventas.dbo.TURN_EST
        WHERE COD_ISL = @isla
        ORDER BY CASE WHEN estado != 'C' THEN 0 ELSE 1 END, FECHA DESC, NUM_TUR DESC;
    END

    IF @fecha IS NOT NULL AND @turno IS NOT NULL AND @isla IS NOT NULL
    BEGIN
        SELECT @turnoGuid = CONVERT(VARCHAR(36), CONVERT(UNIQUEIDENTIFIER, HASHBYTES('MD5', CONCAT(CONVERT(VARCHAR(20), @fecha), '|', @isla, '|', CONVERT(VARCHAR(20), @turno)))));
    END

    -- Determinar tipo de IVA (porcentaje vs valor fijo)
    SELECT @ivaPorcentaje = CASE WHEN MAX(c.iva) < 30 THEN 1 ELSE 0 END
    FROM @canastillaIds c;

    -- Calcular totales según tipo de IVA
    IF @ivaPorcentaje = 1
    BEGIN
        SELECT 
            @subtotal = SUM(ROUND(c.cantidad * c.precio, 2)),
            @totalIva = SUM(ROUND((c.cantidad * c.precio) * (c.iva/100.0), 2))
        FROM @canastillaIds c;
    END
    ELSE 
    BEGIN
        SELECT 
            @subtotal = SUM(ROUND(c.cantidad * c.precio, 2)),
            @totalIva = SUM(ROUND(c.cantidad * c.iva, 2))
        FROM @canastillaIds c;
    END

    SET @total = @subtotal + @totalIva - @descuento;

    IF @subtotal <= 0
    BEGIN
        SELECT 0 AS facturaCanastillaId;
        RETURN;
    END

    SET @montoPago1 = ISNULL(@total1, 0);
    SET @montoPago2 = ISNULL(@total2, 0);

    IF @total1 IS NULL AND @total2 IS NULL
    BEGIN
        SET @montoPago1 = @total;
        SET @montoPago2 = 0;
    END
    ELSE IF @total1 IS NULL
    BEGIN
        SET @montoPago1 = @total - @montoPago2;
    END
    ELSE IF @total2 IS NULL
    BEGIN
        SET @montoPago2 = @total - @montoPago1;
    END

    -- Determinar configuración de resolución
    SELECT @cantidadCanastillas = COUNT(ResolucionId) 
    FROM Resoluciones 
    WHERE esPos = 'S' AND estado = 'AC';

    SET @mismaResolucion = CASE 
        WHEN @cantidadCanastillas = 1 THEN 'SI' 
        ELSE ISNULL((SELECT valor FROM configuracionEstacion WHERE descripcion = 'mismaResolucion'), 'NO')
    END;

    -- Obtener resolución y actualizar consecutivo
    SELECT @ResolucionId = ResolucionId, 
           @consecutivoActual = consecutivoActual, 
           @fechafinal = fechafinal, 
           @ConsecutivoFinal = consecutivoFinal
    FROM Resoluciones 
    WHERE esPos = 'S' AND estado = 'AC' AND (@mismaResolucion = 'SI' OR tipo = 1);

    UPDATE Resoluciones 
    SET consecutivoActual = @consecutivoActual + 1 
    WHERE esPos = 'S' AND estado = 'AC';

    IF @fechafinal IS NULL OR @fechafinal < GETDATE() OR @ConsecutivoFinal <= @consecutivoActual
    BEGIN
        UPDATE Resoluciones 
        SET estado = 'IN' 
        WHERE esPos = 'S' AND estado = 'AC' AND (@mismaResolucion = 'SI' OR tipo = 1);
        SELECT @facturaCanastillaId AS facturaCanastillaId;
        RETURN;
    END

    -- Crear factura
    INSERT INTO FacturasCanastilla (
        fecha, resolucionId, consecutivo, estado, terceroId, enviada, 
        codigoFormaPago, codigoFormaPago2, total1, total2, subtotal, descuento, iva, total, impresa,
        vendedor, isla, fechaturno, turno, turnoguid, placa
    )
    VALUES (
        GETDATE(), @ResolucionId, @consecutivoActual, 'CR', @terceroId, 0, 
        @codigoFormaPagoPrincipal, @COD_FOR_PAG_2, @montoPago1, @montoPago2, @subtotal, @descuento, @totalIva, @total, -1,
        @vendedor, @isla, @fecha, @turno, @turnoGuid, @placa
    );

    SET @facturaCanastillaId = SCOPE_IDENTITY();

    -- Crear detalle de factura
    IF @ivaPorcentaje = 1
    BEGIN
        INSERT INTO FacturasCanastillaDetalle (
            FacturasCanastillaId, canastillaId, cantidad, precio, subtotal, iva, total
        )
        SELECT 
            @facturaCanastillaId, 
            cids.canastillaId, 
            cids.cantidad, 
            cids.precio,
            ROUND(cids.cantidad * cids.precio, 2) AS subtotal,
            ROUND((cids.cantidad * cids.precio) * (cids.iva/100.0), 2) AS iva,
            ROUND((cids.cantidad * cids.precio) + ((cids.cantidad * cids.precio) * (cids.iva/100.0)), 2) AS total
        FROM @canastillaIds cids;
    END
    ELSE 
    BEGIN
        INSERT INTO FacturasCanastillaDetalle (
            FacturasCanastillaId, canastillaId, cantidad, precio, subtotal, iva, total
        )
        SELECT 
            @facturaCanastillaId, 
            cids.canastillaId, 
            cids.cantidad, 
            cids.precio,
            ROUND(cids.cantidad * cids.precio, 2) AS subtotal,
            ROUND(cids.cantidad * cids.iva, 2) AS iva,
            ROUND((cids.cantidad * cids.precio) + (cids.cantidad * cids.iva), 2) AS total
        FROM @canastillaIds cids;
    END

    SELECT consecutivo AS facturaCanastillaId 
    FROM FacturasCanastilla 
    WHERE FacturasCanastillaId = @facturaCanastillaId;
END TRY
BEGIN CATCH
    DECLARE @errorMessage NVARCHAR(2000), @errorProcedure NVARCHAR(255), @errorLine INT;
    SELECT @errorMessage = ERROR_MESSAGE(), @errorProcedure = ERROR_PROCEDURE(), @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH
GO
CREATE OR ALTER PROCEDURE ReimprimirFacturaCanastilla
    @consecutivo INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE FacturasCanastilla
    SET impresa = -1
    WHERE consecutivo = @consecutivo;
END
GO
