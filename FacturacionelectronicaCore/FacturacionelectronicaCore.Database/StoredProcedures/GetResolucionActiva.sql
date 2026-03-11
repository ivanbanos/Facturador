CREATE PROCEDURE [dbo].[GetResolucionActiva]
	@estacion UNIQUEIDENTIFIER
AS
BEGIN
	DECLARE @idEstadoHabilitado INT, @idEstadoActivo INT, @idEstacion INT
	

	SELECT @idEstacion = Id FROM Estaciones WHERE Estaciones.Guid = @estacion
	
	SELECT @idEstadoActivo = Estados.Id FROM Estados WHERE Texto = 'Activo'
	SELECT @idEstadoHabilitado = Estados.Id  FROM Estados WHERE Texto = 'Habilitada'

	SELECT Resolucion.[Guid],ConsecutivoInicial,ConsecutivoFinal,FechaInicial,FechaFinal,Estados.[Guid] as IdEstado, Estados.Texto as Estado,ConsecutivoActual,
	Fecha,Estaciones.[Guid] as [IdEstacion],[Autorizacion],[Habilitada],[Descripcion] FROM Resolucion
	INNER JOIN Estaciones ON Estaciones.Id = Resolucion.IdEstacion
	INNER JOIN Estados ON Estados.Id = Resolucion.IdEstado
	WHERE Estaciones.Guid = @estacion
	AND (Resolucion.IdEstado = @idEstadoActivo OR Resolucion.IdEstado = @idEstadoHabilitado)
END