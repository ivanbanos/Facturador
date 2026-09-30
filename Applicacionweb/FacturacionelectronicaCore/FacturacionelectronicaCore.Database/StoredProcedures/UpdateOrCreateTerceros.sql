CREATE PROCEDURE [dbo].[UpdateOrCreateTerceros]
	@Tercero dbo.[TerceroType] READONLY
AS
BEGIN
	Declare @IdTextoTipoIdentificcion as table(id int null, texto nvarchar(200) null)


	insert into @IdTextoTipoIdentificcion(id, texto)
	select Distinct TipoIdentificacion.Id,t.DescripcionTipoIdentificacion  FROM @Tercero t
	left join TipoIdentificacion ON TipoIdentificacion.Texto = t.DescripcionTipoIdentificacion

	insert into TipoIdentificacion([Guid],Texto)
	select NEWID(), texto
	from @IdTextoTipoIdentificcion
	where id is null

	MERGE [dbo].[Terceros] AS Target
	USING (SELECT t.[Guid], Nombre, segundo, apellidos, tipoPersona, responsabilidadTributaria, Municipio, departamento,
	Direccion, PAis, codigoPostal, celular, Telefono, Telefono2, correo2, vendedor, comentarios, Correo, TipoIdentificacion.Id, Identificacion, IdLocal FROM @Tercero t
	inner join TipoIdentificacion ON TipoIdentificacion.Texto = t.DescripcionTipoIdentificacion where t.Identificacion <> '' and t.nombre <> '')
		AS Source ([Guid], Nombre, segundo, apellidos, tipoPersona, responsabilidadTributaria, Municipio, departamento,
		Direccion, PAis, codigoPostal, celular, Telefono, Telefono2, correo2, vendedor, comentarios, Correo,
		TipoIdentificacion, Identificacion, IdLocal)
	ON (Source.[Guid] = Target.[Guid] OR (Source.Identificacion = Target.Identificacion))
	WHEN MATCHED THEN
		UPDATE SET Target.Nombre =  isnull(Source.Nombre,'no informado'),
				   Target.segundo = isnull(Source.segundo,'no informado'),
				   Target.apellidos = isnull(Source.apellidos,'no informado'),
				   Target.tipoPersona = isnull(Source.tipoPersona,0),
				   Target.responsabilidadTributaria = isnull(Source.responsabilidadTributaria,0),
				   Target.Municipio = isnull(Source.Municipio,'no informado'),
				   Target.departamento = isnull(Source.departamento,'no informado'),
				   Target.Direccion = isnull(Source.Direccion,'no informado'),
				   Target.PAis = Source.PAis,
				   Target.codigoPostal = Source.codigoPostal,
				   Target.celular = Source.celular,
				   Target.Telefono = Source.Telefono,
				   Target.Telefono2 = Source.Telefono2,
				   Target.comentarios = Source.comentarios,
				   Target.vendedor = Source.vendedor,
				   Target.correo2 = isnull(Source.correo2,'no informado'),
				   Target.Correo = isnull(Source.Correo,'no informado'),
				   Target.IdLocal = Source.IdLocal
	WHEN NOT MATCHED BY TARGET THEN
		INSERT ([Guid], Nombre, segundo, apellidos, tipoPersona, responsabilidadTributaria, Municipio, departamento,
		Direccion, PAis, codigoPostal, celular, Telefono, Telefono2, correo2, vendedor, comentarios, Correo, TipoIdentificacion, Identificacion, IdLocal)
		VALUES(NEWID(),  isnull(Source.Nombre,'no informado'), isnull(Source.segundo,'no informado'),isnull(Source.apellidos,'no informado'),
		isnull(Source.tipoPersona,0),isnull(Source.responsabilidadTributaria,0),isnull(Source.Municipio,'no informado'),
		isnull(Source.departamento,'no informado'),isnull(Source.Direccion,'no informado'),Source.PAis,Source.codigoPostal,
		Source.celular,Source.Telefono,Source.Telefono2, Source.Correo2,Source.vendedor,Source.comentarios, Source.Correo, Source.TipoIdentificacion, Source.Identificacion, Source.IdLocal);

	Insert Into [IdTerceroActualizado] ([IdTerceroLocalActualizado], IdEstacion)
	select IdLocal,  e.Id from @Tercero
	cross join estaciones  e
	where IdLocal is not null
END
