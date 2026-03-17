CREATE PROCEDURE [dbo].[GetCombustiblesEstacion]
    @IdEstacion UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.Combustible,
           c.Precio,
           c.EsGas,
           c.FechaActualizacion
    FROM dbo.CombustiblesEstacion c
    INNER JOIN dbo.Estaciones e ON e.Id = c.IdEstacion
    WHERE e.[Guid] = @IdEstacion
    ORDER BY c.Combustible;
END
