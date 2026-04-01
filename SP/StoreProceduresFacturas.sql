GO
 IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'Facturacion_Electronica')
  BEGIN
    CREATE DATABASE [Facturacion_Electronica]


    END
    GO
       USE [Facturacion_Electronica]
    GO

	IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Resoluciones' and xtype='U')
BEGIN
    create table dbo.Resoluciones(
    ResolucionId INT PRIMARY KEY IDENTITY (1, 1),
    descripcion VARCHAR (50) NOT NULL,
    consecutivoInicio int NOT NULL,
    consecutivoFinal int NOT NULL,
    consecutivoActual int NOT NULL,
    fechaInicio DATETIME NOT NULL,
    fechafinal DATETIME NOT NULL,
    estado CHAR(2),
    esPOS CHAR(1),
);
END

GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='terceros' and xtype='U')
BEGIN
    create table dbo.terceros(
    terceroId INT PRIMARY KEY IDENTITY (1, 1),
    tipoIdentificacion int NULL,
    identificacion VARCHAR (50) NOT NULL,
    nombre VARCHAR (50) NULL,
    apellidos VARCHAR (50) NULL,
    telefono VARCHAR (50) NULL,
    correo VARCHAR (50) NULL,
    direccion VARCHAR (50) NULL,
    estado CHAR(2),
	COD_CLI char(15)
);
END

GO
ALTER TABLE
  Terceros
ALTER COLUMN
  identificacion
    VARCHAR(50) NULL;
ALTER TABLE
  Terceros
ALTER COLUMN
  tipoIdentificacion
    int NULL;
ALTER TABLE
  Terceros
ALTER COLUMN
  nombre
    VARCHAR(50) NULL;
ALTER TABLE
  Terceros
ALTER COLUMN
  telefono
    VARCHAR(50) NULL;
ALTER TABLE
  Terceros
ALTER COLUMN
  correo
    VARCHAR(250) NULL;
ALTER TABLE
  Terceros
ALTER COLUMN
  direccion
    VARCHAR(50) NULL;
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TipoIdentificaciones' and xtype='U')
BEGIN
    create table dbo.TipoIdentificaciones(
    TipoIdentificacionId INT PRIMARY KEY IDENTITY (1, 1),

    descripcion VARCHAR (50) NOT NULL,
    estado CHAR(2),
	codigoDian smallint
);
END


GO
ALTER TABLE terceros
ADD FOREIGN KEY (tipoIdentificacion) REFERENCES TipoIdentificaciones(TipoIdentificacionId);
GO

IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
    WHERE
        TABLE_NAME = 'terceros' AND COLUMN_NAME = 'apellidos')
BEGIN
    ALTER TABLE terceros
ADD apellidos VARCHAR(50) NULL;
END;

GO
IF NOT EXISTS (
    SELECT
        *
    FROM
        INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'terceros' AND COLUMN_NAME = 'COD_CLI')
BEGIN
  
  ALTER TABLE terceros
ADD COD_CLI char(15);
END;

GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'TipoIdentificaciones' AND COLUMN_NAME = 'codigoDian')
BEGIN
  ALTER TABLE TipoIdentificaciones
ADD codigoDian smallint;
END;

GO

IF EXISTS (SELECT * FROM sysobjects WHERE name='FacturasPOS' and xtype='U')
BEGIN
    DROP TABLE dbo.FacturasPOS;
END


GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrdenesDeDespacho' and xtype='U')
BEGIN
    create table dbo.OrdenesDeDespacho(
    facturaPOSId INT PRIMARY KEY IDENTITY (1, 1),
    fecha DATETIME NOT NULL,
    resolucionId int NOT NULL,
    consecutivo int NOT NULL,
    ventaId int NOT NULL,
    estado CHAR(2),
	terceroId int,
	reporteEnviado bit default 0,
	turnoEnviado bit default 0,
    turnoguid varchar(50) null,
	Placa varchar(50) null,
	Kilometraje varchar(50) null,
	impresa int default 0,
	consolidadoId int,
	enviadaFacturacion bit default 0,
	enviada bit default 0,
	codigoFormaPago int not null default 4,
    codigoFormaPago2 int null,
    total1 float null,
    total2 float null,
    FOREIGN KEY (resolucionId) REFERENCES dbo.Resoluciones (ResolucionId)
);

END


GO


IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Terceros' AND COLUMN_NAME = 'enviada')
BEGIN
  ALTER TABLE Terceros
ADD enviada bit default 0;
END;

GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'resoluciones' AND COLUMN_NAME = 'tipo')
BEGIN
  
  ALTER TABLE resoluciones
ADD tipo int not null default 0
END;
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CrearFacturaElectronica')
	DROP PROCEDURE [dbo].[CrearFacturaElectronica]
GO
DECLARE @idtipoIdentificaciones int

select @idtipoIdentificaciones = TipoIdentificacionId from TipoIdentificaciones
if @idtipoIdentificaciones is null
begin
    insert into TipoIdentificaciones (descripcion, codigoDian)values('C�dula Ciudadan�a', 1)
	insert into TipoIdentificaciones (descripcion, codigoDian)values('Nit', 2)
	insert into TipoIdentificaciones (descripcion, codigoDian)values('No especificada', 0)
end
GO
IF NOT EXISTS (
  SELECT * FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'numeroTransaccion')
BEGIN
  ALTER TABLE OrdenesDeDespacho ADD numeroTransaccion VARCHAR(50) NULL;
END;
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='configuracionEstacion' and xtype='U')
BEGIN
    
create table dbo.configuracionEstacion(
    configId INT PRIMARY KEY IDENTITY (1, 1),
    descripcion VARCHAR (50) NOT NULL,
    valor VARCHAR (50) NOT NULL
);

INSERT INTO configuracionEstacion(descripcion,valor) values('ClientesCreditosGeneranFactura','SI')
END
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerTercero')
	DROP PROCEDURE [dbo].[ObtenerTercero]
GO
CREATE procedure [dbo].[ObtenerTercero]
( 
    @identificacion CHAR (15) 
)
as
begin try
    set nocount on;
	select terceroId, TipoIdentificaciones.descripcion, tipoIdentificacion, identificacion, nombre, apellidos, telefono, correo, direccion, terceros.estado, COD_CLI 
	from dbo.terceros 
    inner join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
    where REPLACE(@identificacion, ' ', '') = identificacion
    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerTipoIdentificaciones')
	DROP PROCEDURE [dbo].[ObtenerTipoIdentificaciones]
