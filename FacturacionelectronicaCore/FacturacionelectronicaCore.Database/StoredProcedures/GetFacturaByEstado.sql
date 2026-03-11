CREATE PROCEDURE [dbo].[GetFacturaByEstado]
	@estado NVARCHAR(200),
	@estacion UNIQUEIDENTIFIER
AS
BEGIN

	DECLARE @estadoId INT

	SELECT @estadoId = Id FROM Estados WHERE Texto = @estado
	
	SELECT Factura.[Guid], Factura.Consecutivo, Factura.IdTercero, Factura.Combustible, Factura.Cantidad,
			Factura.Precio, Factura.Total, Factura.IdInterno, Factura.Placa, Factura.Kilometraje,
			Factura.Surtidor, Factura.Cara, Factura.Manguera, Factura.Fecha, Factura.FechaProximoMantenimiento, 
			Factura.SubTotal, Factura.Vendedor,Resolucion.Descripcion AS [DescripcionResolucion]
	FROM [dbo].[Facturas] AS Factura
	JOIN [dbo].[Terceros] Tercero
		ON Tercero.Id = Factura.IdTercero
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.Id = Factura.IdEstacion
	JOIN Resolucion 
		ON Resolucion.Id = Factura.IdResolucion
	WHERE Factura.IdEstadoActual = @estadoId AND Estaciones.[Guid] = @estacion
END