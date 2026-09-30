CREATE PROCEDURE [dbo].[SetConsecutivoFacturaPendiente]
	@facturaGuid uniqueidentifier,
	@consecutivoActual int
AS
BEGIN
declare @estadoId int
			select @estadoId = Id
			from Estados
			Where Texto = 'Creada'
	update Facturas set Consecutivo = @consecutivoActual,
	IdEstadoActual = @estadoId
	where Guid = @facturaGuid
END