GO
CREATE procedure [dbo].[ObtenerTipoIdentificaciones]
as
begin try
    set nocount on;
	select TipoIdentificacionId,
	codigoDian,
    descripcion,
    estado 
	from TipoIdentificaciones
    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CrearTercero')
	DROP PROCEDURE [dbo].[CrearTercero]
GO
CREATE procedure [dbo].[CrearTercero]
( 
	@terceroId INT = null,
    @tipoIdentificacion int,
    @identificacion VARCHAR (50) ,
    @nombre VARCHAR (50) ,
    @apellidos VARCHAR (50) = null,
    @telefono VARCHAR (50) ,
    @correo VARCHAR (50) ,
    @direccion VARCHAR (50) ,
    @estado CHAR(2),
	@COD_CLI char(15)=null
)
as
begin try
    set nocount on;
	declare @idTerceroCreado int, @identificacionActual VARCHAR (50);
	
		if @nombre like '%CONSUMIDOR%'
		or @nombre like '%FINAL%'
		or @nombre like '%No informado%'
		or @identificacion like '%222222222222%'
		or @nombre is null
		begin
		 select @idTerceroCreado = terceroId from terceros where  identificacion like '%222222222222%'
		end
		else
		begin
		select @idTerceroCreado = terceroId from terceros where
			REPLACE(@identificacion, ' ', '') = identificacion
			if @idTerceroCreado is null
			begin
                INSERT INTO terceros (tipoIdentificacion,identificacion,nombre,apellidos,telefono,correo,direccion,estado,COD_CLI) 
                values(@tipoIdentificacion,REPLACE(@identificacion, ' ', ''),@nombre,@apellidos,@telefono,@correo,@direccion,@estado,@COD_CLI)

				select @idTerceroCreado = @@Identity
			end
			else
			begin
			update terceros
			set
			tipoIdentificacion = @tipoIdentificacion,
			identificacion = REPLACE(@identificacion, ' ', ''),
			nombre = @nombre,
            apellidos = @apellidos,
			telefono = @telefono,
			correo = @correo,
			direccion = @direccion,
			estado = @estado,
			COD_CLI = @COD_CLI,
			enviada = 0
			where @idTerceroCreado = terceroId
		end
		end
    select terceroId, TipoIdentificaciones.descripcion, tipoIdentificacion, identificacion, nombre, apellidos, telefono, correo, direccion, terceros.estado, COD_CLI 
	from dbo.terceros 
    inner join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
    where @idTerceroCreado = terceroId
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerFacturasPorVentas')
	DROP PROCEDURE [dbo].[ObtenerFacturasPorVentas]
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CambiarEstadoTerceroEnviado')
	DROP PROCEDURE [dbo].[CambiarEstadoTerceroEnviado]
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CambiarEstadoFactursEnviada')
	DROP PROCEDURE [dbo].[CambiarEstadoFactursEnviada]
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerVentasPorIds')
	DROP PROCEDURE [dbo].[ObtenerVentasPorIds]
	
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CambiarEstadoFactursEnviadaFacturacion')
	DROP PROCEDURE [dbo].[CambiarEstadoFactursEnviadaFacturacion]
GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ActuralizarFechasReportesEnviadas')
	DROP PROCEDURE [dbo].[ActuralizarFechasReportesEnviadas]
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'SetFacturaCanastillaEnviada')
	DROP PROCEDURE [dbo].[SetFacturaCanastillaEnviada]
GO
IF type_id('[dbo].[ventasIds]') IS NOT NULL
        DROP TYPE [dbo].[ventasIds];
GO
CREATE TYPE [dbo].[ventasIds] AS TABLE(
	[ventaId] [int] NOT NULL
)
GO 
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CambiarEstadoEnviadas')
	DROP PROCEDURE [dbo].[CambiarEstadoEnviadas]
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'AnularFacturasYgenerar')
	DROP PROCEDURE [dbo].[AnularFacturasYgenerar]
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'AnularFacturas')
	DROP PROCEDURE [dbo].[AnularFacturas]
GO
IF type_id('[dbo].[facturasIds]') IS NOT NULL
        DROP TYPE [dbo].[facturasIds];
GO

CREATE TYPE [dbo].[facturasIds] AS TABLE(
	[facturaId] [int] NOT NULL
)
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'AnularFacturas')
	DROP PROCEDURE [dbo].[AnularFacturas]
GO
CREATE procedure [dbo].[AnularFacturas]
(
	@facturasIds [facturasIds] readonly
)
as
begin try
    set nocount on;
	Update OrdenesDeDespacho
	set estado = 'AN'
	from OrdenesDeDespacho
	Inner join @facturasIds fi on fi.facturaId = OrdenesDeDespacho.facturaPOSId


    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturasPorIdFactura')
	DROP PROCEDURE [dbo].[getFacturasPorIdFactura]
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'resoluciones' AND COLUMN_NAME = 'habilitada')
BEGIN
  
  ALTER TABLE resoluciones
ADD habilitada bit not null default 0
END;
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'resoluciones' AND COLUMN_NAME = 'autorizacion')
BEGIN
  
  ALTER TABLE resoluciones
ADD autorizacion varchar(50) not null default 0
END;
GO
IF NOT EXISTS (
    SELECT
        *
    FROM
        INFORMATION_SCHEMA.COLUMNS
    WHERE
        TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'codigoFormaPago2')
BEGIN
ALTER TABLE
    OrdenesDeDespacho ADD codigoFormaPago2 int null
END;
GO
IF NOT EXISTS (
    SELECT
        *
    FROM
        INFORMATION_SCHEMA.COLUMNS
    WHERE
        TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'total1')
BEGIN
ALTER TABLE
    OrdenesDeDespacho ADD total1 float null
END;
GO
IF NOT EXISTS (
    SELECT
        *
    FROM
        INFORMATION_SCHEMA.COLUMNS
    WHERE
        TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'total2')
BEGIN
ALTER TABLE
    OrdenesDeDespacho ADD total2 float null
END;
GO
IF NOT EXISTS (
    SELECT
        *
    FROM
        INFORMATION_SCHEMA.COLUMNS
    WHERE
        TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'turnoguid')
BEGIN
ALTER TABLE
    OrdenesDeDespacho ADD turnoguid varchar(50) null
