USE Facturacion_Electronica;
GO

IF OBJECT_ID(N'dbo.GetVentaFidelizarAutomaticaPorVenta', N'P') IS NULL
    EXEC('CREATE PROCEDURE dbo.GetVentaFidelizarAutomaticaPorVenta AS SET NOCOUNT ON;');
GO

ALTER PROCEDURE [dbo].[GetVentaFidelizarAutomaticaPorVenta]
(@idVenta int)
AS
BEGIN TRY
    SET NOCOUNT ON;

    SELECT TOP (1)
        ISNULL(TRY_CONVERT(float, v.TOTAL), 0) AS ValorVenta,
        '' AS DocumentoFidelizado,
        ISNULL(
            CASE
                WHEN fp.ventaId IS NULL THEN CONVERT(varchar, od.facturaPOSId)
                ELSE r.descripcion + '-' + CONVERT(varchar, fp.consecutivo)
            END,
            ''
        ) AS Factura
    FROM dbo.OrdenesDeDespacho od
    LEFT JOIN Ventas.dbo.VENTAS v ON v.CONSECUTIVO = od.ventaId
    LEFT JOIN dbo.FacturasPOS fp ON fp.ventaId = od.ventaId
    LEFT JOIN dbo.Resoluciones r ON
        (fp.resolucionId IS NOT NULL AND fp.resolucionId = r.ResolucionId)
        OR (od.resolucionId IS NOT NULL AND od.resolucionId = r.ResolucionId)
    WHERE od.ventaId = @idVenta
    ORDER BY od.facturaPOSId DESC;

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT 0 AS ValorVenta, '' AS DocumentoFidelizado, '' AS Factura;
    END
END TRY
BEGIN CATCH
    DECLARE
        @errorMessage varchar(2000),
        @errorProcedure varchar(255),
        @errorLine int;

    SELECT
        @errorMessage = ERROR_MESSAGE(),
        @errorProcedure = ERROR_PROCEDURE(),
        @errorLine = ERROR_LINE();

    RAISERROR(N'<message>Error occurred in %s :: %s :: Line number: %d</message>',
        16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH;
GO
