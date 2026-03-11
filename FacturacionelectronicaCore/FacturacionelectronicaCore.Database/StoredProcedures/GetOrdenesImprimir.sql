CREATE PROCEDURE [dbo].[GetOrdenesImprimir]
	@estacion UNIQUEIDENTIFIER
AS
BEGIN

	DECLARE @idVentaLocalTable AS TABLE (Id int)

	INSERT INTO @idVentaLocalTable(Id)
	SELECT O.IdObjeto FROM ObjetoImprimir O 
	JOIN [dbo].[OrdenesDeDespacho] AS Factura
		ON O.IdObjeto = Factura.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	WHERE Estaciones.[Guid] = @estacion
	and O.Tipo = 'Orden'

	DELETE ObjetoImprimir FROM ObjetoImprimir O 
	JOIN [dbo].[OrdenesDeDespacho] AS Factura
		ON O.IdObjeto = Factura.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	WHERE Estaciones.[Guid] = @estacion
	and O.Tipo = 'Orden'

	SELECT OrdenDeDespacho.[Guid], Factura.Consecutivo, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, OrdenDeDespacho.Combustible, OrdenDeDespacho.Cantidad,
				OrdenDeDespacho.Precio, OrdenDeDespacho.Total,OrdenDeDespacho.Descuento, OrdenDeDespacho.IdInterno, OrdenDeDespacho.Placa, OrdenDeDespacho.Kilometraje,
				OrdenDeDespacho.Surtidor, OrdenDeDespacho.Cara, OrdenDeDespacho.Manguera, OrdenDeDespacho.Fecha,
				Estado.Texto AS Estado, OrdenDeDespacho.IdLocal, OrdenDeDespacho.IdVentaLocal, OrdenDeDespacho.FechaProximoMantenimiento,
				OrdenDeDespacho.SubTotal, OrdenDeDespacho.Vendedor
	FROM [dbo].[OrdenesDeDespacho] AS OrdenDeDespacho
	JOIN [dbo].[Terceros] Tercero
		ON Tercero.Id = OrdenDeDespacho.IdTercero
	LEFT JOIN [dbo].[Facturas] AS Factura
		ON Factura.Id = OrdenDeDespacho.IdFactura
	JOIN [dbo].Estados AS Estado
		ON Estado.Id = OrdenDeDespacho.IdEstadoActual
	JOIN @idVentaLocalTable O
		ON O.Id = OrdenDeDespacho.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = OrdenDeDespacho.IdEstacion
	WHERE Estaciones.[Guid] = @estacion
END