END;
GO
DECLARE @resolucionId int
SELECT @resolucionId = resolucionId from Resoluciones where estado = 'AC'
if @resolucionId is  null
begin
	insert into Resoluciones (descripcion,consecutivoInicio,consecutivoFinal,
		consecutivoActual, fechaInicio, fechafinal, estado, esPOS, autorizacion)values('POS', 1, 30000, 1, '20200828', 
		'20210828', 'AC', 'S', '18764003223891')
end
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaSinEnviar')
	DROP PROCEDURE [dbo].[getFacturaSinEnviar]
GO
CREATE procedure [dbo].[getFacturaSinEnviar]
as
begin try
    set nocount on;

    declare @facturasTemp as Table(id int)

    insert into @facturasTemp (id)
	select
	top(100)ventaId
	from OrdenesDeDespacho
    where (enviada = 0 or enviada is null)
    and fecha <= DATEADD(MINUTE, -10, GETDATE())
	order by ventaId desc

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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*

	from OrdenesDeDespacho
    inner join @facturasTemp tmp on tmp.id = OrdenesDeDespacho.ventaId
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where OrdenesDeDespacho.estado != 'AN'
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
IF OBJECT_ID(N'[dbo].[BuscarResolucionActiva]') is not null
	DROP PROCEDURE [dbo].[BuscarResolucionActiva]
GO
CREATE procedure [dbo].[BuscarResolucionActiva]
(@DescripcionResolucion varchar(50) = null,
@FechaFinalResolucion datetime = null,
@FechaInicioResolucion datetime = null,
@ConsecutivoInicial int = null,
@ConsecutivoFinal int = null,
@ConsecutivoActual int = null,
@Autorizacion varchar(50) = null,
@Habilitada bit = null,
@Tipo int = null,
@DescripcionResolucionCanastilla varchar(50) = null,
@FechaFinalResolucionCanastilla datetime = null,
@FechaInicioResolucionCanastilla datetime = null,
@ConsecutivoInicialCanastilla int = null,
@ConsecutivoFinalCanastilla int = null,
@ConsecutivoActualCanastilla int = null,
@AutorizacionCanastilla varchar(50) = null,
@HabilitadaCanastilla bit = null,
@TipoCanastilla int = null
)
as
begin try
    set nocount on;
	begin tran 

	declare @mismaResolucion varchar(50)
	select @mismaResolucion=valor from configuracionEstacion where descripcion = 'mismaResolucion'
	if @mismaResolucion is null
	begin
	insert into configuracionEstacion(descripcion, valor) values ('mismaResolucion','SI')
	end

	if @Autorizacion is not null
	begin
		DECLARE @ResolucionId int

		SELECT @ResolucionId = Resoluciones.ResolucionId FROM Resoluciones 
			WHERE Resoluciones.autorizacion = @Autorizacion

		IF @ResolucionId is null
		BEGIN
			
			update resoluciones set estado = 'VE' where esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)

			insert into Resoluciones (descripcion,consecutivoInicio,consecutivoFinal,
		consecutivoActual, fechaInicio, fechafinal, estado, esPOS, autorizacion, habilitada)
		values(@DescripcionResolucion, @ConsecutivoInicial, @ConsecutivoFinal, @ConsecutivoActual, @FechaInicioResolucion, 
		@FechaFinalResolucion, 'AC', 'S', @Autorizacion,@Habilitada)
		END
		ELSE
		BEGIN

			UPDATE Resoluciones
				SET Resoluciones.habilitada = @Habilitada
			FROM Resoluciones
			WHERE Resoluciones.autorizacion = @Autorizacion
		END
	end

	if @AutorizacionCanastilla is not null
	begin

		update configuracionEstacion set valor ='NO' where descripcion = 'mismaResolucion'
	
		DECLARE @ResolucionCanastillaId int

		SELECT @ResolucionCanastillaId = Resoluciones.ResolucionId FROM Resoluciones 
			WHERE Resoluciones.autorizacion = @AutorizacionCanastilla
			
		IF @ResolucionCanastillaId is null
		BEGIN
		update resoluciones set estado = 'VE' where esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 1)

			insert into Resoluciones (descripcion,consecutivoInicio,consecutivoFinal,
		consecutivoActual, fechaInicio, fechafinal, estado, esPOS, autorizacion, habilitada, tipo)
		values(@DescripcionResolucionCanastilla, @ConsecutivoInicialCanastilla, @ConsecutivoFinalCanastilla, @ConsecutivoActualCanastilla, @FechaInicioResolucionCanastilla, 
		@FechaFinalResolucionCanastilla, 'AC', 'S', @AutorizacionCanastilla,@HabilitadaCanastilla,1)
		END
		ELSE
		BEGIN
			UPDATE Resoluciones
				SET Resoluciones.habilitada = @HabilitadaCanastilla
			FROM Resoluciones
			WHERE Resoluciones.autorizacion = @AutorizacionCanastilla
		END
	end
	else
	begin
	update configuracionEstacion set valor ='SI' where descripcion = 'mismaResolucion'
	 end

		select Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada from Resoluciones where esPos = 'S' and estado = 'AC'
    commit tran
end try
begin catch
	
    declare 
        @errorMessage varchar(2000),
        @errorProcedure varchar(255),
        @errorLine int;
	if (@@TRANCOUNT >0)
	begin
		rollback tran
	end
    select  
        @errorMessage = error_message(),
        @errorProcedure = error_procedure(),
        @errorLine = error_line();

    raiserror (	N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
end catch;
GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'BuscarTercerosNoEnviados')
	DROP PROCEDURE [dbo].[BuscarTercerosNoEnviados]
GO
CREATE procedure [dbo].[BuscarTercerosNoEnviados]
as
begin try
    set nocount on;

   declare @tercerosTemp as Table(id int)

    insert into @tercerosTemp (id)
	select 
	terceroId
	from Terceros
    where enviada = 0 or enviada is null
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CambiarConsecutivoActual')
	DROP PROCEDURE [dbo].[CambiarConsecutivoActual]
