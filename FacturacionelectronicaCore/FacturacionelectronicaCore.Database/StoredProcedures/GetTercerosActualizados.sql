CREATE PROCEDURE [dbo].[GetTercerosActualizados]
	@estacion UNIQUEIDENTIFIER = null
AS
BEGIN

DECLARE @idTerceroTable as table (idTercero int)

insert into @idTerceroTable(idTercero)
select O.IdTerceroLocalActualizado from [IdTerceroActualizado] O
inner join Estaciones on o.IdEstacion = Estaciones.Id
where @estacion is null or Estaciones.Guid = @estacion 

Delete [IdTerceroActualizado] from [IdTerceroActualizado] O
inner join @idTerceroTable tt on tt.idTercero = O.IdTerceroLocalActualizado

SELECT [Terceros].[Guid], Nombre, segundo, apellidos, tipoPersona, responsabilidadTributaria, Municipio, departamento,
	Direccion, PAis, codigoPostal, celular, Telefono, Telefono2, correo2, vendedor, comentarios, Correo, 
		   [Terceros].[TipoIdentificacion], [Terceros].[Identificacion],[Terceros].[IdLocal], TipoIdentificacion.Texto AS DescripcionTipoIdentificacion
	FROM [dbo].[Terceros]
	JOIN [dbo].TipoIdentificacion
		ON [Terceros].TipoIdentificacion = TipoIdentificacion.Id
	JOIN @idTerceroTable O ON o.idTercero = [Terceros].idTercero

END