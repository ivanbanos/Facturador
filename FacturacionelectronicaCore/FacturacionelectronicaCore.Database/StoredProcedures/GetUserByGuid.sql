CREATE PROCEDURE [dbo].[GetUserByGuid]
	@Guid uniqueidentifier
AS
BEGIN
	SELECT Usuario.[Guid], Usuario.Nombre, Usuario.Usuario, Usuario.Contrasena
	FROM [dbo].[usuario] AS Usuario
	WHERE Usuario.Guid = @Guid
END
