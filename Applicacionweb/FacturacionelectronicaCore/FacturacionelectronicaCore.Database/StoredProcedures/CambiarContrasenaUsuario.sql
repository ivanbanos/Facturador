﻿CREATE PROCEDURE [dbo].[CambiarContrasenaUsuario]
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