GO
CREATE procedure [dbo].[CambiarConsecutivoActual]
(
	@consecutivoActual int
)
as
begin try
    
			
		Update Resoluciones
				set consecutivoActual = @consecutivoActual
				where estado = 'AC'
    
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
CREATE procedure [dbo].[CambiarEstadoTerceroEnviado]
(
	@terceros [ventasIds] readonly
)
as
begin try
    set nocount on;
	update Terceros set enviada = 1
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
CREATE procedure [dbo].[CambiarEstadoFactursEnviada]
(
	@facturas [ventasIds] readonly
)
as
begin try
    set nocount on;
	update OrdenesDeDespacho set enviada = 1
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'SetFacturaImpresa')
	DROP PROCEDURE [dbo].[SetFacturaImpresa]
GO
CREATE procedure [dbo].[SetFacturaImpresa]
(
	@ventaid int )
as
begin try
    
			
		update OrdenesDeDespacho
				set impresa = 1
				from OrdenesDeDespacho
				where OrdenesDeDespacho.ventaId = @ventaid
			
		Update OrdenesDeDespacho
				set impresa = 1
				from OrdenesDeDespacho
				where OrdenesDeDespacho.ventaId = @ventaid
    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerFacturasPorImprimir')
	DROP PROCEDURE [dbo].[ObtenerFacturasPorImprimir]
GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'SetFacturaNoImpresa')
	DROP PROCEDURE [dbo].[SetFacturaNoImpresa]
GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CrearFactura')
	DROP PROCEDURE [dbo].[CrearFactura]
GO
CREATE procedure [dbo].[CrearFactura]
( 
    @ventaId int,
	@terceroId int,
	@Placa varchar(50) = null,
	@Kilometraje varchar(50) = null,
	@COD_FOR_PAG smallint,
    @Fecha datetime = null,
    @COD_FOR_PAG_2 smallint = null,
    @total1 float = null,
    @total2 float = null,
    @turnoGuid varchar(50) = null,
    @numeroTransaccion varchar(50) = null
)
as
begin try
    set nocount on;
	declare @ResolucionId int, @consecutivoActual int, @fechafinal DATETIME, @facturaPOSId int, @ConsecutivoFinal int,
	
	@clientesCreditoGeneranFactura VARCHAR (50), @soloGeneraOrdenes VARCHAR (50), @verificarConsecutivo int,
    @OrdenDeDespachoId int, @mismaResolucion VARCHAR (50), @codigoFormaPagoPrincipal smallint;
	
	select @OrdenDeDespachoId = facturaPOSId from OrdenesDeDespacho where ventaId = @ventaId

	select @clientesCreditoGeneranFactura=valor from configuracionEstacion where descripcion = 'ClientesCreditosGeneranFactura'
    
	select @soloGeneraOrdenes=valor from configuracionEstacion where descripcion = 'SoloGeneraOrdenes'

	
	select @mismaResolucion=valor from configuracionEstacion where descripcion = 'mismaResolucion'
	if @Fecha is null
	begin
		select @Fecha = GETDATE()
	END

    select @codigoFormaPagoPrincipal = isnull(@COD_FOR_PAG, 4)
    if @codigoFormaPagoPrincipal is null
    begin
        select @codigoFormaPagoPrincipal = 4
    end

	if @OrdenDeDespachoId is not null 
	begin
		select @OrdenDeDespachoId as facturaPOSId
	end
	else 
	begin
		select @ResolucionId = ResolucionId, @consecutivoActual = consecutivoActual, @fechafinal = fechafinal, @ConsecutivoFinal = consecutivoFinal
		from Resoluciones where esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)

		select @consecutivoActual = isnull(max(consecutivo)+1, @consecutivoActual)
		from OrdenesDeDespacho
		where resolucionId = @ResolucionId
		  and consecutivo > 0

		select @consecutivoActual = case when isnull(max(consecutivo)+1, @consecutivoActual) > @consecutivoActual then isnull(max(consecutivo)+1, @consecutivoActual) else @consecutivoActual end  from FacturasCanastilla where resolucionId = @ResolucionId


		if @fechafinal is null or @fechafinal < GETDATE() or @ConsecutivoFinal <= @consecutivoActual
		begin
			update Resoluciones set estado = 'VE' WHERE esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)
			select @facturaPOSId as facturaPOSId
		end
		else
		begin
            if @soloGeneraOrdenes = 'SI' or (  @clientesCreditoGeneranFactura != 'SI' and @codigoFormaPagoPrincipal !=4)
			begin
			
                insert into OrdenesDeDespacho (fecha,resolucionId,consecutivo,ventaId,estado,terceroid, Placa, Kilometraje, enviada, codigoFormaPago, codigoFormaPago2, total1, total2, turnoguid, numeroTransaccion)
                values(@Fecha, @ResolucionId, 0, @ventaId, 'CR',@terceroId, @Placa, @Kilometraje, 0, @codigoFormaPagoPrincipal, null, null, null, @turnoGuid, @numeroTransaccion)
			
				select @facturaPOSId = SCOPE_IDENTITY()

				select @facturaPOSId as facturaPOSId
				
			end
			else
			begin
			while @facturaPOSId is null
			begin
				select @verificarConsecutivo = null
				select @verificarConsecutivo = consecutivo
				from OrdenesDeDespacho
				where consecutivo = @consecutivoActual
				  and resolucionId = @ResolucionId
				  and consecutivo > 0
				if (@verificarConsecutivo is not null )
				begin 
					update Resoluciones set consecutivoActual = consecutivoActual+1 WHERE esPos = 'S' and estado = 'AC'  and (@mismaResolucion = 'SI' or tipo = 0)
					select @consecutivoActual=consecutivoActual from Resoluciones where esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)
				end
				else
				begin
                    insert into OrdenesDeDespacho (fecha,resolucionId,consecutivo,ventaId,estado,terceroid, Placa, Kilometraje, enviada, codigoFormaPago, codigoFormaPago2, total1, total2, turnoguid, numeroTransaccion)
                    select @Fecha, @ResolucionId, @consecutivoActual, @ventaId, 'CR',@terceroId, @Placa, @Kilometraje, 0, @codigoFormaPagoPrincipal, null, null, null, @turnoGuid, @numeroTransaccion
					from Resoluciones WHERE esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)
			
					select @facturaPOSId = SCOPE_IDENTITY()

					update Resoluciones set consecutivoActual = @consecutivoActual+1 WHERE esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)
				end
			end
			--exec MandarImprimir @ventaId=@ventaId
				select @facturaPOSId as facturaPOSId
			end
		end
	end

	
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerFacturaPorVenta')
	DROP PROCEDURE [dbo].[ObtenerFacturaPorVenta]
