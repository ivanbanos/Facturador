-- Habilita (@Activo = 1) o inhabilita (@Activo = 0) un usuario. Un usuario inhabilitado no puede iniciar sesion.
-- Resultado: filas actualizadas (0 si no existe o esta eliminado)
CREATE PROCEDURE [dbo].[CambiarEstadoUsuario]
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
