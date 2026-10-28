-- Eliminacion logica (soft delete): el registro se conserva pero no aparece ni puede iniciar sesion.
-- Resultado: filas actualizadas (0 si no existe o ya estaba eliminado)
CREATE PROCEDURE [dbo].[EliminarUsuario]
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
