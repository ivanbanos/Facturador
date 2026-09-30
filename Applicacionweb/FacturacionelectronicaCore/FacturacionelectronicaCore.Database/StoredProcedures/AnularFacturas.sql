CREATE PROCEDURE [dbo].[AnularFacturas]
	@Facturas [dbo].[Entity] READONLY
AS
BEGIN
	DECLARE @IdEstadoAnulado int;

	SELECT @IdEstadoAnulado = [Estados].Id FROM [dbo].[Estados] WHERE [Estados].Texto = 'Anulado'

	UPDATE Facturas
		SET Facturas.IdEstadoActual = @IdEstadoAnulado
	FROM [dbo].[Facturas] AS Facturas
	JOIN @Facturas AS FacturasTmp
		ON FacturasTmp.[Guid] = Facturas.[Guid]
END