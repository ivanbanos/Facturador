USE [Ventas]
GO
/****** Object:  StoredProcedure [dbo].[GetInfoCupos]    Script Date: 07/05/2024 14:15:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER procedure [dbo].[GetInfoCupos]
as
begin try
    set nocount on;
	select * from CUPO_AUTO
	inner join AUTOMOTO on AUTOMOTO.PLACA = CUPO_AUTO.PLACA
	inner join CLIENTES on CLIENTES.COD_CLI = AUTOMOTO.COD_CLI 
	or  '0'+CLIENTES.COD_CLI = AUTOMOTO.COD_CLI 
	or  '00'+CLIENTES.COD_CLI = AUTOMOTO.COD_CLI

   select * from CLIENTES
	inner join CUPO_CLIE on CLIENTES.COD_CLI = CUPO_CLIE.COD_CLI 
	or  '0'+CLIENTES.COD_CLI = CUPO_CLIE.COD_CLI 
	or  '00'+CLIENTES.COD_CLI = CUPO_CLIE.COD_CLI
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
