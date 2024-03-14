GO
 IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'EstacionSIGES')
  BEGIN
    CREATE DATABASE [EstacionSIGES]


    END
    GO
       USE [EstacionSIGES]
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

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FacturasPOS' and xtype='U')
BEGIN
    create table dbo.FacturasPOS(
    facturaPOSId INT PRIMARY KEY IDENTITY (1, 1),
    fecha DATETIME NOT NULL,
    resolucionId int NOT NULL,
    consecutivo int NOT NULL,
    ventaId int NOT NULL,
    estado CHAR(2),
	terceroId int,
	reporteEnviado bit default 0,
	turnoEnviado bit default 0,
	Placa varchar(50) null,
	Kilometraje varchar(50) null,
	impresa int default 0,
	consolidadoId int,
	enviadaFacturacion bit default 0,
	enviada bit default 0,
	codigoFormaPago int not null default 4,
    FOREIGN KEY (resolucionId) REFERENCES dbo.Resoluciones (ResolucionId)
);

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
	Placa varchar(50) null,
	Kilometraje varchar(50) null,
	impresa int default 0,
	consolidadoId int,
	enviadaFacturacion bit default 0,
	enviada bit default 0,
	codigoFormaPago int not null default 4,
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
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'terceroId')
BEGIN
  ALTER TABLE FacturasPOS
ADD terceroId int;
END;
GO

IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'reporteEnviado')
BEGIN
  ALTER TABLE FacturasPOS
ADD reporteEnviado bit default 0;
END;
GO


IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'turnoEnviado')
BEGIN
  ALTER TABLE FacturasPOS
ADD turnoEnviado bit default 0;
  ALTER TABLE OrdenDeDespacho
ADD turnoEnviado bit default 0;
END;
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
ALTER TABLE FacturasPOS
ADD FOREIGN KEY (terceroId) REFERENCES terceros(terceroId);

GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CrearFacturaElectronica')
	DROP PROCEDURE [dbo].[CrearFacturaElectronica]
GO

GO
IF type_id('[dbo].[facturaPOSIdType]') IS NOT NULL
        DROP TYPE [dbo].[facturaPOSIdType];
GO
CREATE TYPE [dbo].[facturaPOSIdType] AS TABLE(
	[ventaId] [int] NOT NULL
)

GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'Placa')
BEGIN
  ALTER TABLE FacturasPOS
ADD Placa varchar(50) null;
END;
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'Kilometraje')
BEGIN
  ALTER TABLE FacturasPOS
ADD Kilometraje varchar(50) null;
END;
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
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'impresa')
BEGIN
  ALTER TABLE FacturasPOS
ADD impresa int default 0;
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
	select terceroId, TipoIdentificaciones.descripcion, tipoIdentificacion, identificacion, nombre, telefono, correo, direccion, terceros.estado, COD_CLI 
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
				INSERT INTO terceros (tipoIdentificacion,identificacion,nombre,telefono,correo,direccion,estado,COD_CLI) 
				values(@tipoIdentificacion,REPLACE(@identificacion, ' ', ''),@nombre,@telefono,@correo,@direccion,@estado,@COD_CLI)

				select @idTerceroCreado = @@Identity
			end
			else
			begin
			update terceros
			set
			tipoIdentificacion = @tipoIdentificacion,
			identificacion = REPLACE(@identificacion, ' ', ''),
			nombre = @nombre,
			telefono = @telefono,
			correo = @correo,
			direccion = @direccion,
			estado = @estado,
			COD_CLI = @COD_CLI,
			enviada = 0
			where @idTerceroCreado = terceroId
		end
		end
	select terceroId, TipoIdentificaciones.descripcion, tipoIdentificacion, identificacion, nombre, telefono, correo, direccion, terceros.estado, COD_CLI 
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
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'consolidadoId')
BEGIN
  ALTER TABLE FacturasPOS
