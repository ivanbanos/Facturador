CREATE TABLE [dbo].[TipoIdentificacion]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY,
	[Guid] UNIQUEIDENTIFIER NOT NULL,
	Texto NVARCHAR(200) NOT NULL,
	[CodigoDian] int null,
	[CodigoExterno] NVARCHAR(200) null
)
