DECLARE @Prefijo VARCHAR(1) = '1'; -- Change this value to the prefix you want to add
DECLARE @StartConsecutivo INT = 1000; -- Change this to the starting CONSECUTIVO value

-- Create a temp table to store old and new consecutivo values
IF OBJECT_ID('tempdb..#ConsecutivoMap') IS NOT NULL DROP TABLE #ConsecutivoMap;
CREATE TABLE #ConsecutivoMap (
    OldConsecutivo INT,
    NewConsecutivo INT
);

-- Insert mapping of old and new consecutivo values
INSERT INTO #ConsecutivoMap (OldConsecutivo, NewConsecutivo)
SELECT CONSECUTIVO, CAST(@Prefijo + CAST(CONSECUTIVO AS VARCHAR) AS INT)
FROM ventas.dbo.VENTAS
WHERE CONSECUTIVO >= @StartConsecutivo;

-- Update CONSECUTIVO in ventas.dbo.VENTAS
UPDATE ventas.dbo.VENTAS
SET CONSECUTIVO = CAST(@Prefijo + CAST(CONSECUTIVO AS VARCHAR) AS INT)
WHERE CONSECUTIVO >= @StartConsecutivo;

-- Update ventaId in Facturacion_Electronica.dbo.OrdenesDeDespacho using the new CONSECUTIVO values
UPDATE od
SET ventaId = cm.NewConsecutivo
FROM Facturacion_Electronica.dbo.OrdenesDeDespacho od
INNER JOIN #ConsecutivoMap cm ON od.ventaId = cm.OldConsecutivo;

