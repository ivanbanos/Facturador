CREATE OR ALTER PROCEDURE ReimprimirFacturaCanastilla
    @consecutivo INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE FacturaCanastilla
    SET impresa = -1
    WHERE consecutivo = @consecutivo;
END
GO
