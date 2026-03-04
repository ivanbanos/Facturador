SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @SourceDb SYSNAME = N'BasesInstalada';
DECLARE @TargetDb SYSNAME = N'EstacionSiges';
DECLARE @SchemaName SYSNAME = N'dbo';

IF DB_ID(@SourceDb) IS NULL
BEGIN
    RAISERROR('La base origen no existe: BasesInstalada.', 16, 1);
    RETURN;
END;

IF DB_ID(@TargetDb) IS NULL
BEGIN
    RAISERROR('La base destino no existe: EstacionSiges.', 16, 1);
    RETURN;
END;

IF @SourceDb = @TargetDb
BEGIN
    RAISERROR('La base origen y destino no pueden ser la misma.', 16, 1);
    RETURN;
END;

DECLARE @Tablas TABLE
(
    Orden INT NOT NULL,
    TableName SYSNAME NOT NULL
);

INSERT INTO @Tablas (Orden, TableName)
VALUES
    (1, N'Venta'),
    (2, N'Vehiculos'),
    (3, N'Resoluciones'),
    (4, N'OrdenesDeDespacho');

DECLARE @Resultado TABLE
(
    Tabla SYSNAME NOT NULL,
    TotalOrigen INT NOT NULL,
    TotalDestinoAntes INT NOT NULL,
    PendientesPorMigrar INT NOT NULL,
    RegistrosInsertados INT NOT NULL
);

DECLARE
    @TableName SYSNAME,
    @PkColumn SYSNAME,
    @PkCount INT,
    @HasIdentity BIT,
    @ColumnList NVARCHAR(MAX),
    @TotalOrigen INT,
    @TotalDestinoAntes INT,
    @Pendientes INT,
    @RowsInserted INT,
    @Sql NVARCHAR(MAX);

BEGIN TRY
    SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
    BEGIN TRAN;

    DECLARE cTablas CURSOR LOCAL FAST_FORWARD FOR
        SELECT TableName
        FROM @Tablas
        ORDER BY Orden;

    OPEN cTablas;
    FETCH NEXT FROM cTablas INTO @TableName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @PkColumn = NULL;
        SET @PkCount = 0;
        SET @HasIdentity = 0;
        SET @ColumnList = NULL;
        SET @TotalOrigen = 0;
        SET @TotalDestinoAntes = 0;
        SET @Pendientes = 0;
        SET @RowsInserted = 0;

        SET @Sql = N'
IF NOT EXISTS (
    SELECT 1
    FROM ' + QUOTENAME(@SourceDb) + N'.sys.tables t
    INNER JOIN ' + QUOTENAME(@SourceDb) + N'.sys.schemas s ON s.schema_id = t.schema_id
    WHERE t.name = @TableName AND s.name = @SchemaName
)
BEGIN
    RAISERROR(''No existe la tabla origen: %s'', 16, 1, @TableName);
    RETURN;
END;

IF NOT EXISTS (
    SELECT 1
    FROM ' + QUOTENAME(@TargetDb) + N'.sys.tables t
    INNER JOIN ' + QUOTENAME(@TargetDb) + N'.sys.schemas s ON s.schema_id = t.schema_id
    WHERE t.name = @TableName AND s.name = @SchemaName
)
BEGIN
    RAISERROR(''No existe la tabla destino: %s'', 16, 1, @TableName);
    RETURN;
END;
';

        EXEC sp_executesql
            @Sql,
            N'@TableName SYSNAME, @SchemaName SYSNAME',
            @TableName = @TableName,
            @SchemaName = @SchemaName;

        SET @Sql = N'
SELECT
    @PkCountOut = COUNT(*),
    @PkColumnOut = MAX(c.name)
FROM ' + QUOTENAME(@TargetDb) + N'.sys.key_constraints kc
INNER JOIN ' + QUOTENAME(@TargetDb) + N'.sys.tables t ON t.object_id = kc.parent_object_id
INNER JOIN ' + QUOTENAME(@TargetDb) + N'.sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN ' + QUOTENAME(@TargetDb) + N'.sys.index_columns ic
    ON ic.object_id = kc.parent_object_id
   AND ic.index_id = kc.unique_index_id
