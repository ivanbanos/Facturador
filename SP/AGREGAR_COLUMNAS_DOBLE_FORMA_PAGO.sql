-- =============================================================================
-- Agregar columnas de doble forma de pago a OrdenesDeDespacho
-- EJECUTAR ANTES de publicar getFacturaSinEnviarSiesa / getFacturaSinEnviarSiesaSiges
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.OrdenesDeDespacho') AND name = 'codigoFormaPago2')
BEGIN
    ALTER TABLE dbo.OrdenesDeDespacho ADD codigoFormaPago2 int NULL;
    PRINT 'Columna codigoFormaPago2 agregada.';
END
ELSE
    PRINT 'Columna codigoFormaPago2 ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.OrdenesDeDespacho') AND name = 'total1')
BEGIN
    ALTER TABLE dbo.OrdenesDeDespacho ADD total1 float NULL;
    PRINT 'Columna total1 agregada.';
END
ELSE
    PRINT 'Columna total1 ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.OrdenesDeDespacho') AND name = 'total2')
BEGIN
    ALTER TABLE dbo.OrdenesDeDespacho ADD total2 float NULL;
    PRINT 'Columna total2 agregada.';
END
ELSE
    PRINT 'Columna total2 ya existe.';
GO
