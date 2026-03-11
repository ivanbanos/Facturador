CREATE TABLE [dbo].[EstadoFactura]
(
	[Id] INT NOT NULL PRIMARY KEY,	
    [Guid] UNIQUEIDENTIFIER NOT NULL, 
	IdEstado INT NOT NULL,
	IdFactura INT NOT NULL,
	Fecha DATETIME NOT NULL, 
    CONSTRAINT [FK_EstadoFactura_Facturas] FOREIGN KEY ([IdFactura]) REFERENCES [Facturas]([Id]), 
    CONSTRAINT [FK_EstadoFactura_Estados] FOREIGN KEY ([IdEstado]) REFERENCES [Estados]([Id])
)