INNER JOIN ' + QUOTENAME(@TargetDb) + N'.sys.columns c
    ON c.object_id = ic.object_id
   AND c.column_id = ic.column_id
WHERE kc.type = ''PK''
  AND t.name = @TableName
  AND s.name = @SchemaName;';

        EXEC sp_executesql
            @Sql,
            N'@TableName SYSNAME, @SchemaName SYSNAME, @PkCountOut INT OUTPUT, @PkColumnOut SYSNAME OUTPUT',
            @TableName = @TableName,
            @SchemaName = @SchemaName,
            @PkCountOut = @PkCount OUTPUT,
            @PkColumnOut = @PkColumn OUTPUT;

        IF @PkCount <> 1 OR @PkColumn IS NULL
        BEGIN
            RAISERROR('Cada tabla debe tener PK simple para esta migración segura.', 16, 1);
            RETURN;
        END;

        SET @Sql = N'
SELECT @HasIdentityOut = CASE WHEN EXISTS (
    SELECT 1
    FROM ' + QUOTENAME(@TargetDb) + N'.sys.identity_columns ic
    INNER JOIN ' + QUOTENAME(@TargetDb) + N'.sys.tables t ON t.object_id = ic.object_id
    INNER JOIN ' + QUOTENAME(@TargetDb) + N'.sys.schemas s ON s.schema_id = t.schema_id
    WHERE t.name = @TableName
      AND s.name = @SchemaName
) THEN 1 ELSE 0 END;';

        EXEC sp_executesql
            @Sql,
            N'@TableName SYSNAME, @SchemaName SYSNAME, @HasIdentityOut BIT OUTPUT',
            @TableName = @TableName,
            @SchemaName = @SchemaName,
            @HasIdentityOut = @HasIdentity OUTPUT;

        SET @Sql = N'
    SELECT @ColumnListOut = STUFF((
        SELECT '','' + QUOTENAME(tgtc.name)
        FROM ' + QUOTENAME(@TargetDb) + N'.sys.tables tgtt
        INNER JOIN ' + QUOTENAME(@TargetDb) + N'.sys.schemas tgts ON tgts.schema_id = tgtt.schema_id
        INNER JOIN ' + QUOTENAME(@TargetDb) + N'.sys.columns tgtc ON tgtc.object_id = tgtt.object_id
        INNER JOIN ' + QUOTENAME(@SourceDb) + N'.sys.tables srct ON srct.name = tgtt.name
        INNER JOIN ' + QUOTENAME(@SourceDb) + N'.sys.schemas srcs ON srcs.schema_id = srct.schema_id AND srcs.name = tgts.name
        INNER JOIN ' + QUOTENAME(@SourceDb) + N'.sys.columns srcc ON srcc.object_id = srct.object_id AND srcc.name = tgtc.name
        WHERE tgtt.name = @TableName
          AND tgts.name = @SchemaName
        ORDER BY tgtc.column_id
        FOR XML PATH(''''), TYPE
    ).value(''.'', ''NVARCHAR(MAX)''), 1, 1, '''');';

        EXEC sp_executesql
            @Sql,
            N'@TableName SYSNAME, @SchemaName SYSNAME, @ColumnListOut NVARCHAR(MAX) OUTPUT',
            @TableName = @TableName,
            @SchemaName = @SchemaName,
            @ColumnListOut = @ColumnList OUTPUT;

        IF @ColumnList IS NULL OR LEN(@ColumnList) = 0
        BEGIN
            RAISERROR('No fue posible construir la lista de columnas comunes.', 16, 1);
            RETURN;
        END;

        SET @Sql = N'
SELECT @TotalOrigenOut = COUNT(*)
FROM ' + QUOTENAME(@SourceDb) + N'.' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N';

SELECT @TotalDestinoAntesOut = COUNT(*)
FROM ' + QUOTENAME(@TargetDb) + N'.' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N';

SELECT @PendientesOut = COUNT(*)
FROM ' + QUOTENAME(@SourceDb) + N'.' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N' s
WHERE NOT EXISTS (
    SELECT 1
    FROM ' + QUOTENAME(@TargetDb) + N'.' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N' t
    WHERE t.' + QUOTENAME(@PkColumn) + N' = s.' + QUOTENAME(@PkColumn) + N'
);';

        EXEC sp_executesql
            @Sql,
            N'@TotalOrigenOut INT OUTPUT, @TotalDestinoAntesOut INT OUTPUT, @PendientesOut INT OUTPUT',
            @TotalOrigenOut = @TotalOrigen OUTPUT,
            @TotalDestinoAntesOut = @TotalDestinoAntes OUTPUT,
            @PendientesOut = @Pendientes OUTPUT;

        SET @Sql = N'';

        IF @HasIdentity = 1
        BEGIN
            SET @Sql = @Sql +
                N'SET IDENTITY_INSERT ' + QUOTENAME(@TargetDb) + N'.' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N' ON; ';
        END;

        SET @Sql = @Sql +
            N'INSERT INTO ' + QUOTENAME(@TargetDb) + N'.' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N' (' + @ColumnList + N')
              SELECT ' + @ColumnList + N'
              FROM ' + QUOTENAME(@SourceDb) + N'.' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N' s
              WHERE NOT EXISTS (
                    SELECT 1
                    FROM ' + QUOTENAME(@TargetDb) + N'.' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N' t WITH (UPDLOCK, HOLDLOCK)
                    WHERE t.' + QUOTENAME(@PkColumn) + N' = s.' + QUOTENAME(@PkColumn) + N'
              );
              SELECT @RowsInsertedOut = @@ROWCOUNT;';

        IF @HasIdentity = 1
        BEGIN
            SET @Sql = @Sql +
                N' SET IDENTITY_INSERT ' + QUOTENAME(@TargetDb) + N'.' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) + N' OFF;';
        END;

        EXEC sp_executesql
            @Sql,
            N'@RowsInsertedOut INT OUTPUT',
            @RowsInsertedOut = @RowsInserted OUTPUT;

        INSERT INTO @Resultado (Tabla, TotalOrigen, TotalDestinoAntes, PendientesPorMigrar, RegistrosInsertados)
        VALUES (@TableName, @TotalOrigen, @TotalDestinoAntes, @Pendientes, @RowsInserted);

        FETCH NEXT FROM cTablas INTO @TableName;
    END;

    CLOSE cTablas;
    DEALLOCATE cTablas;

    COMMIT TRAN;

    SELECT Tabla, TotalOrigen, TotalDestinoAntes, PendientesPorMigrar, RegistrosInsertados
    FROM @Resultado
    ORDER BY Tabla;
END TRY
BEGIN CATCH
    IF CURSOR_STATUS('local', 'cTablas') >= -1
    BEGIN
        CLOSE cTablas;
        DEALLOCATE cTablas;
    END;

    BEGIN TRY
        SET IDENTITY_INSERT [EstacionSiges].[dbo].[Venta] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT [EstacionSiges].[dbo].[Vehiculos] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT [EstacionSiges].[dbo].[OrdenesDeDespacho] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    BEGIN TRY
        SET IDENTITY_INSERT [EstacionSiges].[dbo].[Resoluciones] OFF;
    END TRY
    BEGIN CATCH
    END CATCH;

    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorNumber INT = ERROR_NUMBER();
    DECLARE @ErrorLine INT = ERROR_LINE();

    RAISERROR('Error en migracion transaccional. Linea: %d. Detalle: %s (Nro: %d).', 16, 1, @ErrorLine, @ErrorMessage, @ErrorNumber);
    RETURN;
END CATCH;