ADD consolidadoId int;
END;
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
	Update FacturasPOS
	set estado = 'AN'
	from FacturasPOS
	Inner join @facturasIds fi on fi.facturaId = FacturasPOS.facturaPOSId


    
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
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'enviada')
BEGIN
  ALTER TABLE FacturasPOS
ADD enviada bit default 0;
END;
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'codigoFormaPago')
BEGIN
ALTER TABLE
  FacturasPOS ADD codigoFormaPago int not null default 4
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
	from FacturasPOS
    where enviada = 0 or enviada is null
	order by ventaId desc

	insert into @facturasTemp (id)
	select 
	top(100)ventaId
	from OrdenesDeDespacho
    where enviada = 0 or enviada is null
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
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasPOS.[facturaPOSId]
      ,FacturasPOS.[fecha]
      ,FacturasPOS.[resolucionId]
      ,FacturasPOS.[consecutivo]
      ,FacturasPOS.[ventaId]
      ,FacturasPOS.[estado]
      ,FacturasPOS.[terceroId]
      ,FacturasPOS.[Placa]
      ,FacturasPOS.[Kilometraje]
      ,FacturasPOS.[impresa]
      ,FacturasPOS.[consolidadoId]
      ,FacturasPOS.[enviada]
      ,FacturasPOS.[codigoFormaPago]
      ,FacturasPOS.[reporteEnviado]
      ,FacturasPOS.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from FacturasPOS
    inner join @facturasTemp tmp on tmp.id = FacturasPOS.ventaId 
	left join dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	
	where FacturasPOS.estado != 'AN'
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
      ,OrdenesDeDespacho.[reporteEnviado]
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
	update FacturasPOS set enviada = 1
    from FacturasPOS
    inner join @facturas f on f.ventaId = FacturasPOS.ventaId
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
    
			
		Update FacturasPOS
				set impresa = 1
				from FacturasPOS
				where FacturasPOS.ventaId = @ventaid
			
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
	@Fecha datetime = null
)
as
begin try
    set nocount on;
	declare @ResolucionId int, @consecutivoActual int, @fechafinal DATETIME, @facturaPOSId int, @ConsecutivoFinal int,
	
	@clientesCreditoGeneranFactura VARCHAR (50), @soloGeneraOrdenes VARCHAR (50), @verificarConsecutivo int,
	@OrdenDeDespachoId int, @mismaResolucion VARCHAR (50);
	
	select @facturaPOSId = facturaPOSId from FacturasPOS where ventaId = @ventaId
	select @OrdenDeDespachoId = facturaPOSId from OrdenesDeDespacho where ventaId = @ventaId

	select @clientesCreditoGeneranFactura=valor from configuracionEstacion where descripcion = 'ClientesCreditosGeneranFactura'
    
	select @soloGeneraOrdenes=valor from configuracionEstacion where descripcion = 'SoloGeneraOrdenes'

	
	select @mismaResolucion=valor from configuracionEstacion where descripcion = 'mismaResolucion'
	if @Fecha is null
	begin
		select @Fecha = GETDATE()
	END

	if @facturaPOSId is not null 
	begin
		select @facturaPOSId as facturaPOSId
	end
	else if @OrdenDeDespachoId is not null 
	begin
		select @OrdenDeDespachoId as facturaPOSId
	end
	else 
	begin
		select @ResolucionId = ResolucionId, @consecutivoActual = consecutivoActual, @fechafinal = fechafinal, @ConsecutivoFinal = consecutivoFinal
		from Resoluciones where esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)

		select @consecutivoActual = isnull(max(consecutivo)+1, @consecutivoActual) from FacturasPOS where resolucionId = @ResolucionId

		select @consecutivoActual = case when isnull(max(consecutivo)+1, @consecutivoActual) > @consecutivoActual then isnull(max(consecutivo)+1, @consecutivoActual) else @consecutivoActual end  from FacturasCanastilla where resolucionId = @ResolucionId


		if @fechafinal is null or @fechafinal < GETDATE() or @ConsecutivoFinal <= @consecutivoActual
		begin
			update Resoluciones set estado = 'VE' WHERE esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)
			select @facturaPOSId as facturaPOSId
		end
		else
		begin
			if @soloGeneraOrdenes = 'SI' or (  @clientesCreditoGeneranFactura != 'SI' and @COD_FOR_PAG !=4)
			begin
			
				insert into OrdenesDeDespacho (fecha,resolucionId,consecutivo,ventaId,estado,terceroid, Placa, Kilometraje, enviada, codigoFormaPago)
				values(@Fecha, @ResolucionId, 0, @ventaId, 'CR',@terceroId, @Placa, @Kilometraje, 0, @COD_FOR_PAG)
			
				select @facturaPOSId = SCOPE_IDENTITY()

				select @facturaPOSId as facturaPOSId
				
			end
			else
			begin
			while @facturaPOSId is null
			begin
				select @verificarConsecutivo = null
				select @verificarConsecutivo = consecutivo from FacturasPOS  where  consecutivo = @consecutivoActual
				if (@verificarConsecutivo is not null )
				begin 
					update Resoluciones set consecutivoActual = consecutivoActual+1 WHERE esPos = 'S' and estado = 'AC'  and (@mismaResolucion = 'SI' or tipo = 0)
					select @consecutivoActual=consecutivoActual from Resoluciones where esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)
				end
				else
				begin
					insert into FacturasPOS (fecha,resolucionId,consecutivo,ventaId,estado,terceroid, Placa, Kilometraje, enviada, codigoFormaPago)
					select @Fecha, @ResolucionId, @consecutivoActual, @ventaId, 'CR',@terceroId, @Placa, @Kilometraje, 0, @COD_FOR_PAG
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
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasPOS.[facturaPOSId]
      ,FacturasPOS.[fecha]
      ,FacturasPOS.[resolucionId]
      ,FacturasPOS.[consecutivo]
      ,FacturasPOS.[ventaId]
      ,FacturasPOS.[estado]
      ,FacturasPOS.[terceroId]
      ,FacturasPOS.[Placa]
      ,FacturasPOS.[Kilometraje]
      ,FacturasPOS.[impresa]
      ,FacturasPOS.[consolidadoId]
      ,FacturasPOS.[enviada]
      ,FacturasPOS.[codigoFormaPago]
      ,FacturasPOS.[reporteEnviado]
      ,FacturasPOS.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasPOS
	left join dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	
	where @ventaId = FacturasPOS.ventaId
	
	and FacturasPOS.estado != 'AN'
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
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, FacturasPOS.[facturaPOSId]
      ,FacturasPOS.[fecha]
      ,FacturasPOS.[resolucionId]
      ,FacturasPOS.[consecutivo]
      ,FacturasPOS.[ventaId]
      ,FacturasPOS.[estado]
      ,FacturasPOS.[terceroId]
      ,FacturasPOS.[Placa]
      ,FacturasPOS.[Kilometraje]
      ,FacturasPOS.[impresa]
      ,FacturasPOS.[consolidadoId]
      ,FacturasPOS.[enviada]
      ,FacturasPOS.[codigoFormaPago]
      ,FacturasPOS.[reporteEnviado]
      ,FacturasPOS.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasPOS
	left join dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	inner join @ventas v on FacturasPOS.ventaId = v.[ventaId]
	
	where FacturasPOS.estado != 'AN'
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
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasPOS.[facturaPOSId]
      ,FacturasPOS.[fecha]
      ,FacturasPOS.[resolucionId]
      ,FacturasPOS.[consecutivo]
      ,FacturasPOS.[ventaId]
      ,FacturasPOS.[estado]
      ,FacturasPOS.[terceroId]
      ,FacturasPOS.[Placa]
      ,FacturasPOS.[Kilometraje]
      ,FacturasPOS.[impresa]
      ,FacturasPOS.[consolidadoId]
      ,FacturasPOS.[enviada]
      ,FacturasPOS.[codigoFormaPago]
      ,FacturasPOS.[reporteEnviado]
      ,FacturasPOS.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasPOS
	left join dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where Resoluciones.descripcion = @prefijo
	AND FacturasPOS.consecutivo = @consecutivo
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
	@ventaId int )
