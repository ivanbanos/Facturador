CREATE PROCEDURE [dbo].[GetUserByGuid]
	@Guid uniqueidentifier
AS
BEGIN
	SELECT Usuario.[Guid], Usuario.Nombre, Usuario.Usuario AS NombreUsuario, Usuario.IdentificacionTercero, Usuario.Activo, Usuario.Rol
	FROM [dbo].[Usuario] AS Usuario
	WHERE Usuario.Guid = @Guid AND Usuario.Activo = 1 AND Usuario.Eliminado = 0
END
