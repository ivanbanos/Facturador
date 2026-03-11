CREATE PROCEDURE [dbo].[HabilitarResolucion]
	@fechaVencimiento DateTime,
	@resolucion UNIQUEIDENTIFIER
AS
BEGIN
	DECLARE @idEstadoHabilitado INT, @idEstacion INT
	

	
	SELECT @idEstadoHabilitado = Id FROM Estados WHERE Texto = 'Habilitada'

	UPDATE Resolucion
		SET Resolucion.IdEstado = @idEstadoHabilitado,
		Resolucion.FechaFinal = @fechaVencimiento,
		Resolucion.Habilitada = 1
	FROM Resolucion
	WHERE Resolucion.[Guid] = @resolucion
END