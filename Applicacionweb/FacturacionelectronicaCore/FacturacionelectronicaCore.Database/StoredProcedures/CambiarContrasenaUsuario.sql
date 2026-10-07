CREATE PROCEDURE [dbo].[CambiarContrasenaUsuario]
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