GO
CREATE procedure [dbo].[ObtenerFacturaPorVenta]
(
	@ventaId int
)
as
begin try
    set nocount on;


	select top(1)
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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	
	where @ventaId = OrdenesDeDespacho.ventaId
	
	and OrdenesDeDespacho.estado != 'AN'
	union
	select top(1)
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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	
	where @ventaId = OrdenesDeDespacho.ventaId
    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerFacturasPorVentas')
	DROP PROCEDURE [dbo].ObtenerFacturasPorVentas
GO
CREATE procedure [dbo].[ObtenerFacturasPorVentas]
(
	@ventas [ventasIds] readonly
)
as
begin try
    set nocount on;
	select 
	Resoluciones.habilitada as habilitada,
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, OrdenesDeDespacho.[facturaPOSId]
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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	inner join @ventas v on OrdenesDeDespacho.ventaId = v.[ventaId]
	
	where OrdenesDeDespacho.estado != 'AN'
    union
	select 
	Resoluciones.habilitada as habilitada,
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, OrdenesDeDespacho.[facturaPOSId]
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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	inner join @ventas v on OrdenesDeDespacho.ventaId = v.[ventaId]
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaPorConsecutivo')
	DROP PROCEDURE [dbo].getFacturaPorConsecutivo
GO
CREATE procedure [dbo].[getFacturaPorConsecutivo]
(
	@prefijo varchar(50),
	@consecutivo int
)
as
begin try
    set nocount on;
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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where Resoluciones.descripcion = @prefijo
	AND OrdenesDeDespacho.consecutivo = @consecutivo
	union
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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where Resoluciones.descripcion = @prefijo
	AND OrdenesDeDespacho.consecutivo = @consecutivo
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

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'MandarImprimir')
	DROP PROCEDURE [dbo].[MandarImprimir]
GO
CREATE procedure [dbo].[MandarImprimir]
(
	@ventaId int, @veces int )
as
begin try
		declare @impresa int
		
		select @impresa = impresa from OrdenesDeDespacho
				where OrdenesDeDespacho.ventaId = @ventaId

		if @impresa >=0
		begin
		update OrdenesDeDespacho
				set impresa = -1,
				enviada=0
				from OrdenesDeDespacho
				where OrdenesDeDespacho.ventaId = @ventaId
		end
		else begin
		
		update OrdenesDeDespacho
				set impresa = impresa-1,
				enviada=0
				from OrdenesDeDespacho
				where OrdenesDeDespacho.ventaId = @ventaId
		end


		
		select @impresa = impresa from OrdenesDeDespacho
				where OrdenesDeDespacho.ventaId = @ventaId
		if @impresa >=0
		begin
		Update OrdenesDeDespacho
				set impresa = -1,
				enviada=0
				from OrdenesDeDespacho
				where OrdenesDeDespacho.ventaId = @ventaId
		end
		else begin
		
		Update OrdenesDeDespacho
				set impresa = impresa-1,
				enviada=0
				from OrdenesDeDespacho
				where OrdenesDeDespacho.ventaId = @ventaId
		end
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaImprimir')
	DROP PROCEDURE [dbo].[getFacturaImprimir]
GO
CREATE procedure [dbo].[getFacturaImprimir]
as
begin try
    set nocount on;



	select top(1)
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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where OrdenesDeDespacho.impresa <= -1
	union
	select top(1)
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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where OrdenesDeDespacho.impresa <= -1

    
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

declare @soloOrdenes int

select @soloOrdenes=configId from configuracionEstacion where descripcion = 'SoloGeneraOrdenes'

IF @soloOrdenes is null
begin
INSERT INTO configuracionEstacion(descripcion,valor) values('SoloGeneraOrdenes','SI')
end
GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaSinenviadaFacturacion')
	DROP PROCEDURE [dbo].[getFacturaSinenviadaFacturacion]
GO
CREATE procedure [dbo].[getFacturaSinenviadaFacturacion]
as
begin try
    set nocount on;

    declare @facturasTemp as Table(id int)

    insert into @facturasTemp (id)
	select
	top(100)ventaId
	from OrdenesDeDespacho
    where (enviadaFacturacion = 0 or enviadaFacturacion is null)
    and fecha <= DATEADD(MINUTE, -10, GETDATE())
	order by ventaId desc

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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*

	from OrdenesDeDespacho
    inner join @facturasTemp tmp on tmp.id = OrdenesDeDespacho.ventaId
	inner join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	inner join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    inner join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where OrdenesDeDespacho.estado != 'AN'

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

CREATE procedure [dbo].[CambiarEstadoFactursEnviadaFacturacion]
(
	@facturas [ventasIds] readonly
)
as
begin try
    set nocount on;
	update OrdenesDeDespacho set enviadaFacturacion = 1
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
GO
IF OBJECT_ID(N'dbo.ActuralizarTurnoEnviadas', N'P') IS NULL
    EXEC('CREATE PROCEDURE [dbo].[ActuralizarTurnoEnviadas] AS SET NOCOUNT ON;');
GO
ALTER procedure [dbo].[ActuralizarTurnoEnviadas]
(
	@facturas [ventasIds] readonly
)
as
begin try
    set nocount on;
	update OrdenesDeDespacho set turnoEnviado = 1
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
IF OBJECT_ID(N'dbo.GetTurnosPorFecha', N'P') IS NULL
    EXEC('CREATE PROCEDURE [dbo].[GetTurnosPorFecha] AS SET NOCOUNT ON;');
GO
ALTER procedure [dbo].[GetTurnosPorFecha]
(
    @fechaInicio datetime,
    @fechaFin datetime
)
as
begin try
    set nocount on;

    select
        cast(t.FECHA as int) as Id,
        e.NOMBRE as Nombre,
        i.DESCRIPCION as Isla,
        case when t.ESTADO = 'C' then 1 else 0 end as IdEstado,
        Ventas.dbo.Finteger(t.FECHA) + Ventas.dbo.HINTEGER(t.HORA_INI) as FechaApertura,
        case
            when t.ESTADO = 'C' then Ventas.dbo.Finteger(t.FECHA) + Ventas.dbo.HINTEGER(t.HORA_FIN)
            else null
        end as FechaCierre,
        cast(t.NUM_TUR as int) as Numero, FECHA
    from Ventas.dbo.TURN_EST t
    left join Ventas.dbo.EMPLEADO e on e.COD_EMP = t.COD_EMP
    left join Ventas.dbo.ISLAS i on i.COD_ISL = t.COD_ISL
    where Ventas.dbo.Finteger(t.FECHA) >= cast(@fechaInicio as date)
      and Ventas.dbo.Finteger(t.FECHA) < dateadd(day, 1, cast(@fechaFin as date))
    order by t.FECHA desc, t.NUM_TUR desc;
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

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'PrepararRetroactivoTurnosPendientes')
	DROP PROCEDURE [dbo].[PrepararRetroactivoTurnosPendientes]
