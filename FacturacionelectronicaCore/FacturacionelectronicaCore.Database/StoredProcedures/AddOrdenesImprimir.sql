CREATE PROCEDURE [dbo].[AddOrdenesImprimir]
@Ordenes [dbo].[Entity] READONLY
AS
BEGIN
	insert into ObjetoImprimir(IdObjeto, Tipo)
	select OrdenesDeDespacho.Id, 'Orden' FROM [dbo].OrdenesDeDespacho AS OrdenesDeDespacho
	JOIN @Ordenes AS OrdenesTmp
		ON OrdenesTmp.[Guid] = OrdenesDeDespacho.[Guid]
END