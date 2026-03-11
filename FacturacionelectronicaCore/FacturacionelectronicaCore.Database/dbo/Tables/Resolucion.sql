CREATE TABLE [dbo].[Resolucion]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Guid] UNIQUEIDENTIFIER NOT NULL, 
	ConsecutivoInicial INT NOT NULL,
	ConsecutivoFinal INT NOT NULL,
	FechaInicial DateTime NOT NULL,
	FechaFinal DateTime NOT NULL,
	IdEstado Int NOT NULL,
	ConsecutivoActual INT NOT NULL,
	Fecha DateTime NOT NULL,
    [IdEstacion] INT NULL, 
    [Autorizacion] varchar(50) NULL, 
    [Habilitada] bit NULL, 
    [Descripcion] varchar(50) NULL, 
    CONSTRAINT [FK_Resolucion_Estados] FOREIGN KEY ([IdEstado]) REFERENCES [Estados]([Id])
)