GO
CREATE procedure [dbo].[PrepararRetroactivoTurnosPendientes]
as
begin try
    set nocount on;

    ;with turnosVentas as (
        select
            v.CONSECUTIVO as ventaId,
                        CONVERT(varchar(36), CONVERT(uniqueidentifier, HASHBYTES('MD5', CONVERT(varchar(20), ISNULL(v.FECHA_REAL, t.FECHA)) + '|' + CONVERT(varchar(20), v.COD_ISL) + '|' + CONVERT(varchar(20), ISNULL(v.NUM_TUR, t.NUM_TUR))))) as turnoGuid
        from Ventas.dbo.VENTAS v
                outer apply (
                        select top(1) ts.FECHA, ts.NUM_TUR
                        from Ventas.dbo.TURN_EST ts
                        where ts.COD_ISL = v.COD_ISL
                            and (
                                        (v.FECHA_REAL is not null and ts.FECHA = v.FECHA_REAL)
                                        or v.FECHA_REAL is null
                                    )
                        order by case when ts.estado != 'C' then 0 else 1 end, ts.FECHA desc, ts.NUM_TUR desc
                ) t
        where v.FECHA_REAL is not null
          and v.COD_ISL is not null
                    and ISNULL(v.NUM_TUR, t.NUM_TUR) is not null
    )
    update f
    set f.turnoguid = tv.turnoGuid
    from OrdenesDeDespacho f
    inner join turnosVentas tv on tv.ventaId = f.ventaId
    where f.turnoguid is null;

    ;with turnosVentas as (
        select
            v.CONSECUTIVO as ventaId,
                        CONVERT(varchar(36), CONVERT(uniqueidentifier, HASHBYTES('MD5', CONVERT(varchar(20), ISNULL(v.FECHA_REAL, t.FECHA)) + '|' + CONVERT(varchar(20), v.COD_ISL) + '|' + CONVERT(varchar(20), ISNULL(v.NUM_TUR, t.NUM_TUR))))) as turnoGuid
        from Ventas.dbo.VENTAS v
                outer apply (
                        select top(1) ts.FECHA, ts.NUM_TUR
                        from Ventas.dbo.TURN_EST ts
                        where ts.COD_ISL = v.COD_ISL
                            and (
                                        (v.FECHA_REAL is not null and ts.FECHA = v.FECHA_REAL)
                                        or v.FECHA_REAL is null
                                    )
                        order by case when ts.estado != 'C' then 0 else 1 end, ts.FECHA desc, ts.NUM_TUR desc
                ) t
        where v.FECHA_REAL is not null
          and v.COD_ISL is not null
                    and ISNULL(v.NUM_TUR, t.NUM_TUR) is not null
    )
    update o
    set o.turnoguid = tv.turnoGuid
    from OrdenesDeDespacho o
    inner join turnosVentas tv on tv.ventaId = o.ventaId
    where o.turnoguid is null;

    update OrdenesDeDespacho
    set turnoEnviado = 0
    where turnoguid is not null
      and (turnoEnviado = 1 or turnoEnviado is null)
      and estado != 'AN';

    update OrdenesDeDespacho
    set turnoEnviado = 0
    where turnoguid is not null
      and (turnoEnviado = 1 or turnoEnviado is null)
      and estado != 'AN';
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
CREATE procedure [dbo].[ActuralizarFechasReportesEnviadas]
(
	@facturas [ventasIds] readonly
)
as
begin try
    set nocount on;
	update OrdenesDeDespacho set reporteEnviado = 1
    from OrdenesDeDespacho
    inner join @facturas f on f.ventaId = OrdenesDeDespacho.ventaId

	update OrdenesDeDespacho set reporteEnviado = 1
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CambiarEstadoFactursEnviadaFacturacion')
	DROP PROCEDURE [dbo].CambiarEstadoFactursEnviadaFacturacion
GO
CREATE procedure [dbo].[CambiarEstadoFactursEnviadaFacturacion]
(
	@facturas [ventasIds] readonly
)
as
begin try
    set nocount on;
	update OrdenesDeDespacho set enviadaFacturacion = 1
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'enviarFacturacionSiigo')
	DROP PROCEDURE [dbo].enviarFacturacionSiigo
GO
CREATE procedure [dbo].[enviarFacturacionSiigo]
(
	@ventaId int
)
as
begin try
    set nocount on;
	update OrdenesDeDespacho set enviadaFacturacion = 1
    from OrdenesDeDespacho
    where @ventaId = OrdenesDeDespacho.ventaId
    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaSiigo')
	DROP PROCEDURE [dbo].getFacturaSiigo
GO
CREATE procedure [dbo].[getFacturaSiigo]
as
begin try
    set nocount on;

	declare @idFacturas as table (id int primary key);

	begin tran;

	update o
	set o.enviadaFacturacion = 0
	output deleted.ventaId into @idFacturas(id)
	from dbo.OrdenesDeDespacho o with (updlock, rowlock)
	where o.enviadaFacturacion = 1;

	commit tran;

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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	from dbo.OrdenesDeDespacho
	inner join @idFacturas idf on idf.id = OrdenesDeDespacho.ventaid
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId;

