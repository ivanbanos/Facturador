-- =====================================================================================
-- Migracion: gestion de usuarios (editar, inhabilitar y eliminar con soft delete)
--  * Agrega dbo.Usuario.Activo, dbo.Usuario.Eliminado y dbo.Usuario.FechaEliminacion
--  * Los SP de consulta y login ignoran usuarios inhabilitados / eliminados
--  * Crea dbo.ActualizarUsuario, dbo.CambiarEstadoUsuario y dbo.EliminarUsuario
-- Script idempotente: se puede ejecutar varias veces. Requiere MIGRACION_USUARIOS_CLIENTES.sql.
-- =====================================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT 'Paso 1: Columnas Activo / Eliminado / FechaEliminacion';
IF COL_LENGTH('dbo.Usuario', 'Activo') IS NULL
BEGIN
    ALTER TABLE [dbo].[Usuario] ADD [Activo] BIT NOT NULL CONSTRAINT [DF_Usuario_Activo] DEFAULT (1);
END;
IF COL_LENGTH('dbo.Usuario', 'Eliminado') IS NULL
BEGIN
    ALTER TABLE [dbo].[Usuario] ADD [Eliminado] BIT NOT NULL CONSTRAINT [DF_Usuario_Eliminado] DEFAULT (0);
END;
IF COL_LENGTH('dbo.Usuario', 'FechaEliminacion') IS NULL
BEGIN
    ALTER TABLE [dbo].[Usuario] ADD [FechaEliminacion] DATETIME NULL;
END;
GO

PRINT 'Paso 2: Procedimientos almacenados';
GO
CREATE OR ALTER PROCEDURE [dbo].[GetUsuarios]
AS
BEGIN
	SELECT [Guid], [Nombre], [Usuario] AS NombreUsuario, [IdentificacionTercero], [Activo]
	FROM [dbo].[Usuario]
	WHERE [Eliminado] = 0
	ORDER BY [Nombre]
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetUserByGuid]
	@Guid uniqueidentifier
AS
BEGIN
	SELECT Usuario.[Guid], Usuario.Nombre, Usuario.Usuario AS NombreUsuario, Usuario.IdentificacionTercero, Usuario.Activo
	FROM [dbo].[Usuario] AS Usuario
	WHERE Usuario.Guid = @Guid AND Usuario.Activo = 1 AND Usuario.Eliminado = 0
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetUserGuidByUserNPass]
	@Usuario nvarchar(250),
	@Contrasena nvarchar(MAX)
AS
BEGIN
	SELECT Usuario.[Guid], Usuario.Nombre, Usuario.Usuario AS NombreUsuario, Usuario.IdentificacionTercero, Usuario.Activo
	FROM [dbo].[Usuario] AS Usuario
	WHERE Usuario.Usuario = @Usuario AND Usuario.Contrasena = @Contrasena
		AND Usuario.Activo = 1 AND Usuario.Eliminado = 0
END
GO

CREATE OR ALTER PROCEDURE [dbo].[CrearUsuario]
	@Guid uniqueidentifier,
	@Nombre nvarchar(50),
	@Usuario nvarchar(50),
	@Contrasena nvarchar(MAX),
	@IdentificacionTercero nvarchar(250) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	-- Los usuarios eliminados (soft delete) liberan su nombre de usuario
	IF EXISTS (SELECT 1 FROM [dbo].[Usuario] WITH (UPDLOCK, HOLDLOCK) WHERE [Usuario] = @Usuario AND [Eliminado] = 0)
	BEGIN
		SELECT 0 AS Resultado;
		RETURN;
	END

	INSERT INTO [dbo].[Usuario] ([Guid], [Nombre], [Usuario], [Contrasena], [IdentificacionTercero])
	VALUES (@Guid, @Nombre, @Usuario, @Contrasena, NULLIF(LTRIM(RTRIM(@IdentificacionTercero)), ''));

	SELECT 1 AS Resultado;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[CambiarContrasenaUsuario]
	@Guid uniqueidentifier,
	@ContrasenaActual nvarchar(MAX),
	@ContrasenaNueva nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [dbo].[Usuario]
	SET [Contrasena] = @ContrasenaNueva
	WHERE [Guid] = @Guid AND [Contrasena] = @ContrasenaActual
		AND [Activo] = 1 AND [Eliminado] = 0;

	SELECT @@ROWCOUNT AS Resultado;
END
GO

-- Actualiza nombre, usuario, cliente asociado y (opcional) contrasena.
-- Resultado: 1 = actualizado, 0 = no existe (o esta eliminado), -1 = el nombre de usuario ya lo usa otro
CREATE OR ALTER PROCEDURE [dbo].[ActualizarUsuario]
	@Guid uniqueidentifier,
	@Nombre nvarchar(50),
	@Usuario nvarchar(50),
	@Contrasena nvarchar(MAX) = NULL,
	@IdentificacionTercero nvarchar(250) = NULL
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

	UPDATE [dbo].[Usuario]
	SET [Nombre] = @Nombre,
		[Usuario] = @Usuario,
		[Contrasena] = COALESCE(NULLIF(@Contrasena, ''), [Contrasena]),
		[IdentificacionTercero] = NULLIF(LTRIM(RTRIM(@IdentificacionTercero)), '')
	WHERE [Guid] = @Guid AND [Eliminado] = 0;

	SELECT 1 AS Resultado;
END
GO

-- Habilita (@Activo = 1) o inhabilita (@Activo = 0) un usuario. Un usuario inhabilitado no puede iniciar sesion.
-- Resultado: filas actualizadas (0 si no existe o esta eliminado)
CREATE OR ALTER PROCEDURE [dbo].[CambiarEstadoUsuario]
	@Guid uniqueidentifier,
	@Activo bit
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [dbo].[Usuario]
	SET [Activo] = @Activo
	WHERE [Guid] = @Guid AND [Eliminado] = 0;

	SELECT @@ROWCOUNT AS Resultado;
END
GO

-- Eliminacion logica (soft delete): el registro se conserva pero no aparece ni puede iniciar sesion.
-- Resultado: filas actualizadas (0 si no existe o ya estaba eliminado)
CREATE OR ALTER PROCEDURE [dbo].[EliminarUsuario]
	@Guid uniqueidentifier
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE [dbo].[Usuario]
	SET [Eliminado] = 1,
		[Activo] = 0,
		[FechaEliminacion] = GETDATE()
	WHERE [Guid] = @Guid AND [Eliminado] = 0;

	SELECT @@ROWCOUNT AS Resultado;
END
GO

PRINT 'Migracion MIGRACION_USUARIOS_GESTION completada';
