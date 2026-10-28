-- Actualiza nombre, usuario, cliente asociado y (opcional) contrasena.
-- Resultado: 1 = actualizado, 0 = no existe (o esta eliminado), -1 = el nombre de usuario ya lo usa otro
CREATE PROCEDURE [dbo].[ActualizarUsuario]
	@Guid uniqueidentifier,
	@Nombre nvarchar(50),
	@Usuario nvarchar(50),
	@Contrasena nvarchar(MAX) = NULL,
	@IdentificacionTercero nvarchar(250) = NULL,
	@Rol nvarchar(30) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF NOT EXISTS (SELECT 1 FROM [dbo].[Usuario] WHERE [Guid] = @Guid AND [Eliminado] = 0)
	BEGIN
		SELECT 0 AS Resultado;
		RETURN;
	END

	IF EXISTS (SELECT 1 FROM [dbo].[Usuario] WITH (UPDLOCK, HOLDLOCK)
		WHERE [Usuario] = @Usuario AND [Guid] <> @Guid AND [Eliminado] = 0)
	BEGIN
		SELECT -1 AS Resultado;
		RETURN;
	END

	DECLARE @Identificacion nvarchar(250) = NULLIF(LTRIM(RTRIM(@IdentificacionTercero)), '');

	-- Los clientes no llevan rol: su perfil lo define la identificacion del tercero
	UPDATE [dbo].[Usuario]
	SET [Nombre] = @Nombre,
		[Usuario] = @Usuario,
		[Contrasena] = COALESCE(NULLIF(@Contrasena, ''), [Contrasena]),
		[IdentificacionTercero] = @Identificacion,
		[Rol] = CASE WHEN @Identificacion IS NULL THEN NULLIF(LTRIM(RTRIM(@Rol)), '') END
	WHERE [Guid] = @Guid AND [Eliminado] = 0;

	SELECT 1 AS Resultado;
END
