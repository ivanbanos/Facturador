CREATE TABLE [dbo].[CombustiblesEstacion]
(
    [Id] INT NOT NULL PRIMARY KEY IDENTITY,
    [IdEstacion] INT NOT NULL,
    [Combustible] NVARCHAR(100) NOT NULL,
    [Precio] DECIMAL(18,3) NOT NULL,
    [EsGas] BIT NOT NULL CONSTRAINT [DF_CombustiblesEstacion_EsGas] DEFAULT 0,
    [FechaActualizacion] DATETIME NOT NULL CONSTRAINT [DF_CombustiblesEstacion_FechaActualizacion] DEFAULT GETDATE(),
    CONSTRAINT [FK_CombustiblesEstacion_Estaciones] FOREIGN KEY ([IdEstacion]) REFERENCES [dbo].[Estaciones]([Id]),
    CONSTRAINT [UQ_CombustiblesEstacion_IdEstacion_Combustible] UNIQUE ([IdEstacion], [Combustible])
)
