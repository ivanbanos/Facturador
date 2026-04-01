CREATE PROCEDURE [dbo].[AgregarFactura]
	@facturas [dbo].[FacturaType] readonly,
	@estacion UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ordenes dbo.OrdenesDeDespachoType;

	INSERT INTO @ordenes (
		[Guid],
		IdFactura,
		Identificacion,
		NombreTercero,
		Combustible,
		Cantidad,
		Precio,
		Total,
		Descuento,
		IdInterno,
		Placa,
		Kilometraje,
		IdEstadoActual,
		Surtidor,
		Cara,
		Manguera,
		Fecha,
		Estado,
		IdentificacionTercero,
		FormaDePago,
		IdLocal,
		IdVentaLocal,
		IdTerceroLocal,
		IdEstacion,
		SubTotal,
		FechaProximoMantenimiento,
		Vendedor
	)
	SELECT
		f.[Guid],
		NULL,
		f.Identificacion,
		f.NombreTercero,
		f.Combustible,
		f.Cantidad,
		f.Precio,
		f.Total,
		f.Descuento,
		f.IdInterno,
		f.Placa,
		f.Kilometraje,
		f.IdEstadoActual,
		f.Surtidor,
		f.Cara,
		f.Manguera,
		ISNULL(f.[Fecha], GETDATE()),
		f.Estado,
		f.IdentificacionTercero,
		f.FormaDePago,
		f.IdLocal,
		f.IdVentaLocal,
		f.IdTerceroLocal,
		f.IdEstacion,
		ISNULL(f.SubTotal, 0),
		ISNULL(f.FechaProximoMantenimiento, ISNULL(f.[Fecha], GETDATE())),
		f.Vendedor
	FROM @facturas f;

	EXEC dbo.AgregarOrdenDespacho @ordenes = @ordenes, @estacion = @estacion;
END
