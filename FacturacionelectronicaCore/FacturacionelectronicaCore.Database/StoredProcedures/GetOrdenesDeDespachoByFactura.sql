CREATE PROCEDURE [dbo].[GetOrdenesDeDespachoByFactura]
	@FacturaGuid UNIQUEIDENTIFIER
AS
BEGIN
	SELECT OrdenDeDespacho.[Guid], Factura.Consecutivo, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, OrdenDeDespacho.Combustible, OrdenDeDespacho.Cantidad,
			OrdenDeDespacho.Precio, OrdenDeDespacho.Total,OrdenDeDespacho.Descuento, OrdenDeDespacho.IdInterno, OrdenDeDespacho.Placa, OrdenDeDespacho.Kilometraje,
			OrdenDeDespacho.Surtidor, OrdenDeDespacho.Cara, OrdenDeDespacho.Manguera, OrdenDeDespacho.Fecha,
			Estado.Texto AS Estado, OrdenDeDespacho.IdLocal, OrdenDeDespacho.IdVentaLocal, OrdenDeDespacho.FechaProximoMantenimiento,
			OrdenDeDespacho.SubTotal, OrdenDeDespacho.Vendedor
	FROM [dbo].[OrdenesDeDespacho] AS OrdenDeDespacho
	JOIN [dbo].[Terceros] AS Tercero
		ON Tercero.Id = OrdenDeDespacho.IdTercero
	JOIN [dbo].[Estados] AS Estado
		ON OrdenDeDespacho.IdEstadoActual = Estado.Id
	JOIN [dbo].[Facturas] AS Factura
		ON Factura.Id = OrdenDeDespacho.IdFactura
	JOIN [dbo].Estaciones AS Estaciones
		ON Estaciones.Id = OrdenDeDespacho.IdEstacion
	WHERE Factura.[Guid] = @FacturaGuid
END
