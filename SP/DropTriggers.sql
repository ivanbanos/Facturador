IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'UpdateFacturaVolverImprimir' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].[UpdateFacturaVolverImprimir];
END;
IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'crearFacturaVenta' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].[crearFacturaVenta];
END;

