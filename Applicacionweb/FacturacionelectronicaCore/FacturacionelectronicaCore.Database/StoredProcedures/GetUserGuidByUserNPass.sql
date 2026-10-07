CREATE PROCEDURE [dbo].[GetUserGuidByUserNPass]
	@Usuario nvarchar(250),
	@Contrasena nvarchar(MAX)
AS
BEGIN
	SELECT Usuario.[Guid], Usuario.Nombre, Usuario.Usuario AS NombreUsuario, Usuario.IdentificacionTercero
	FROM [dbo].[Usuario] AS Usuario
	WHERE Usuario.Usuario = @Usuario AND Usuario.Contrasena = @Contrasena
END
