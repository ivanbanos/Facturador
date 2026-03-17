SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    PRINT 'Paso 1: Ajustar dbo.Estaciones (EsGas)';

    IF COL_LENGTH('dbo.Estaciones', 'EsGas') IS NULL
    BEGIN
        ALTER TABLE [dbo].[Estaciones]
        ADD [EsGas] BIT NOT NULL
            CONSTRAINT [DF_Estaciones_EsGas] DEFAULT (0);
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.default_constraints dc
        INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
        INNER JOIN sys.tables t ON t.object_id = c.object_id
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        WHERE s.name = 'dbo'
          AND t.name = 'Estaciones'
          AND c.name = 'EsGas'
    )
    BEGIN
        ALTER TABLE [dbo].[Estaciones]
        ADD CONSTRAINT [DF_Estaciones_EsGas] DEFAULT (0) FOR [EsGas];
    END;

    PRINT 'Paso 2: Recrear tipo dbo.EstacionType y SP dbo.AgregarOActualizarEstacion';

    IF OBJECT_ID('dbo.AgregarOActualizarEstacion', 'P') IS NOT NULL
    BEGIN
        DROP PROCEDURE [dbo].[AgregarOActualizarEstacion];
    END;

    IF TYPE_ID(N'dbo.EstacionType') IS NOT NULL
    BEGIN
        DROP TYPE [dbo].[EstacionType];
    END;

    EXEC(N'
        CREATE TYPE [dbo].[EstacionType] AS TABLE
        (
            [Guid] UNIQUEIDENTIFIER NOT NULL,
            [Direccion] NVARCHAR(250) NOT NULL,
            [linea1] NVARCHAR(250) NULL,
            [linea2] NVARCHAR(250) NULL,
            [linea3] NVARCHAR(250) NULL,
            [linea4] NVARCHAR(250) NULL,
            [Nit] NVARCHAR(250) NOT NULL UNIQUE,
            [Nombre] NVARCHAR(250) NOT NULL,
            [Razon] NVARCHAR(250) NOT NULL,
            [Telefono] NVARCHAR(250) NOT NULL,
            [EsGas] BIT NOT NULL
        );
    ');

    EXEC(N'
        CREATE PROCEDURE [dbo].[AgregarOActualizarEstacion]
            @estaciones [dbo].[EstacionType] READONLY
        AS
        BEGIN
            SET NOCOUNT ON;

            DECLARE @estadoActivoId INT;

            SELECT @estadoActivoId = Id
            FROM Estados
            WHERE Texto = ''Activo'';

            MERGE [dbo].[Estaciones] AS TARGET
            USING (
                SELECT [Guid], [linea1], [linea2], [linea3], [linea4], [Direccion],
                       [Nit], [Nombre], [Razon], [Telefono], [EsGas]
                FROM @estaciones
            ) AS Source ([Guid], [linea1], [linea2], [linea3], [linea4], [Direccion],
                         [Nit], [Nombre], [Razon], [Telefono], [EsGas])
            ON (Source.[Guid] = Target.[Guid])
            WHEN MATCHED THEN
                UPDATE SET
                    Target.[linea1] = Source.[linea1],
                    Target.[linea2] = Source.[linea2],
                    Target.[linea3] = Source.[linea3],
                    Target.[linea4] = Source.[linea4],
                    Target.[Direccion] = Source.[Direccion],
                    Target.[Nit] = Source.[Nit],
                    Target.[Nombre] = Source.[Nombre],
                    Target.[Razon] = Source.[Razon],
                    Target.[Telefono] = Source.[Telefono],
                    Target.[EsGas] = Source.[EsGas]
            WHEN NOT MATCHED BY TARGET THEN
                INSERT ([Guid], [linea1], [linea2], [linea3], [linea4], [Direccion],
                        [Nit], [Nombre], [Razon], [Telefono], [EsGas], [IdEstadoActual])
                VALUES (NEWID(), Source.[linea1], Source.[linea2], Source.[linea3], Source.[linea4], Source.[Direccion],
                        Source.[Nit], Source.[Nombre], Source.[Razon], Source.[Telefono], Source.[EsGas], @estadoActivoId);
        END;
    ');

    PRINT 'Paso 3: Crear/Ajustar dbo.CombustiblesEstacion';

    IF OBJECT_ID('dbo.CombustiblesEstacion', 'U') IS NULL
    BEGIN
        CREATE TABLE [dbo].[CombustiblesEstacion]
        (
            [Id] INT NOT NULL PRIMARY KEY IDENTITY,
            [IdEstacion] INT NOT NULL,
            [Combustible] NVARCHAR(100) NOT NULL,
            [Precio] DECIMAL(18,3) NOT NULL,
            [EsGas] BIT NOT NULL CONSTRAINT [DF_CombustiblesEstacion_EsGas] DEFAULT (0),
            [FechaActualizacion] DATETIME NOT NULL CONSTRAINT [DF_CombustiblesEstacion_FechaActualizacion] DEFAULT (GETDATE())
        );
    END
    ELSE
    BEGIN
        IF COL_LENGTH('dbo.CombustiblesEstacion', 'IdEstacion') IS NULL
            ALTER TABLE [dbo].[CombustiblesEstacion] ADD [IdEstacion] INT NOT NULL DEFAULT (0);

        IF COL_LENGTH('dbo.CombustiblesEstacion', 'Combustible') IS NULL
            ALTER TABLE [dbo].[CombustiblesEstacion] ADD [Combustible] NVARCHAR(100) NOT NULL DEFAULT (N'');

        IF COL_LENGTH('dbo.CombustiblesEstacion', 'Precio') IS NULL
            ALTER TABLE [dbo].[CombustiblesEstacion] ADD [Precio] DECIMAL(18,3) NOT NULL DEFAULT (0);

        IF COL_LENGTH('dbo.CombustiblesEstacion', 'EsGas') IS NULL
            ALTER TABLE [dbo].[CombustiblesEstacion] ADD [EsGas] BIT NOT NULL CONSTRAINT [DF_CombustiblesEstacion_EsGas] DEFAULT (0);

        IF COL_LENGTH('dbo.CombustiblesEstacion', 'FechaActualizacion') IS NULL
            ALTER TABLE [dbo].[CombustiblesEstacion] ADD [FechaActualizacion] DATETIME NOT NULL CONSTRAINT [DF_CombustiblesEstacion_FechaActualizacion] DEFAULT (GETDATE());
    END;

    IF OBJECT_ID('dbo.FK_CombustiblesEstacion_Estaciones', 'F') IS NULL
    BEGIN
        ALTER TABLE [dbo].[CombustiblesEstacion]
        ADD CONSTRAINT [FK_CombustiblesEstacion_Estaciones]
            FOREIGN KEY ([IdEstacion]) REFERENCES [dbo].[Estaciones]([Id]);
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM sys.key_constraints
        WHERE [type] = 'UQ'
          AND [name] = 'UQ_CombustiblesEstacion_IdEstacion_Combustible'
    )
    BEGIN
        ALTER TABLE [dbo].[CombustiblesEstacion]
        ADD CONSTRAINT [UQ_CombustiblesEstacion_IdEstacion_Combustible]
            UNIQUE ([IdEstacion], [Combustible]);
    END;

    PRINT 'Paso 4: Crear/Ajustar SPs de estaciones y combustibles';

    IF OBJECT_ID('dbo.GetEstaciones', 'P') IS NULL
        EXEC(N'CREATE PROCEDURE [dbo].[GetEstaciones] AS BEGIN SET NOCOUNT ON; END;');

    EXEC(N'
        ALTER PROCEDURE [dbo].[GetEstaciones]
        AS
        BEGIN
            SET NOCOUNT ON;

            DECLARE @estadoActivoId INT;

            SELECT @estadoActivoId = Id
            FROM Estados
            WHERE Texto = ''Activo'';

            SELECT [Estaciones].[Guid], [Estaciones].[linea1], [Estaciones].[linea2], [Estaciones].[linea3], [Estaciones].[linea4],
                   [Estaciones].[Direccion], [Estaciones].[Nit], [Estaciones].[Nombre], [Estaciones].[Razon], [Estaciones].[Telefono],
                   [Estaciones].[EsGas], [Estaciones].[IdEstadoActual]
            FROM [dbo].[Estaciones]
            WHERE [Estaciones].[IdEstadoActual] = @estadoActivoId
            ORDER BY [Estaciones].[Id] DESC;
        END;
    ');

    IF OBJECT_ID('dbo.GetEstacion', 'P') IS NULL
        EXEC(N'CREATE PROCEDURE [dbo].[GetEstacion] @IdEstacion UNIQUEIDENTIFIER AS BEGIN SET NOCOUNT ON; END;');

    EXEC(N'
        ALTER PROCEDURE [dbo].[GetEstacion]
            @IdEstacion UNIQUEIDENTIFIER
        AS
        BEGIN
            SET NOCOUNT ON;

            SELECT [Estaciones].[Guid], [Estaciones].[Nit], [Estaciones].[Nombre], [Estaciones].[Direccion], [Estaciones].[Razon],
                   [Estaciones].[linea1], [Estaciones].[linea2], [Estaciones].[linea3], [Estaciones].[linea4], [Estaciones].[Telefono], [Estaciones].[EsGas]
            FROM [dbo].[Estaciones]
            WHERE [Estaciones].[Guid] = @IdEstacion;
        END;
    ');

    IF OBJECT_ID('dbo.GetCombustiblesEstacion', 'P') IS NULL
        EXEC(N'CREATE PROCEDURE [dbo].[GetCombustiblesEstacion] @IdEstacion UNIQUEIDENTIFIER AS BEGIN SET NOCOUNT ON; END;');

    EXEC(N'
        ALTER PROCEDURE [dbo].[GetCombustiblesEstacion]
            @IdEstacion UNIQUEIDENTIFIER
        AS
        BEGIN
            SET NOCOUNT ON;

            SELECT c.[Combustible],
                   c.[Precio],
                   c.[EsGas],
                   c.[FechaActualizacion]
            FROM [dbo].[CombustiblesEstacion] c
            INNER JOIN [dbo].[Estaciones] e ON e.[Id] = c.[IdEstacion]
            WHERE e.[Guid] = @IdEstacion
            ORDER BY c.[Combustible];
        END;
    ');

    IF OBJECT_ID('dbo.UpsertCombustibleEstacion', 'P') IS NULL
        EXEC(N'CREATE PROCEDURE [dbo].[UpsertCombustibleEstacion] @IdEstacion UNIQUEIDENTIFIER, @Combustible NVARCHAR(100), @Precio DECIMAL(18,3), @EsGas BIT AS BEGIN SET NOCOUNT ON; END;');

    EXEC(N'
        ALTER PROCEDURE [dbo].[UpsertCombustibleEstacion]
            @IdEstacion UNIQUEIDENTIFIER,
            @Combustible NVARCHAR(100),
            @Precio DECIMAL(18,3),
            @EsGas BIT
        AS
        BEGIN
            SET NOCOUNT ON;

            DECLARE @IdEstacionInt INT;

            SELECT @IdEstacionInt = [Id]
            FROM [dbo].[Estaciones]
            WHERE [Guid] = @IdEstacion;

            IF @IdEstacionInt IS NULL
            BEGIN
                THROW 50001, ''No existe la estacion para el Guid enviado.'', 1;
            END;

            MERGE [dbo].[CombustiblesEstacion] AS TARGET
            USING (
                SELECT @IdEstacionInt AS [IdEstacion],
                       LTRIM(RTRIM(@Combustible)) AS [Combustible],
                       @Precio AS [Precio],
                       @EsGas AS [EsGas]
            ) AS SOURCE
            ON TARGET.[IdEstacion] = SOURCE.[IdEstacion]
               AND TARGET.[Combustible] = SOURCE.[Combustible]
            WHEN MATCHED THEN
                UPDATE SET
                    TARGET.[Precio] = SOURCE.[Precio],
                    TARGET.[EsGas] = SOURCE.[EsGas],
                    TARGET.[FechaActualizacion] = GETDATE()
            WHEN NOT MATCHED BY TARGET THEN
                INSERT ([IdEstacion], [Combustible], [Precio], [EsGas], [FechaActualizacion])
                VALUES (SOURCE.[IdEstacion], SOURCE.[Combustible], SOURCE.[Precio], SOURCE.[EsGas], GETDATE());
        END;
    ');

    COMMIT TRANSACTION;
    PRINT 'Migracion completada correctamente.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrLine INT = ERROR_LINE();
    DECLARE @ErrNum INT = ERROR_NUMBER();

    RAISERROR('Error en migracion (Linea %d, Numero %d): %s', 16, 1, @ErrLine, @ErrNum, @ErrMsg);
END CATCH;
