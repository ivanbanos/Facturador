CREATE PROCEDURE [dbo].[AgregarOrdenDespacho]
	@ordenes [dbo].[OrdenesDeDespachoType] readonly,
	@estacion UNIQUEIDENTIFIER
AS
BEGIN
	DECLARE @estadoId INT, @idEstacion int

	SELECT @estadoId = Id FROM Estados WHERE Texto = 'Creada'

	SELECT @idEstacion = Estaciones.Id
			FROM Estaciones 
			WHERE Estaciones.Guid = @estacion


	declare @IdTercero int, @idTipoIdentificacion int
	select @IdTercero = Id from terceros where Identificacion = '222222222222'
	select @idTipoIdentificacion = Id from TipoIdentificacion where Texto ='No especificada'
	if @idTipoIdentificacion is null
	begin
		INSERT INTO tipoTipoIdentificacion(Guid, Texto)
		VALUES(NEWID(), 'No especificada');
		select @idTipoIdentificacion = @@identity
	end
	if @IdTercero is null
	begin
		INSERT INTO Terceros ([Guid], Nombre, Direccion, Telefono, Correo, TipoIdentificacion, Identificacion, IdLocal)
		VALUES(NEWID(), 'CONSUMIDOR FINAL', 'no informado', 'no informado', 'no informado', @idTipoIdentificacion, '222222222222', 1);
		select @IdTercero = @@identity
	end

	INSERT INTO OrdenesDeDespacho([Guid],IdTercero,Combustible,Cantidad,Precio,Total,Descuento,IdInterno,Placa,
	Kilometraje,IdEstadoActual,Surtidor,Cara,Manguera ,[Fecha], FormaDePago, IdLocal, IdVentaLocal, 
	FechaProximoMantenimiento, SubTotal, Vendedor,IdEstacion)

	SELECT NewID(),isnull(t.Id,@IdTercero),o.Combustible,o.Cantidad,o.Precio,o.Total,o.Descuento,o.IdInterno,o.Placa,
	o.Kilometraje,@estadoId,o.Surtidor,o.Cara,o.Manguera ,o.[Fecha], o.FormaDePago, o.IdLocal, o.IdVentaLocal,
	o.FechaProximoMantenimiento, o.SubTotal, o.Vendedor,@idEstacion
	FROM @ordenes o
	left JOIN Terceros t 
		ON t.Identificacion = o.Identificacion
	left join OrdenesDeDespacho on o.IdLocal = OrdenesDeDespacho.IdLocal and OrdenesDeDespacho.IdEstacion = @idEstacion
	where OrdenesDeDespacho.Id is null
END
