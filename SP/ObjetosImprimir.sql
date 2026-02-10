use Facturacion_Electronica
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ObjetoImprimir' and xtype='U')
BEGIN
    create table dbo.[ObjetoImprimir](
    Id INT PRIMARY KEY IDENTITY (1, 1),
    fecha DateTime NOT NULL,
    Isla int NOT NULL,
    Numero int NOT NULL, 
	Objeto varchar(10),
	impreso bit Not null
);
END
    GO
	IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'GetObjetoImprimir')
	DROP PROCEDURE [dbo].GetObjetoImprimir
GO
CREATE procedure [dbo].GetObjetoImprimir
as
begin try
    set nocount on;
	select * from ObjetoImprimir where impreso =100
end try
begin catch
    declare 
        @errorMessage varchar(2000),
        @errorProcedure varchar(255),
        @errorLine int;

    select  
        @errorMessage = error_message(),
        @errorProcedure = error_procedure(),
        @errorLine = error_line();

    raiserror (	N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
end catch;
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'SetObjetoImpreso')
	DROP PROCEDURE [dbo].SetObjetoImpreso
GO
CREATE procedure [dbo].SetObjetoImpreso
( 
    @Id int
)
as
begin try
    set nocount on;
	Update ObjetoImprimir set impreso = 1
end try
begin catch
    declare 
        @errorMessage varchar(2000),
        @errorProcedure varchar(255),
        @errorLine int;

    select  
        @errorMessage = error_message(),
        @errorProcedure = error_procedure(),
        @errorLine = error_line();

    raiserror (	N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
end catch;
GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'AgregarObjetoImprimir')
	DROP PROCEDURE [dbo].AgregarObjetoImprimir
GO
CREATE procedure [dbo].AgregarObjetoImprimir
( 
   @fecha DateTime ,
    @Isla int,
    @Numero int, 
	@Objeto varchar(10)
)
as
begin try
    set nocount on;
	insert into  ObjetoImprimir (fecha, Isla,Numero,Objeto,impreso)
	values(@fecha, @Isla, @Numero, @Objeto,0)
end try
begin catch
    declare 
        @errorMessage varchar(2000),
        @errorProcedure varchar(255),
        @errorLine int;

    select  
        @errorMessage = error_message(),
        @errorProcedure = error_procedure(),
        @errorLine = error_line();

    raiserror (	N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
end catch;
GO