as
begin try
		declare @impresa int
		
		select @impresa = impresa from FacturasPOS
				where FacturasPOS.ventaId = @ventaId

		if @impresa >=0
		begin
		Update FacturasPOS
				set impresa = -1,
				enviada=0
				from FacturasPOS
				where FacturasPOS.ventaId = @ventaId
		end
		else begin
		
		Update FacturasPOS
				set impresa = impresa-1,
				enviada=0
				from FacturasPOS
				where FacturasPOS.ventaId = @ventaId
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
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasPOS.[facturaPOSId]
      ,FacturasPOS.[fecha]
      ,FacturasPOS.[resolucionId]
      ,FacturasPOS.[consecutivo]
      ,FacturasPOS.[ventaId]
      ,FacturasPOS.[estado]
      ,FacturasPOS.[terceroId]
      ,FacturasPOS.[Placa]
      ,FacturasPOS.[Kilometraje]
      ,FacturasPOS.[impresa]
      ,FacturasPOS.[consolidadoId]
      ,FacturasPOS.[enviada]
      ,FacturasPOS.[codigoFormaPago]
      ,FacturasPOS.[reporteEnviado]
      ,FacturasPOS.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasPOS
	left join dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where FacturasPOS.impresa <= -1
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

IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasPOS' AND COLUMN_NAME = 'enviadaFacturacion')
BEGIN
  ALTER TABLE FacturasPOS
