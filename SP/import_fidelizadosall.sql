USE fidelizacion;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRAN;

    DECLARE @RutaArchivo NVARCHAR(4000) = N'C:\Users\ivana\Documents\GitHub\Facturador\SP\fidelizadosAll.rpt';
    DECLARE @CentroVentaId INT = 1015;
    DECLARE @TipoDocumentoId INT = 1;
    DECLARE @EstadoId INT = 1;  -- Change if your active estado uses another Id
    DECLARE @SexoId INT = 1;    -- Change if your default sexo uses another Id
    DECLARE @CiudadIdFallback INT = 73001; -- Used when source CiudadId does not exist in dbo.Ciudad
    DECLARE @CelularDefecto NVARCHAR(MAX) = N'';
    DECLARE @DireccionDefecto NVARCHAR(MAX) = N'';

    IF NOT EXISTS (SELECT 1 FROM dbo.CentroVenta WHERE Id = @CentroVentaId)
        THROW 50001, 'CentroVentaId does not exist in dbo.CentroVenta.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Estado WHERE Id = @EstadoId)
        THROW 50002, 'EstadoId does not exist in dbo.Estado.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Sexo WHERE Id = @SexoId)
        THROW 50003, 'SexoId does not exist in dbo.Sexo.', 1;

    IF NOT EXISTS (SELECT 1 FROM dbo.Ciudad WHERE Id = @CiudadIdFallback)
        THROW 50005, 'CiudadIdFallback does not exist in dbo.Ciudad.', 1;

    IF OBJECT_ID('tempdb..#RawLines') IS NOT NULL DROP TABLE #RawLines;
    CREATE TABLE #RawLines
    (
        Line NVARCHAR(4000) NULL
    );

    DECLARE @sql NVARCHAR(MAX) =
        N'BULK INSERT #RawLines
          FROM ''' + REPLACE(@RutaArchivo, '''', '''''') + N'''
          WITH (
              ROWTERMINATOR = ''0x0a'',
              CODEPAGE = ''ACP'',
              DATAFILETYPE = ''char'',
              TABLOCK
          );';

    EXEC (@sql);

    IF OBJECT_ID('tempdb..#Source') IS NOT NULL DROP TABLE #Source;

    ;WITH Clean AS
    (
        SELECT REPLACE(Line, CHAR(13), N'') AS L
        FROM #RawLines
        WHERE Line IS NOT NULL
    ),
    DataLines AS
    (
        SELECT L
        FROM Clean
        WHERE LEN(LTRIM(RTRIM(L))) > 0
          AND LEFT(LTRIM(L), 1) LIKE '[0-9]'
    ),
    Parsed AS
    (
        SELECT
            Documento = NULLIF(LTRIM(RTRIM(SUBSTRING(L, 17, 15))), N''),
            Nombre = NULLIF(LTRIM(RTRIM(SUBSTRING(L, 70, 60))), N''),
            Contrasena = NULLIF(NULLIF(LTRIM(RTRIM(SUBSTRING(L, 169, 32))), N''), N'NULL'),
            Puntos = TRY_CONVERT(REAL, REPLACE(NULLIF(LTRIM(RTRIM(SUBSTRING(L, 131, 13))), N''), N',', N'.')),
            PorcentajePuntos = COALESCE(
                TRY_CONVERT(REAL, REPLACE(NULLIF(LTRIM(RTRIM(SUBSTRING(L, 286, 16))), N''), N',', N'.')),
                0
            ),
            PuntosReservados = TRY_CONVERT(REAL, REPLACE(NULLIF(LTRIM(RTRIM(SUBSTRING(L, 303, 16))), N''), N',', N'.')),
            FechaCreacion = TRY_CONVERT(DATETIME2(7), NULLIF(LTRIM(RTRIM(SUBSTRING(L, 202, 23))), N'')),
            CiudadId = TRY_CONVERT(INT, NULLIF(LTRIM(RTRIM(SUBSTRING(L, 61, 8))), N'')),
            UsuarioId = TRY_CONVERT(INT, NULLIF(LTRIM(RTRIM(SUBSTRING(L, 270, 15))), N'')),
            RowRank = ROW_NUMBER() OVER
            (
                PARTITION BY NULLIF(LTRIM(RTRIM(SUBSTRING(L, 17, 15))), N'')
                ORDER BY TRY_CONVERT(DATETIME2(7), NULLIF(LTRIM(RTRIM(SUBSTRING(L, 202, 23))), N'')) DESC
            )
        FROM DataLines
    )
    SELECT
        Documento,
        Nombre,
        Contrasena,
        Puntos,
        PorcentajePuntos,
        PuntosReservados,
        FechaCreacion,
        CiudadId,
        UsuarioId
    INTO #Source
    FROM Parsed
    WHERE RowRank = 1
      AND Documento IS NOT NULL
      AND Nombre IS NOT NULL
      AND UsuarioId IS NOT NULL;

    IF OBJECT_ID('tempdb..#SourceResolved') IS NOT NULL DROP TABLE #SourceResolved;

    SELECT
        s.Documento,
        s.Nombre,
        s.Contrasena,
        s.Puntos,
        s.PorcentajePuntos,
        s.PuntosReservados,
        s.FechaCreacion,
        s.CiudadId AS CiudadIdOriginal,
        COALESCE(c.Id, @CiudadIdFallback) AS CiudadIdFinal,
        s.UsuarioId,
        CASE WHEN c.Id IS NULL THEN 1 ELSE 0 END AS CiudadCorregida
    INTO #SourceResolved
    FROM #Source s
    LEFT JOIN dbo.Ciudad c
        ON c.Id = s.CiudadId;

    INSERT INTO dbo.Fidelizado
    (
        Documento,
        TipoDocumentoId,
        Nombre,
        Contrasena,
        Puntos,
        PorcentajePuntos,
        PuntosReservados,
        FechaCreacion,
        FechaUltimoReclamo,
        CentroVentaId,
        EstadoId,
        Guid
    )
    SELECT
        s.Documento,
        @TipoDocumentoId,
        s.Nombre,
        s.Contrasena,
        s.Puntos,
        s.PorcentajePuntos,
        s.PuntosReservados,
        s.FechaCreacion,
        NULL,
        @CentroVentaId,
        @EstadoId,
        NEWID()
    FROM #SourceResolved s
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Fidelizado f
        WHERE f.Documento = s.Documento
          AND f.TipoDocumentoId = @TipoDocumentoId
          AND f.CentroVentaId = @CentroVentaId
    );

    DECLARE @InsertadosFidelizado INT = @@ROWCOUNT;

    INSERT INTO dbo.InformacionAdicional
    (
        Telefono,
        Celular,
        Direccion,
        SexoId,
        CiudadId,
        UsuarioId,
        FidelizadoId
    )
    SELECT
        NULL,
        @CelularDefecto,
        @DireccionDefecto,
        @SexoId,
        s.CiudadIdFinal,
        s.UsuarioId,
        f.Id
    FROM #SourceResolved s
    INNER JOIN dbo.Fidelizado f
        ON f.Documento = s.Documento
       AND f.TipoDocumentoId = @TipoDocumentoId
       AND f.CentroVentaId = @CentroVentaId
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.InformacionAdicional ia
        WHERE ia.FidelizadoId = f.Id
    );

    DECLARE @InsertadosInfoAdicional INT = @@ROWCOUNT;
    DECLARE @CiudadesCorregidas INT = (SELECT COUNT(1) FROM #SourceResolved WHERE CiudadCorregida = 1);

    COMMIT;

    SELECT
        @InsertadosFidelizado AS FidelizadoInsertados,
        @InsertadosInfoAdicional AS InformacionAdicionalInsertados,
        @CiudadesCorregidas AS CiudadesCorregidasConFallback,
        @CiudadIdFallback AS CiudadIdFallbackUsado;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK;

    THROW;
END CATCH;
GO
