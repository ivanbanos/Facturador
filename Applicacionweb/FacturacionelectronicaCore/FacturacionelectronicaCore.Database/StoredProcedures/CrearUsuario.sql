CREATE PROCEDURE [dbo].[CrearUsuario]
	@Guid uniqueidentifier,
	@Nombre nvarchar(50),
	@Usuario nvarchar(50),
	@Contrasena nvarchar(MAX),
	@IdentificacionTercero nvarchar(250) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF EXISTS (SELECT 1 FROM [dbo].[Usuario] WITH (UPDLOCK, HOLDLOCK) WHERE [Usuario] = @Usuario)
	BEGIN
		SELECT 0 AS Resultado;
		RETURN;
	END

	INSERT INTO [dbo].[Usuario] ([Guid], [Nombre], [Usuario], [Contrasena], [IdentificacionTercero])
	VALUES (@Guid, @Nombre, @Usuario, @Contrasena, NULLIF(LTRIM(RTRIM(@IdentificacionTercero)), ''));

	SELECT 1 AS Resultado;
END
