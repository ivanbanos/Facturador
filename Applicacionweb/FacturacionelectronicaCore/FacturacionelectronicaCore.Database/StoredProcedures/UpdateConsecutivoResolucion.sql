CREATE PROCEDURE [dbo].[UpdateConsecutivoResolucion]
AS
	declare @maxConsecutivoTable as table (idResolucion int, maxConsecutivo int)

	insert into @maxConsecutivoTable  (idResolucion, maxConsecutivo)
	select Resolucion.Id, max(Factura.consecutivo)
	from Resolucion
	inner join Estados on Resolucion.IdEstado = Estados.Id
	left join Facturas as Factura On Factura.IdEstacion = Resolucion.IdEstacion
	where Estados.Texto = 'Activo'
	GROUP BY Resolucion.id


	update Resolucion set ConsecutivoActual = maxConsecutivo
	from Resolucion
	inner join @maxConsecutivoTable as mct on Resolucion.Id = mct.idResolucion
RETURN 0
