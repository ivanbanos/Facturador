CREATE TYPE [dbo].[ResolucionType]
	AS TABLE
	(
	ConsecutivoInicial INT NOT NULL,
	ConsecutivoFinal INT NOT NULL,
	FechaInicial DateTime NOT NULL,
	FechaFinal DateTime NOT NULL,
	IdEstado UNIQUEIDENTIFIER NULL,
	ConsecutivoActual INT NOT NULL,
	Fecha DateTime NULL,
    [IdEstacion] UNIQUEIDENTIFIER NULL, 
    [Autorizacion] varchar(50) NULL, 
    [Habilitada] bit NULL, 
    [Descripcion] varchar(50) NULL
	)
