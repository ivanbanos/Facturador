CREATE PROCEDURE [dbo].[GetEstaciones]
AS
BEGIN
	DECLARE @estadoActivoId INT
			SELECT @estadoActivoId = Id
			FROM Estados
			WHERE Texto = 'Activo'

	SELECT [Estaciones].[Guid],[Estaciones].[linea1],[Estaciones].[linea2],[Estaciones].[linea3],[Estaciones].[linea4],
			[Estaciones].[Direccion],[Estaciones].[Nit],[Estaciones].[Nombre],[Estaciones].[Razon],[Estaciones].[Telefono],
			[Estaciones].[IdEstadoActual]
	FROM [dbo].[Estaciones] 
	WHERE [Estaciones].[IdEstadoActual] = @estadoActivoId
	ORDER BY Estaciones.Id DESC
END
