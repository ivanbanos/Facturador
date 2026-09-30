CREATE PROCEDURE [dbo].[UpsertCombustibleEstacion]
    @IdEstacion UNIQUEIDENTIFIER,
    @Combustible NVARCHAR(100),
    @Precio DECIMAL(18,3),
    @EsGas BIT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @idEstacionInt INT;
    SELECT @idEstacionInt = Id FROM dbo.Estaciones WHERE [Guid] = @IdEstacion;

    IF @idEstacionInt IS NULL
    BEGIN
        RAISERROR('La estacion no existe.', 16, 1);
        RETURN;
    END

    MERGE dbo.CombustiblesEstacion AS target
    USING (
        SELECT @idEstacionInt AS IdEstacion,
               LTRIM(RTRIM(@Combustible)) AS Combustible,
               @Precio AS Precio,
               @EsGas AS EsGas
    ) AS source
    ON target.IdEstacion = source.IdEstacion AND target.Combustible = source.Combustible
    WHEN MATCHED THEN
        UPDATE SET target.Precio = source.Precio,
                   target.EsGas = source.EsGas,
                   target.FechaActualizacion = GETDATE()
    WHEN NOT MATCHED THEN
        INSERT (IdEstacion, Combustible, Precio, EsGas, FechaActualizacion)
        VALUES (source.IdEstacion, source.Combustible, source.Precio, source.EsGas, GETDATE());
END
