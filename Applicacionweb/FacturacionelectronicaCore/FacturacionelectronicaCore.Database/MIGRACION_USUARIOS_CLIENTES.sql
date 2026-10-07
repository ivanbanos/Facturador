-- =====================================================================================
-- Migracion: usuarios asociados a clientes (terceros) y cambio de contrasena
--  * Agrega dbo.Usuario.IdentificacionTercero (NULL = administrador, valor = cliente)
--  * Actualiza los SP de consulta de usuarios para devolver IdentificacionTercero
--  * Crea dbo.CrearUsuario y dbo.CambiarContrasenaUsuario
-- Script idempotente: se puede ejecutar varias veces.
-- =====================================================================================
SET NOCOUNT ON;
SET XACT_ABORT ON;

PRINT 'Paso 1: Columna dbo.Usuario.IdentificacionTercero';
IF COL_LENGTH('dbo.Usuario', 'IdentificacionTercero') IS NULL
BEGIN
    ALTER TABLE [dbo].[Usuario] ADD [IdentificacionTercero] NVARCHAR(250) NULL;
END;
GO

PRINT 'Paso 2: SP de consulta de usuarios';
GO
CREATE OR ALTER PROCEDURE [dbo].[GetUserGuidByUserNPass]
	@Usuario nvarchar(250),
	@Contrasena nvarchar(MAX)
AS
BEGIN
	SELECT Usuario.[Guid], Usuario.Nombre, Usuario.Usuario AS NombreUsuario, Usuario.IdentificacionTercero
	FROM [dbo].[Usuario] AS Usuario
	WHERE Usuario.Usuario = @Usuario AND Usuario.Contrasena = @Contrasena
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetUserByGuid]
	@Guid uniqueidentifier
AS
BEGIN
	SELECT Usuario.[Guid], Usuario.Nombre, Usuario.Usuario AS NombreUsuario, Usuario.IdentificacionTercero
	FROM [dbo].[Usuario] AS Usuario
	WHERE Usuario.Guid = @Guid
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetUsuarios]
AS
BEGIN
	SELECT [Guid], [Nombre], [Usuario] AS NombreUsuario, [IdentificacionTercero]
	FROM [dbo].[Usuario]
	ORDER BY [Nombre]
END
GO

PRINT 'Paso 3: SP CrearUsuario';
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

	IF EXISTS (SELECT 1 FROM [dbo].[Usuario] WITH (UPDLOCK, HOLDLOCK) WHERE [Usuario] = @Usuario)
	BEGIN
		SELECT 0 AS Resultado;
		RETURN;
	END

	INSERT INTO [dbo].[Usuario] ([Guid], [Nombre], [Usuario], [Contrasena], [IdentificacionTercero])
	VALUES (@Guid, @Nombre, @Usuario, @Contrasena, NULLIF(LTRIM(RTRIM(@IdentificacionTercero)), ''));

	SELECT 1 AS Resultado;
END
GO

PRINT 'Paso 4: SP CambiarContrasenaUsuario';
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
	WHERE [Guid] = @Guid AND [Contrasena] = @ContrasenaActual;

	SELECT @@ROWCOUNT AS Resultado;
END
GO

PRINT 'Migracion MIGRACION_USUARIOS_CLIENTES completada';
