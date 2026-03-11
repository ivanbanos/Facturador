CREATE PROCEDURE [dbo].[AgregarFactura]
	@facturas [dbo].[FacturaType] readonly,
	@estacion UNIQUEIDENTIFIER
AS
BEGIN

declare @estadoId int, @idEstacion int
			select @estadoId = Id
			from Estados
			Where Texto = 'Creada'

declare @estadoActivoId int
			select @estadoId = Id
			from Estados
			Where Texto = 'Activo'

			declare @IdResolucion int

			SELECT @IdResolucion = Resolucion.Id, @idEstacion = Estaciones.Id
			FROM dbo.Resolucion
			JOIN dbo.Estados
				ON Estados.Id = Resolucion.IdEstado
			JOIN dbo.Estaciones
				ON Estaciones.Id = Resolucion.IdEstacion
			WHERE Estados.Texto = 'Activo'
			AND Estaciones.Guid = @estacion
	
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

	insert into Facturas([Guid],[Consecutivo],IdTercero,Combustible,Cantidad,Precio,Total,Descuento,IdInterno,Placa,
	Kilometraje,IdResolucion,IdEstadoActual,Surtidor,Cara,Manguera ,[Fecha], FormaDePago, IdLocal, IdVentaLocal, 
	FechaProximoMantenimiento, SubTotal, Vendedor, IdEstacion)
	select newID(),f.[Consecutivo],isnull(t.Id,@IdTercero),f.Combustible,f.Cantidad,f.Precio,f.Total,f.Descuento,f.IdInterno,f.Placa,
	f.Kilometraje, @IdResolucion, @estadoId, f.Surtidor, f.Cara, f.Manguera, f.[Fecha], f.FormaDePago, f.IdLocal, f.IdVentaLocal,
	f.FechaProximoMantenimiento, f.SubTotal, f.Vendedor, @idEstacion
	
	from @facturas f
	left join Terceros t on t.Identificacion = f.Identificacion
	left join Facturas on f.IdLocal = Facturas.IdLocal and Facturas.IdEstacion = @idEstacion
	where Facturas.Id is null
END
