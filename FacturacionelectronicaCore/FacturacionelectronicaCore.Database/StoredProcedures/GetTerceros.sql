CREATE PROCEDURE [dbo].[GetTerceros]
AS
BEGIN
	SELECT [Terceros].[Guid], Nombre, segundo, apellidos, tipoPersona, responsabilidadTributaria, Municipio, departamento,
	Direccion, PAis, codigoPostal, celular, Telefono, Telefono2, correo2, vendedor, comentarios, Correo, 
		   [Terceros].[TipoIdentificacion], [Terceros].[Identificacion],[Terceros].[IdLocal], TipoIdentificacion.Texto AS DescripcionTipoIdentificacion
	FROM [dbo].[Terceros]
	JOIN [dbo].TipoIdentificacion
		ON [Terceros].TipoIdentificacion = TipoIdentificacion.Id
END
