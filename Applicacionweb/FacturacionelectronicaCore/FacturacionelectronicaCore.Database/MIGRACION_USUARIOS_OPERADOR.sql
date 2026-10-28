-- =====================================================================================
-- Migracion: perfil Operador (igual al Administrador pero sin gestion de usuarios)
--  * Agrega dbo.Usuario.Rol (NULL = Administrador, 'Operador' = operador).
--    Los usuarios con IdentificacionTercero siguen siendo Cliente y no llevan rol.
--  * Los SP de consulta devuelven el rol; CrearUsuario y ActualizarUsuario lo reciben.
-- Script idempotente: se puede ejecutar varias veces.
-- Requiere MIGRACION_USUARIOS_CLIENTES.sql y MIGRACION_USUARIOS_GESTION.sql.
-- Los usuarios existentes no cambian: quedan como Administrador o Cliente igual que antes.
-- =====================================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Los SP de este script usan columnas de las migraciones anteriores: si faltan, se detiene antes de cambiar nada
IF COL_LENGTH('dbo.Usuario', 'IdentificacionTercero') IS NULL
    OR COL_LENGTH('dbo.Usuario', 'Activo') IS NULL
    OR COL_LENGTH('dbo.Usuario', 'Eliminado') IS NULL
BEGIN
    ;THROW 50001, 'Ejecute primero MIGRACION_USUARIOS_CLIENTES.sql y MIGRACION_USUARIOS_GESTION.sql', 1;
END;
GO

PRINT 'Paso 1: Columna dbo.Usuario.Rol';
IF COL_LENGTH('dbo.Usuario', 'Rol') IS NULL
BEGIN
    ALTER TABLE [dbo].[Usuario] ADD [Rol] NVARCHAR(30) NULL;
END;
GO

PRINT 'Paso 2: SP de consulta de usuarios';
GO
CREATE OR ALTER PROCEDURE [dbo].[GetUserGuidByUserNPass]
	@Usuario nvarchar(250),
	@Contrasena nvarchar(MAX)
AS
BEGIN
	SELECT Usuario.[Guid], Usuario.Nombre, Usuario.Usuario AS NombreUsuario, Usuario.IdentificacionTercero, Usuario.Activo, Usuario.Rol
	FROM [dbo].[Usuario] AS Usuario
	WHERE Usuario.Usuario = @Usuario AND Usuario.Contrasena = @Contrasena
		AND Usuario.Activo = 1 AND Usuario.Eliminado = 0
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetUserByGuid]
	@Guid uniqueidentifier
AS
BEGIN
	SELECT Usuario.[Guid], Usuario.Nombre, Usuario.Usuario AS NombreUsuario, Usuario.IdentificacionTercero, Usuario.Activo, Usuario.Rol
	FROM [dbo].[Usuario] AS Usuario
	WHERE Usuario.Guid = @Guid AND Usuario.Activo = 1 AND Usuario.Eliminado = 0
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetUsuarios]
AS
BEGIN
	SELECT [Guid], [Nombre], [Usuario] AS NombreUsuario, [IdentificacionTercero], [Activo], [Rol]
	FROM [dbo].[Usuario]
	WHERE [Eliminado] = 0
	ORDER BY [Nombre]
END
GO

PRINT 'Paso 3: SP CrearUsuario y ActualizarUsuario con rol';
GO
CREATE OR ALTER PROCEDURE [dbo].[CrearUsuario]
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
GO

-- Resultado: 1 = actualizado, 0 = no existe (o esta eliminado), -1 = el nombre de usuario ya lo usa otro
CREATE OR ALTER PROCEDURE [dbo].[ActualizarUsuario]
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
GO

PRINT 'Migracion MIGRACION_USUARIOS_OPERADOR completada';
