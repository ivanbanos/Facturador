CREATE PROCEDURE [dbo].[BorrarEstacion]
	@Estaciones [dbo].[Entity] READONLY
AS
BEGIN
	DECLARE @IdEstadoBorrado INT;

	SELECT @IdEstadoBorrado = [Estados].Id FROM [dbo].[Estados] WHERE [Estados].Texto = 'Borrado'

	UPDATE Estaciones
		SET [Estaciones].[IdEstadoActual] = @IdEstadoBorrado
	FROM [dbo].[Estaciones] AS Estaciones
	JOIN @Estaciones AS EstacionTmp
		ON EstacionTmp.[Guid] = Estaciones.[Guid]
END
