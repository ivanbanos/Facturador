CREATE PROCEDURE [dbo].[AgregarFechaReporteFactura]
	@facturas [dbo].[FacturaFechaReporteType] readonly,

	@estacion UNIQUEIDENTIFIER
AS
BEGIN
	update Facturas set FechaReporte = fr.FechaReporte from Facturas f
	INNER JOIN @facturas fr ON fr.IdVentaLocal = f.IdVentaLocal
	INNER JOIN Estaciones e ON e.Id = f.IdEstacion
	where e.Guid = @estacion

	
	update OrdenesDeDespacho set FechaReporte = fr.FechaReporte from OrdenesDeDespacho f
	INNER JOIN @facturas fr ON fr.IdVentaLocal = f.IdVentaLocal
	INNER JOIN Estaciones e ON e.Id = f.IdEstacion
	where e.Guid = @estacion
END
