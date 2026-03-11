CREATE PROCEDURE [dbo].[AnularOrdenesDeDespacho]
	@Ordenes [dbo].[Entity] READONLY
AS
BEGIN
	DECLARE @IdEstadoAnulado int;

	SELECT @IdEstadoAnulado = [Estados].Id FROM [dbo].[Estados] WHERE [Estados].Texto = 'Anulado'

	UPDATE OrdenesDeDespacho
		SET OrdenesDeDespacho.IdEstadoActual = @IdEstadoAnulado
	FROM [dbo].[OrdenesDeDespacho] AS OrdenesDeDespacho
	JOIN @Ordenes AS OrdenesTmp
		ON OrdenesTmp.[Guid] = OrdenesDeDespacho.[Guid]
END