end try
begin catch
    declare 
        @errorMessage varchar(2000),
        @errorProcedure varchar(255),
        @errorLine int;

	if (@@TRANCOUNT > 0)
	begin
		rollback tran;
	end

    select  
        @errorMessage = error_message(),
        @errorProcedure = error_procedure(),
        @errorLine = error_line();

    raiserror (	N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
end catch;
GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ConvertirAFactura')
	DROP PROCEDURE [dbo].ConvertirAFactura
GO
CREATE procedure [dbo].[ConvertirAFactura]
(
	@ventaId int
)
as
begin try
    set nocount on;

	declare @ResolucionId int,
			@consecutivoActual int,
			@fechafinal DATETIME,
			@facturaPOSId int,
			@ConsecutivoFinal int,
			@verificarConsecutivo int,
			@mismaResolucion VARCHAR (50),
			@loopCounter int = 0,
			@maxIterations int = 1000;

	select @mismaResolucion = valor from configuracionEstacion where descripcion = 'mismaResolucion';
	if @mismaResolucion is null
	begin
		insert into configuracionEstacion(descripcion, valor) values ('mismaResolucion','SI');
		select @mismaResolucion = 'SI';
	end

	select @facturaPOSId = facturaPOSId
	from OrdenesDeDespacho
	where ventaId = @ventaId;

	if @facturaPOSId is null
	begin
		select cast(null as int) as facturaPOSId;
		return;
	end

	if exists(
		select 1
		from OrdenesDeDespacho
		where ventaId = @ventaId
		  and consecutivo > 0
	)
	begin
		select @facturaPOSId as facturaPOSId;
		return;
	end

	set transaction isolation level serializable;
	begin tran;

	select @ResolucionId = ResolucionId,
		   @consecutivoActual = consecutivoActual,
		   @fechafinal = fechafinal,
		   @ConsecutivoFinal = consecutivoFinal
	from Resoluciones with (updlock, holdlock, rowlock)
	where esPos = 'S'
	  and estado = 'AC'
	  and (@mismaResolucion = 'SI' or tipo = 0);

	if @ResolucionId is null
	begin
		rollback tran;
		select @facturaPOSId as facturaPOSId;
		return;
	end

	select @consecutivoActual = isnull(max(consecutivo)+1, @consecutivoActual)
	from OrdenesDeDespacho
	where resolucionId = @ResolucionId
	  and consecutivo > 0;

	select @consecutivoActual = case
		when isnull(max(consecutivo)+1, @consecutivoActual) > @consecutivoActual then isnull(max(consecutivo)+1, @consecutivoActual)
		else @consecutivoActual
	end
	from FacturasCanastilla
	where resolucionId = @ResolucionId;

	if @fechafinal is null or @fechafinal < GETDATE() or @ConsecutivoFinal <= @consecutivoActual
	begin
		update Resoluciones
		set estado = 'VE'
		where esPos = 'S'
		  and estado = 'AC'
		  and (@mismaResolucion = 'SI' or tipo = 0);

		commit tran;
		select @facturaPOSId as facturaPOSId;
		return;
	end

	while @loopCounter < @maxIterations
	begin
		select @verificarConsecutivo = null;
		select @verificarConsecutivo = consecutivo
		from OrdenesDeDespacho
		where consecutivo = @consecutivoActual
		  and resolucionId = @ResolucionId
		  and consecutivo > 0;

		if @verificarConsecutivo is null
		begin
			select @verificarConsecutivo = consecutivo
			from FacturasCanastilla
			where consecutivo = @consecutivoActual
			  and resolucionId = @ResolucionId;
		end

		if @verificarConsecutivo is null
			break;

		update Resoluciones
		set consecutivoActual = consecutivoActual + 1
		where esPos = 'S'
		  and estado = 'AC'
		  and (@mismaResolucion = 'SI' or tipo = 0);

		select @consecutivoActual = consecutivoActual
		from Resoluciones
		where esPos = 'S'
		  and estado = 'AC'
		  and (@mismaResolucion = 'SI' or tipo = 0);

		set @loopCounter = @loopCounter + 1;
	end

	if @loopCounter >= @maxIterations
	begin
		rollback tran;
		raiserror (N'<message>No se encontro consecutivo disponible para la resolucion activa</message>', 16, 1);
		return;
	end

	update OrdenesDeDespacho
	set consecutivo = @consecutivoActual,
		resolucionId = @ResolucionId,
		estado = 'CR'
	where ventaId = @ventaId;

	update Resoluciones
	set consecutivoActual = @consecutivoActual + 1
	where esPos = 'S'
	  and estado = 'AC'
	  and (@mismaResolucion = 'SI' or tipo = 0);

	commit tran;
	select @facturaPOSId as facturaPOSId;
end try
begin catch
    declare
        @errorMessage varchar(2000),
        @errorProcedure varchar(255),
        @errorLine int;

	if (@@TRANCOUNT > 0)
	begin
		rollback tran;
	end

    select
        @errorMessage = error_message(),
        @errorProcedure = error_procedure(),
        @errorLine = error_line();

    raiserror (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
end catch;
GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ConvertirAOrden')
	DROP PROCEDURE [dbo].ConvertirAOrden
GO
CREATE procedure [dbo].[ConvertirAOrden]
(
	@ventaId int
)
as
begin try
    set nocount on;

	declare @facturaPOSId int;

	select @facturaPOSId = facturaPOSId
	from OrdenesDeDespacho
	where ventaId = @ventaId;

	if @facturaPOSId is null
	begin
		select cast(null as int) as facturaPOSId;
		return;
	end

	update OrdenesDeDespacho
	set consecutivo = 0,
		estado = 'CR'
	where ventaId = @ventaId;

	select @facturaPOSId as facturaPOSId;
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

    raiserror (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
end catch;
GO


IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ActualizarFactura')
	DROP PROCEDURE [dbo].[ActualizarFactura]
GO
CREATE procedure [dbo].[ActualizarFactura]
( 
    @facturaPOSId int,
	@Placa varchar(50) = null,
	@Kilometraje varchar(50) = null,
	@codigoFormaPago int = null,
    @codigoFormaPago2 int = null,
    @total1 float = null,
    @total2 float = null,
	@terceroId int = null,
	@ventaId int
)
as
begin try
    set nocount on;


	update OrdenesDeDespacho
	set Placa = @Placa,
	Kilometraje = @Kilometraje,
	impresa = impresa+1,
    enviada = 0,
    codigoFormaPago = @codigoFormaPago,
    codigoFormaPago2 = isnull(@codigoFormaPago2, codigoFormaPago2),
    total1 = isnull(@total1, total1),
    total2 = isnull(@total2, total2),
	terceroId = isnull(@terceroId, terceroId)
	where @facturaPOSId = facturaPOSId
	  and ventaId = @ventaId
	select 'Ok' as result
	


	exec Ventas.dbo.setKilimetrajeVenta @ventaId, @Kilometraje, @Placa, @codigoFormaPago

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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'MandarImprimirConsecutivo')
	DROP PROCEDURE [dbo].[MandarImprimirConsecutivo]
GO
CREATE procedure [dbo].[MandarImprimirConsecutivo]
(
	@consecutivo int )
as
begin try
		declare @impresa int
		
		select @impresa = impresa from OrdenesDeDespacho
				where OrdenesDeDespacho.consecutivo = @consecutivo or OrdenesDeDespacho.ventaid = @consecutivo

		if @impresa >=0
		begin
		update OrdenesDeDespacho
				set impresa = -1,
				enviada=0
				from OrdenesDeDespacho
				where OrdenesDeDespacho.consecutivo = @consecutivo or OrdenesDeDespacho.ventaid = @consecutivo
		end
		else begin
		
		update OrdenesDeDespacho
				set impresa = impresa-1,
				enviada=0
				from OrdenesDeDespacho
				where OrdenesDeDespacho.consecutivo = @consecutivo or OrdenesDeDespacho.ventaid = @consecutivo
		end


		
		select @impresa = impresa from OrdenesDeDespacho
				where OrdenesDeDespacho.ventaId = @consecutivo
		if @impresa >=0
		begin
		Update OrdenesDeDespacho
				set impresa = -1,
				enviada=0
				from OrdenesDeDespacho
				where OrdenesDeDespacho.ventaId = @consecutivo
		end
		else begin
		
		Update OrdenesDeDespacho
				set impresa = impresa-1,
				enviada=0
				from OrdenesDeDespacho
				where OrdenesDeDespacho.ventaId = @consecutivo
		end
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

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'GetFacturaPorIdVenta')
	DROP PROCEDURE [dbo].[GetFacturaPorIdVenta]
GO
CREATE procedure [dbo].[GetFacturaPorIdVenta]
(@idVenta int)
as
begin try
    set nocount on;



	select top(1)
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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where OrdenesDeDespacho.ventaId =@idVenta
	union
	select top(1)
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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where OrdenesDeDespacho.ventaId =@idVenta

    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaSinEnviarTurno')
	DROP PROCEDURE [dbo].[getFacturaSinEnviarTurno]
GO
CREATE procedure [dbo].[getFacturaSinEnviarTurno]
as
begin try
    set nocount on;

    declare @facturasTemp as Table(id int)

    insert into @facturasTemp (id)
	select
	top(100)ventaId
	from OrdenesDeDespacho
    where (turnoEnviado = 0 or turnoEnviado is null)
    and turnoguid is not null
    and fecha <= DATEADD(MINUTE, -10, GETDATE())
	order by ventaId desc

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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*

	from OrdenesDeDespacho
    inner join @facturasTemp tmp on tmp.id = OrdenesDeDespacho.ventaId
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where OrdenesDeDespacho.estado != 'AN'
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaPorConsecutivoORVentaId')
	DROP PROCEDURE [dbo].getFacturaPorConsecutivoORVentaId
GO
CREATE procedure [dbo].[getFacturaPorConsecutivoORVentaId]
(
	@consecutivo int
)
as
begin try
    set nocount on;
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
    ,OrdenesDeDespacho.[codigoFormaPago2]
    ,OrdenesDeDespacho.[total1]
    ,OrdenesDeDespacho.[total2]
    ,OrdenesDeDespacho.[turnoguid]
    ,OrdenesDeDespacho.[numeroTransaccion]
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where OrdenesDeDespacho.ventaId = @consecutivo
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

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Fidelizado' and xtype='U')
BEGIN
    create table dbo.Fidelizado(
    Id INT PRIMARY KEY IDENTITY (1, 1),
    documento VARCHAR (50) NOT NULL,
    puntos float NOT NULL
);
END

GO
drop procedure [dbo].GetFidelizado
GO
CREATE procedure [dbo].GetFidelizado
(@ventaId int)
as
begin try
    set nocount on;
	select *
	from dbo.Fidelizado 
    inner join VentaFidelizada on Fidelizado.documento = VentaFidelizada.identificacion
	where VentaFidelizada.ventaId = @ventaId
    
    
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
drop procedure [dbo].AddFidelizado
GO
CREATE procedure [dbo].AddFidelizado
(@documento varchar(50),@puntos float)
as
begin try
    set nocount on;
	declare @Id int;
	select @Id=Id from fidelizado where @documento = documento
	if @Id is null
	begin
	insert into dbo.Fidelizado (documento, puntos) values(@documento, @puntos)

    end
	
	select @Id=Id from fidelizado where @documento = documento

	update Fidelizado set puntos=@puntos from Fidelizado where  @documento = documento
    
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

GO

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'GetTerceroByQuery')
	DROP PROCEDURE [dbo].[GetTerceroByQuery]
