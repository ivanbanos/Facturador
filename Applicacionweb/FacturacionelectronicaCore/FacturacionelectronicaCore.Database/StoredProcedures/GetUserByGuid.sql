CREATE PROCEDURE [dbo].[GetUserByGuid]
	@Guid uniqueidentifier
AS
BEGIN
	SELECT Usuario.[Guid], Usuario.Nombre, Usuario.Usuario AS NombreUsuario, Usuario.IdentificacionTercero
	FROM [dbo].[Usuario] AS Usuario
	WHERE Usuario.Guid = @Guid
END