ADD enviadaFacturacion bit default 0;
END;
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
	from FacturasPOS
    where enviadaFacturacion = 0 or enviadaFacturacion is null
	order by fecha desc
	insert into @facturasTemp (id)
	select 
	top(100)ventaId
	from OrdenesDeDespacho
    where enviadaFacturacion = 0 or enviadaFacturacion is null
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
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasPOS.[facturaPOSId]
      ,FacturasPOS.[fecha]
      ,FacturasPOS.[resolucionId]
      ,FacturasPOS.[consecutivo]
      ,FacturasPOS.[ventaId]
      ,FacturasPOS.[estado]
      ,FacturasPOS.[terceroId]
      ,FacturasPOS.[Placa]
      ,FacturasPOS.[Kilometraje]
      ,FacturasPOS.[impresa]
      ,FacturasPOS.[consolidadoId]
      ,FacturasPOS.[enviada]
      ,FacturasPOS.[codigoFormaPago]
      ,FacturasPOS.[reporteEnviado]
      ,FacturasPOS.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from FacturasPOS
    inner join @facturasTemp tmp on tmp.id = FacturasPOS.ventaId 
	inner join dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	inner join dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    inner join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where FacturasPOS.estado != 'AN'
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
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from OrdenesDeDespacho
    inner join @facturasTemp tmp on tmp.id = OrdenesDeDespacho.ventaId 
	inner join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	inner join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    inner join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	
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
	update FacturasPOS set enviadaFacturacion = 1
    from FacturasPOS
    inner join @facturas f on f.ventaId = FacturasPOS.ventaId

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
CREATE procedure [dbo].[ActuralizarTurnoEnviadas]
(
	@facturas [ventasIds] readonly
)
as
begin try
    set nocount on;
	update FacturasPOS set turnoEnviado = 1
    from FacturasPOS
    inner join @facturas f on f.ventaId = FacturasPOS.ventaId

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
CREATE procedure [dbo].[ActuralizarFechasReportesEnviadas]
(
	@facturas [ventasIds] readonly
)
as
begin try
    set nocount on;
	update FacturasPOS set reporteEnviado = 1
    from FacturasPOS
    inner join @facturas f on f.ventaId = FacturasPOS.ventaId

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
	update FacturasPOS set enviadaFacturacion = 1
    from FacturasPOS
    inner join @facturas f on f.ventaId = FacturasPOS.ventaId

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
	update FacturasPOS set enviadaFacturacion = 1
    from FacturasPOS
    where @ventaId = FacturasPOS.ventaId

	
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

	declare @idFacturas as table (id int);
	
	insert into @idFacturas(id)
	select FacturasPOS.ventaid
	from dbo.FacturasPOS
	where FacturasPOS.enviadaFacturacion = 1

	
	insert into @idFacturas(id)
	select OrdenesDeDespacho.ventaid
	from dbo.OrdenesDeDespacho
	where OrdenesDeDespacho.enviadaFacturacion = 1
	
	update FacturasPOS
	set enviadaFacturacion = 0
	update OrdenesDeDespacho
	set enviadaFacturacion = 0

	select
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasPOS.[facturaPOSId]
      ,FacturasPOS.[fecha]
      ,FacturasPOS.[resolucionId]
      ,FacturasPOS.[consecutivo]
      ,FacturasPOS.[ventaId]
      ,FacturasPOS.[estado]
      ,FacturasPOS.[terceroId]
      ,FacturasPOS.[Placa]
      ,FacturasPOS.[Kilometraje]
      ,FacturasPOS.[impresa]
      ,FacturasPOS.[consolidadoId]
      ,FacturasPOS.[enviada]
      ,FacturasPOS.[codigoFormaPago]
      ,FacturasPOS.[reporteEnviado]
      ,FacturasPOS.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasPOS
	inner join @idFacturas idf on idf.id = FacturasPOS.ventaid
	left join dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
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
      ,OrdenesDeDespacho.[reporteEnviado]
      ,OrdenesDeDespacho.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.OrdenesDeDespacho
	inner join @idFacturas idf on idf.id = OrdenesDeDespacho.ventaid
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
	declare @ResolucionId int, @consecutivoActual int, @fechafinal DATETIME, @facturaPOSId int, @ConsecutivoFinal int,
	
	@clientesCreditoGeneranFactura VARCHAR (50), @soloGeneraOrdenes VARCHAR (50), @verificarConsecutivo int,
	@OrdenDeDespachoId int,  @mismaResolucion VARCHAR (50);
	
	select @mismaResolucion=valor from configuracionEstacion where descripcion = 'mismaResolucion'
	if @mismaResolucion is null
	begin
	insert into configuracionEstacion(descripcion, valor) values ('mismaResolucion','SI')
	end

	select @facturaPOSId = facturaPOSId from FacturasPOS where ventaId = @ventaId
	declare @terceroId int,
	@Placa varchar(50) = null,
	@Kilometraje varchar(50) = null,
	@COD_FOR_PAG smallint,
	@Fecha datetime = null;

	select @terceroId = terceroId, @Placa = Placa, @Kilometraje = Kilometraje, @COD_FOR_PAG = codigoFormaPago, @Fecha = GETDATE()
	from OrdenesDeDespacho
	where  @ventaId = OrdenesDeDespacho.ventaId

	if @facturaPOSId is not null 
	begin
		select @facturaPOSId as facturaPOSId
	end
	else 
	begin
		select @ResolucionId = ResolucionId, @consecutivoActual = consecutivoActual, @fechafinal = fechafinal, @ConsecutivoFinal = consecutivoFinal
		from Resoluciones where esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)

		if @fechafinal is null or @fechafinal < GETDATE() or @ConsecutivoFinal <= @consecutivoActual
		begin
			update Resoluciones set estado = 'VE' WHERE esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)
			select @facturaPOSId as facturaPOSId
		end
		else
		begin
			while @facturaPOSId is null
			begin
				select @verificarConsecutivo = null
				select @verificarConsecutivo = consecutivo from FacturasPOS  where  consecutivo = @consecutivoActual
				if (@verificarConsecutivo is not null )
				begin 
					update Resoluciones set consecutivoActual = consecutivoActual+1 WHERE esPos = 'S' and estado = 'AC' 
					select @consecutivoActual=consecutivoActual from Resoluciones where esPos = 'S' and estado = 'AC' 
				end
				else
				begin
					insert into FacturasPOS (fecha,resolucionId,consecutivo,ventaId,estado,terceroid, Placa, Kilometraje, enviada, codigoFormaPago)
					select @Fecha, @ResolucionId, @consecutivoActual, @ventaId, 'CR',@terceroId, @Placa, @Kilometraje, 0, @COD_FOR_PAG
					from Resoluciones WHERE esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)
			
					select @facturaPOSId = SCOPE_IDENTITY()

					update Resoluciones set consecutivoActual = @consecutivoActual+1 WHERE esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)

					
					delete OrdenesDeDespacho 
					from OrdenesDeDespacho
					where @ventaId = OrdenesDeDespacho.ventaId
    
				end
			
			
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
	declare @ResolucionId int, @consecutivoActual int, @fechafinal DATETIME, @facturaPOSId int, @ConsecutivoFinal int,
	
	@clientesCreditoGeneranFactura VARCHAR (50), @soloGeneraOrdenes VARCHAR (50), @verificarConsecutivo int,
	@OrdenDeDespachoId int,  @mismaResolucion VARCHAR (50);
	
	select @mismaResolucion=valor from configuracionEstacion where descripcion = 'mismaResolucion'
	if @mismaResolucion is null
	begin
	insert into configuracionEstacion(descripcion, valor) values ('mismaResolucion','SI')
	end

	select @facturaPOSId = facturaPOSId from OrdenesDeDespacho where ventaId = @ventaId
	declare @terceroId int,
	@Placa varchar(50) = null,
	@Kilometraje varchar(50) = null,
	@COD_FOR_PAG smallint,
	@Fecha datetime = null;

	select @terceroId = terceroId, @Placa = Placa, @Kilometraje = Kilometraje, @COD_FOR_PAG = codigoFormaPago, @Fecha = GETDATE()
	from FacturasPOS
	where  @ventaId = FacturasPOS.ventaId

	if @facturaPOSId is not null 
	begin
		select @facturaPOSId as facturaPOSId
	end
	else 
	begin
		select @ResolucionId = ResolucionId, @consecutivoActual = consecutivoActual, @fechafinal = fechafinal, @ConsecutivoFinal = consecutivoFinal
		from Resoluciones where esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)

		if @fechafinal is null or @fechafinal < GETDATE() or @ConsecutivoFinal <= @consecutivoActual
		begin
			update Resoluciones set estado = 'VE' WHERE esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)
			select @facturaPOSId as facturaPOSId
		end
		else
		begin
			
					insert into OrdenesDeDespacho(consecutivo,fecha,resolucionId,ventaId,estado,terceroid, Placa, Kilometraje, enviada, codigoFormaPago)
					select 0, @Fecha, @ResolucionId, @ventaId, 'CR',@terceroId, @Placa, @Kilometraje, 0, @COD_FOR_PAG
					from Resoluciones WHERE esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 0)
			
					select @facturaPOSId = SCOPE_IDENTITY()


					
					update FacturasPOS set estado = 'AN'
					from FacturasPOS
					where @ventaId = FacturasPOS.ventaId
    
			
			
				select @facturaPOSId as facturaPOSId
			
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


IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ActualizarFactura')
	DROP PROCEDURE [dbo].[ActualizarFactura]
GO
CREATE procedure [dbo].[ActualizarFactura]
( 
    @facturaPOSId int,
	@Placa varchar(50) = null,
	@Kilometraje varchar(50) = null,
	@codigoFormaPago int = null,
	@terceroId int = null,
	@ventaId int
)
as
begin try
    set nocount on;


	Update FacturasPOS
	set Placa = @Placa,
	Kilometraje = @Kilometraje,
	impresa = impresa+1,
    enviada = 0,
    codigoFormaPago = @codigoFormaPago,
	terceroId = isnull(@terceroId, terceroId)
	Where @facturaPOSId = facturaPOSId
	and ventaId = @ventaID

	
	Update OrdenesDeDespacho
	set Placa = @Placa,
	Kilometraje = @Kilometraje,
	impresa = impresa+1,
    enviada = 0,
    codigoFormaPago = @codigoFormaPago,
	terceroId = isnull(@terceroId, terceroId)
	Where @facturaPOSId = facturaPOSId
	and ventaId = @ventaID
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
		
		select @impresa = impresa from FacturasPOS
				where FacturasPOS.consecutivo = @consecutivo or FacturasPOS.ventaid = @consecutivo

		if @impresa >=0
		begin
		Update FacturasPOS
				set impresa = -1,
				enviada=0
				from FacturasPOS
				where FacturasPOS.consecutivo = @consecutivo or FacturasPOS.ventaid = @consecutivo
		end
		else begin
		
		Update FacturasPOS
				set impresa = impresa-1,
				enviada=0
				from FacturasPOS
				where FacturasPOS.consecutivo = @consecutivo or FacturasPOS.ventaid = @consecutivo
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
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasPOS.[facturaPOSId]
      ,FacturasPOS.[fecha]
      ,FacturasPOS.[resolucionId]
      ,FacturasPOS.[consecutivo]
      ,FacturasPOS.[ventaId]
      ,FacturasPOS.[estado]
      ,FacturasPOS.[terceroId]
      ,FacturasPOS.[Placa]
      ,FacturasPOS.[Kilometraje]
      ,FacturasPOS.[impresa]
      ,FacturasPOS.[consolidadoId]
      ,FacturasPOS.[enviada]
      ,FacturasPOS.[codigoFormaPago]
      ,FacturasPOS.[reporteEnviado]
      ,FacturasPOS.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasPOS
	left join dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where FacturasPOS.ventaId =@idVenta
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
	from FacturasPOS
    where turnoEnviado = 0 or turnoEnviado is null
	order by ventaId desc

	insert into @facturasTemp (id)
	select 
	top(100)ventaId
	from OrdenesDeDespacho
    where turnoEnviado = 0 or turnoEnviado is null
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
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasPOS.[facturaPOSId]
      ,FacturasPOS.[fecha]
      ,FacturasPOS.[resolucionId]
      ,FacturasPOS.[consecutivo]
      ,FacturasPOS.[ventaId]
      ,FacturasPOS.[estado]
      ,FacturasPOS.[terceroId]
      ,FacturasPOS.[Placa]
      ,FacturasPOS.[Kilometraje]
      ,FacturasPOS.[impresa]
      ,FacturasPOS.[consolidadoId]
      ,FacturasPOS.[enviada]
      ,FacturasPOS.[codigoFormaPago]
      ,FacturasPOS.[reporteEnviado]
      ,FacturasPOS.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from FacturasPOS
    inner join @facturasTemp tmp on tmp.id = FacturasPOS.ventaId 
	left join dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	
	where FacturasPOS.estado != 'AN'
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
      ,OrdenesDeDespacho.[reporteEnviado]
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaPorConsecutivo')
	DROP PROCEDURE [dbo].getFacturaPorConsecutivo
GO
CREATE procedure [dbo].[getFacturaPorConsecutivo]
(
	@consecutivo int
)
as
begin try
    set nocount on;
	select 
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasPOS.[facturaPOSId]
      ,FacturasPOS.[fecha]
      ,FacturasPOS.[resolucionId]
      ,FacturasPOS.[consecutivo]
      ,FacturasPOS.[ventaId]
      ,FacturasPOS.[estado]
      ,FacturasPOS.[terceroId]
      ,FacturasPOS.[Placa]
      ,FacturasPOS.[Kilometraje]
      ,FacturasPOS.[impresa]
      ,FacturasPOS.[consolidadoId]
      ,FacturasPOS.[enviada]
      ,FacturasPOS.[codigoFormaPago]
      ,FacturasPOS.[reporteEnviado]
      ,FacturasPOS.[enviadaFacturacion], terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasPOS
	left join dbo.Resoluciones on FacturasPOS.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasPOS.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where FacturasPOS.consecutivo = @consecutivo or FacturasPOS.ventaid = @consecutivo
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