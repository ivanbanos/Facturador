CREATE PROCEDURE [dbo].[GetEstacion]
	@IdEstacion UNIQUEIDENTIFIER
AS
BEGIN
	SELECT [Estaciones].[Guid], [Estaciones].[Nit], [Estaciones].[Nombre], [Estaciones].[Direccion], [Estaciones].[Razon],
		   [Estaciones].[linea1], [Estaciones].[linea2],[Estaciones].[linea3], [Estaciones].[linea4], [Estaciones].[Telefono]
	FROM [dbo].[Estaciones]
	WHERE [Estaciones].[Guid] = @IdEstacion
END
