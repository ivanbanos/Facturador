CREATE PROCEDURE [dbo].[GetUsuarios]
AS
BEGIN
	SELECT [Guid], [Nombre], [Usuario] AS NombreUsuario, [IdentificacionTercero], [Activo], [Rol]
	FROM [dbo].[Usuario]
	WHERE [Eliminado] = 0
	ORDER BY [Nombre]
END
