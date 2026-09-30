CREATE PROCEDURE [dbo].[AddFacturasImprimir]
	@Facturas [dbo].[Entity] READONLY
AS
BEGIN
	insert into ObjetoImprimir(IdObjeto, Tipo)
	select Facturas.Id, 'Factura' 
	FROM [dbo].[Facturas] AS Facturas
	JOIN @Facturas AS FacturasTmp
		ON FacturasTmp.[Guid] = Facturas.[Guid]
END