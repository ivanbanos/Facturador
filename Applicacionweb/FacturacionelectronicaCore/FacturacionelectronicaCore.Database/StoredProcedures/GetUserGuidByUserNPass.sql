CREATE PROCEDURE [dbo].[GetUserGuidByUserNPass]
	@Usuario nvarchar(250),
	@Contrasena nvarchar(MAX)
AS
BEGIN
	SELECT Usuario.[Guid], Usuario.Nombre, Usuario.Usuario AS NombreUsuario, Usuario.IdentificacionTercero, Usuario.Activo, Usuario.Rol
	FROM [dbo].[Usuario] AS Usuario
	WHERE Usuario.Usuario = @Usuario AND Usuario.Contrasena = @Contrasena
		AND Usuario.Activo = 1 AND Usuario.Eliminado = 0
END
