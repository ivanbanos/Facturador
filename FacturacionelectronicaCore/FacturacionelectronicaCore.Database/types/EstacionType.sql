CREATE TYPE [dbo].[EstacionType]
	AS TABLE
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
    [Telefono] NVARCHAR(250) NOT NULL
	)
