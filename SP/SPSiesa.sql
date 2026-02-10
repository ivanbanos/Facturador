-- ALTER TABLE
--  Terceros
-- ADD
--  enviadoSiesa
--    bit NULL;
-- GO

--  ALTER TABLE OrdenesDeDespacho
-- 	ADD EnviadaSiesa bit default 0;
-- END
-- GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CambiarEstadoTerceroEnviadoSiesa')
	DROP PROCEDURE [dbo].[CambiarEstadoTerceroEnviadoSiesa]
GO
CREATE procedure [dbo].[CambiarEstadoTerceroEnviadoSiesa]
(
	@terceros [ventasIds] readonly
)
as
begin try
    set nocount on;
	update Terceros set enviadoSiesa = 1
    from Terceros
    inner join @terceros t on t.ventaId = Terceros.terceroId
    
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

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'BuscarTercerosNoEnviadosSiesa')
	DROP PROCEDURE [dbo].[BuscarTercerosNoEnviadosSiesa]
GO
CREATE procedure [dbo].[BuscarTercerosNoEnviadosSiesa]
as
begin try
    set nocount on;

   declare @tercerosTemp as Table(id int)

    insert into @tercerosTemp (id)
	select 
	top(25) terceroId
	from Terceros
    where enviadoSiesa = 0 or enviadoSiesa is null
	--and identificacion is not null

    
    select  terceroId, TipoIdentificaciones.descripcion,  tipoIdentificacion, ISNULL(NULLIF(identificacion, ''), 'No informado') AS identificacion , ISNULL(NULLIF(nombre, ''), 'No informado') AS nombre, ISNULL(NULLIF(telefono, ''), 'No informado') AS telefono, ISNULL(NULLIF(correo, ''), 'No informado') AS correo, ISNULL(NULLIF(direccion, ''), 'No informado') AS direccion, terceros.estado, COD_CLI ,enviada
	from Terceros
    inner join @tercerosTemp tmp on tmp.id = Terceros.terceroId 
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	
    
    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ActuralizarEnviadasSiesa')
	DROP PROCEDURE [dbo].[ActuralizarEnviadasSiesa]
GO
CREATE procedure [dbo].[ActuralizarEnviadasSiesa]
(
	@facturas [ventasIds] readonly
)
as
begin try
    set nocount on;
	update OrdenesDeDespacho set EnviadaSiesa = 1
    from OrdenesDeDespacho
    inner join @facturas f on f.ventaId = OrdenesDeDespacho.ventaId
    
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

--   ALTER TABLE OrdenesDeDespacho
-- 	ADD EnviadaSiesa bit default 1;
	GO
	GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaSinEnviarSiesa')
	DROP PROCEDURE [dbo].[getFacturaSinEnviarSiesa]
GO
CREATE procedure [dbo].[getFacturaSinEnviarSiesa]
as
begin try
    set nocount on;

    declare @facturasTemp as Table(id int)


	insert into @facturasTemp (id)
	select 
	top(10)ventaId
	from OrdenesDeDespacho
     where (enviada =1 and (EnviadaSiesa = 0 OR EnviadaSiesa IS NULL)) ORDER BY ventaId desc

	declare @terceroId int, @tipoIdentificacion int

	select @tipoIdentificacion = TipoIdentificacionId 
			from dbo.TipoIdentificaciones ti
			where ti.descripcion = 'No especificada'

	select @terceroId = t.terceroId from dbo.terceros t
			where t.nombre like '%CONSUMIDOR FINAL%'
			if @terceroId is null
			begin
			insert into dbo.terceros(COD_CLI,correo,direccion,estado,identificacion,nombre,telefono,tipoIdentificacion)
			values(null, 'no informado', 'no informado', 'AC', '222222222222', 'CONSUMIDOR FINAL', 'no informado', @tipoIdentificacion)

			select @terceroId = SCOPE_IDENTITY()
			end
			
	update FacturasPOS set terceroId = @terceroId
	from FacturasPOS
	inner join terceros on FacturasPOS.terceroId = terceros.terceroId
	where terceros.identificacion is null
	update OrdenesDeDespacho set terceroId = @terceroId
	from OrdenesDeDespacho
	inner join terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
	where terceros.identificacion is null

	
	select 
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, OrdenesDeDespacho.[facturaPOSId]
      ,OrdenesDeDespacho.[fecha]
      ,OrdenesDeDespacho.[resolucionId]
      ,OrdenesDeDespacho.[consecutivo]
      ,OrdenesDeDespacho.[ventaId]
      ,OrdenesDeDespacho.[estado]
      ,OrdenesDeDespacho.[terceroId]
      ,OrdenesDeDespacho.[Placa]
      ,OrdenesDeDespacho.[Kilometraje]
      ,OrdenesDeDespacho.[impresa]
      ,OrdenesDeDespacho.[consolidadoId]
      ,OrdenesDeDespacho.[enviada]
      ,OrdenesDeDespacho.[codigoFormaPago]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.numeroTransaccion
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from OrdenesDeDespacho
    inner join @facturasTemp tmp on tmp.id = OrdenesDeDespacho.ventaId 
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
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


	IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Auxiliares' and xtype='U')
BEGIN
    create table dbo.Auxiliares(
    AxiliarId INT PRIMARY KEY IDENTITY (1, 1),
    auxiliar VARCHAR (50) NOT NULL,
    codigoformapago int NOT NULL,
	combustible varchar(50),
    factura bit NOT NULL,
    cruce bit NOT NULL,
);

 insert into Auxiliares(auxiliar, codigoformapago, combustible, factura) 
 values('42950202', 4, 'corriente', 1);
END
GO
-- alter table Auxiliares add cruce bit not null default 0
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'BuscarAuxiliar')
	DROP PROCEDURE [dbo].[BuscarAuxiliar]
GO
CREATE procedure [dbo].[BuscarAuxiliar]
(
@codigoformapago int,
@combustible varchar(50),
@factura bit,
@cruce bit
)
as
begin try
    set nocount on;

   select auxiliar from Auxiliares where @codigoformapago = codigoformapago and @combustible = combustible and @factura = factura
   and @cruce = cruce
    
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