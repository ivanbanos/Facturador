CREATE PROCEDURE [dbo].[GetFacturasImprimir]
	@estacion UNIQUEIDENTIFIER
AS
BEGIN
BEGIN TRAN
	DECLARE @idVentaLocalTable AS TABLE (Id INT)

	INSERT INTO @idVentaLocalTable(id)
	SELECT O.IdObjeto FROM ObjetoImprimir O 
	JOIN [dbo].[Facturas] AS Factura
		ON O.IdObjeto = Factura.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	WHERE Estaciones.[Guid] = @estacion
	AND O.Tipo = 'Factura'

	DELETE ObjetoImprimir FROM ObjetoImprimir O 
	JOIN [dbo].[Facturas] AS Factura
		ON O.IdObjeto = Factura.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	WHERE Estaciones.[Guid] = @estacion
	AND O.Tipo = 'Factura'

	SELECT Factura.[Guid], Factura.Consecutivo, Tercero.[Guid] AS IdTercero, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, Factura.Combustible, Factura.Cantidad,
		   Factura.Precio, Factura.Total,Factura.Descuento, Factura.IdInterno, Factura.Placa, Factura.Kilometraje,
		   Factura.Surtidor, Factura.Cara, Factura.Manguera, Factura.Fecha, Factura.IdLocal, Factura.IdVentaLocal,
		   Estado.Texto AS Estado, Factura.FechaProximoMantenimiento, Factura.SubTotal, Factura.Vendedor,Resolucion.Descripcion AS [DescripcionResolucion]
	FROM [dbo].[Facturas] AS Factura
	JOIN [dbo].[Terceros] Tercero
		ON Tercero.Id = Factura.IdTercero
	JOIN [dbo].Estados AS Estado
		ON Estado.Id = Factura.IdEstadoActual
	JOIN @idVentaLocalTable O
		ON O.Id = Factura.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	JOIN Resolucion 
		ON Resolucion.Id = Factura.IdResolucion
	WHERE Estaciones.[Guid] = @estacion
COMMIT TRAN
END