GO
CREATE procedure [dbo].[GetTerceroByQuery]
( 
    @identificacion CHAR (15) 
)
as
begin try
    set nocount on;
	select terceroId, TipoIdentificaciones.descripcion, tipoIdentificacion, identificacion, nombre, apellidos, telefono, correo, direccion, terceros.estado, COD_CLI 
	from dbo.terceros 
    inner join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
    where REPLACE(@identificacion, ' ', '') = identificacion
    
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
GO

	IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='VentaFidelizada' and xtype='U')
BEGIN
    create table dbo.[VentaFidelizada](
    Id INT PRIMARY KEY IDENTITY (1, 1),
    identificacion VARCHAR (50) NOT NULL,
    ventaId int NOT NULL
);
END
    GO
    GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ActualizarFacturaFidelizada')
	DROP PROCEDURE [dbo].[ActualizarFacturaFidelizada]
GO
CREATE procedure [dbo].[ActualizarFacturaFidelizada]
( 
    @identificacion varchar (50) ,
    @ventaId int
)
as
begin try
    set nocount on;
	insert into VentaFidelizada(identificacion, ventaId)
    values(@identificacion, @ventaId)
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
IF COL_LENGTH('ObjetoImprimir', 'Objeto') IS NOT NULL
BEGIN
    ALTER TABLE ObjetoImprimir ALTER COLUMN Objeto varchar(50);
END
GO
	IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'GetObjetoImprimir')
	DROP PROCEDURE [dbo].GetObjetoImprimir
GO
CREATE procedure [dbo].GetObjetoImprimir
as
begin try
    set nocount on;
    select * from ObjetoImprimir where impreso =0 order by Id asc
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
    Update ObjetoImprimir set impreso = 1 where Id = @Id
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
	@Objeto varchar(50)
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



