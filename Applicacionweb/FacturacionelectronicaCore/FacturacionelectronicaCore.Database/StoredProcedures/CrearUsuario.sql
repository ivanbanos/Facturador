CREATE PROCEDURE [dbo].[CrearUsuario]
	@Guid uniqueidentifier,
	@Nombre nvarchar(50),
	@Usuario nvarchar(50),
	@Contrasena nvarchar(MAX),
	@IdentificacionTercero nvarchar(250) = NULL,
	@Rol nvarchar(30) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	-- Los usuarios eliminados (soft delete) liberan su nombre de usuario
	IF EXISTS (SELECT 1 FROM [dbo].[Usuario] WITH (UPDLOCK, HOLDLOCK) WHERE [Usuario] = @Usuario AND [Eliminado] = 0)
	BEGIN
		SELECT 0 AS Resultado;
		RETURN;
	END

	DECLARE @Identificacion nvarchar(250) = NULLIF(LTRIM(RTRIM(@IdentificacionTercero)), '');

	-- Los clientes no llevan rol: su perfil lo define la identificacion del tercero
	INSERT INTO [dbo].[Usuario] ([Guid], [Nombre], [Usuario], [Contrasena], [IdentificacionTercero], [Rol])
	VALUES (@Guid, @Nombre, @Usuario, @Contrasena, @Identificacion,
		CASE WHEN @Identificacion IS NULL THEN NULLIF(LTRIM(RTRIM(@Rol)), '') END);

	SELECT 1 AS Resultado;
END
