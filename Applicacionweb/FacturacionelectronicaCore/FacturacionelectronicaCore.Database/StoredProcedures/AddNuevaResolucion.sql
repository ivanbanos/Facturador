CREATE PROCEDURE [dbo].[AddNuevaResolucion]
	@Resoluciones [dbo].[ResolucionType] READONLY
AS
BEGIN
	DECLARE @idEstadoAnulada INT, @idEstadoActivo INT, @idEstacion INT, @estacion UNIQUEIDENTIFIER
	
	SELECT @idEstadoAnulada = Estados.Id FROM Estados WHERE Texto = 'Anulada'
	SELECT @idEstadoActivo = Estados.Id FROM Estados WHERE Texto = 'Activo'

	UPDATE Resolucion SET IdEstado = @idEstadoAnulada
	From Resolucion
	JOIN Estaciones
		ON Estaciones.Id = Resolucion.IdEstacion
	JOIN @Resoluciones Resoluciones ON Estaciones.[Guid] = Resoluciones.IdEstacion 

	INSERT INTO Resolucion([Guid],ConsecutivoInicial,ConsecutivoFinal,FechaInicial,FechaFinal,IdEstado,ConsecutivoActual,
						   Fecha,[IdEstacion],[Autorizacion],[Habilitada],[Descripcion])
	SELECT NEWID(),ConsecutivoInicial,ConsecutivoFinal,FechaInicial,FechaFinal,@idEstadoActivo,ConsecutivoInicial,
		   GETDATE(),Estaciones.Id,[Autorizacion],[Habilitada],[Descripcion] 
	FROM @Resoluciones Resoluciones
	JOIN Estaciones
		ON Estaciones.[Guid] = Resoluciones.IdEstacion
END
