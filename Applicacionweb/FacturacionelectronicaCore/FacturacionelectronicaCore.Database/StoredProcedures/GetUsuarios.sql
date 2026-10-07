CREATE PROCEDURE [dbo].[GetUsuarios]
AS
BEGIN
	SELECT [Guid], [Nombre], [Usuario] AS NombreUsuario, [IdentificacionTercero]
	FROM [dbo].[Usuario]
	ORDER BY [Nombre]
END
