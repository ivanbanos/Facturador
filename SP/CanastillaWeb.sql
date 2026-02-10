use cootransmagdalena

GO

	IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Canastilla' and xtype='U')
BEGIN
    create table dbo.Canastilla(
    CanastillaId INT PRIMARY KEY IDENTITY (1, 1),
    [Guid] uniqueidentifier NULL,
    descripcion VARCHAR (50) NOT NULL,
	unidad VARCHAR (50) NOT NULL,
	precio float NOT NULL,
	deleted bit not null default 0
);
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Canastilla' AND COLUMN_NAME = 'iva')
BEGIN
  
  ALTER TABLE Canastilla
ADD iva int not null default 0;
END;

GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Canastilla' AND COLUMN_NAME = 'campoextra')
BEGIN
  
  ALTER TABLE Canastilla
ADD campoextra varchar(50) NULL;
END;

GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Canastilla' AND COLUMN_NAME = 'estacion')
BEGIN
  
  ALTER TABLE Canastilla
ADD estacion uniqueidentifier NULL;
END;

GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CanastillaType' and xtype='U')
BEGIN
--drop procedure UpdateOrCreateCanastilla
--drop TYPE [dbo].[CanastillaType]

create TYPE [dbo].[CanastillaType] AS TABLE 
(CanastillaId INT null,
    [Guid] uniqueidentifier NULL,
    descripcion VARCHAR (50) NOT NULL,
	unidad VARCHAR (50) NOT NULL,
	precio float NOT NULL,
	deleted bit not null default 0,
	iva int not null default 0,
	campoextra varchar(50) NULL,
	estacion uniqueidentifier NULL
	);  
	END
GO
IF type_id('[dbo].[UpdateOrCreateCanastilla]') IS NOT NULL
begin
DROP PROCEDURE [dbo].[UpdateOrCreateCanastilla]
		end
GO
create or alter PROCEDURE [dbo].[UpdateOrCreateCanastilla]
	@canastillas dbo.[CanastillaType] READONLY
AS
BEGIN
	

	UPDATE Canastilla SET Canastilla.descripcion =  c.descripcion,
	Canastilla.unidad =  c.unidad,
	Canastilla.precio =  c.precio,
	Canastilla.deleted =  c.deleted,
	Canastilla.iva =  c.iva,
	Canastilla.campoextra =  c.campoextra,
	Canastilla.estacion =  c.estacion
		from @canastillas c
		inner join Canastilla on  Canastilla.[Guid] = c.[Guid]

	Insert into Canastilla([Guid],descripcion,unidad,precio,deleted,iva,campoextra,estacion)
	select NEWID(),c.descripcion,c.unidad,c.precio,0,c.iva,c.campoextra,c.estacion
	from @canastillas c
	left join Canastilla on  Canastilla.[Guid] = c.[Guid]
		where Canastilla.[Guid] is null

END
GO

--DROP PROCEDURE [dbo].[GetCanastilla]
		
GO
create or alter PROCEDURE [dbo].[GetCanastilla]
	@estacion UNIQUEIDENTIFIER = NULL
AS
BEGIN
	

	select [Guid],descripcion,unidad,precio,deleted, iva, campoextra, estacion
	from Canastilla
	where deleted = 0
	AND (@estacion IS NULL OR estacion = @estacion)

END
GO
drop procedure AddNuevaResolucion 
drop procedure AnularResolucion
DROP TYPE [dbo].[ResolucionType]
GO
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
    [Descripcion] varchar(50) NULL,
	[tipo] int not null default 0
	)
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Resolucion' AND COLUMN_NAME = 'tipo')
BEGIN
  
  ALTER TABLE Resolucion
ADD [tipo] int not null default 0
END;
GO
drop PROCEDURE [dbo].[HabilitarResolucion]
GO
CREATE PROCEDURE [dbo].[HabilitarResolucion]
	@fechaVencimiento DateTime,
	@resolucion UNIQUEIDENTIFIER
AS
BEGIN
	DECLARE @idEstadoHabilitado INT, @idEstacion INT
	

	
	SELECT @idEstadoHabilitado = Id FROM Estados WHERE Texto = 'Habilitada'

	UPDATE Resolucion
		SET Resolucion.IdEstado = @idEstadoHabilitado,
		Resolucion.FechaFinal = @fechaVencimiento,
		Resolucion.Habilitada = 1
	FROM Resolucion
	WHERE Resolucion.[Guid] = @resolucion
END
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FacturasCanastilla' and xtype='U')
BEGIN
    create table dbo.FacturasCanastilla(
    FacturasCanastillaId INT PRIMARY KEY IDENTITY (1, 1),
    fecha DATETIME NOT NULL,
    resolucionId int NOT NULL,
    consecutivo int NOT NULL,
    estado CHAR(2),
	terceroId int,
	codigoFormaPago int not null default 4,
	subtotal float not null,
	descuento  float NOT NULL,
	iva float not null,
	total float NOT NULL,
	idEstacion int not null,
	[guid] uniqueidentifier not null
    FOREIGN KEY (resolucionId) REFERENCES dbo.Resolucion (Id)
);

END

GO
-- Crear índice por consecutivo para mejorar rendimiento en búsquedas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FacturasCanastilla_Consecutivo_IdEstacion')
BEGIN
    CREATE INDEX IX_FacturasCanastilla_Consecutivo_IdEstacion ON FacturasCanastilla (consecutivo, idEstacion);
END

GO
IF EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FacturasCanastilla' AND COLUMN_NAME = 'canastillaId')
BEGIN
  
  ALTER TABLE FacturasCanastilla
drop column canastillaId;
END;
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FacturasCanastillaDetalle' and xtype='U')
BEGIN
    create table dbo.FacturasCanastillaDetalle(
    FacturasCanastillaDetalleId INT PRIMARY KEY IDENTITY (1, 1),
	FacturasCanastillaId INT NOT NULL,
	canastillaId int not null,
	cantidad float not null,
	precio float NOT NULL,
	subtotal float not null,
	iva float not null,
	total float NOT NULL,
    FOREIGN KEY (FacturasCanastillaId) REFERENCES dbo.FacturasCanastilla (FacturasCanastillaId)
);

END

GO
DROP PROCEDURE [dbo].[UpdateConsecutivoResolucion]
GO
CREATE PROCEDURE [dbo].[UpdateConsecutivoResolucion]
AS
	declare @maxConsecutivoTable as table (idResolucion int, maxConsecutivo int)

	insert into @maxConsecutivoTable  (idResolucion, maxConsecutivo)
	select Resolucion.Id, case when max(Factura.consecutivo)>max(FacturasCanastilla.consecutivo)
	then max(Factura.consecutivo) else  max(FacturasCanastilla.consecutivo) end
	from Resolucion
	inner join Estados on Resolucion.IdEstado = Estados.Id
	left join Facturas as Factura On Factura.IdEstacion = Resolucion.IdEstacion
	left join FacturasCanastilla as FacturasCanastilla On FacturasCanastilla.IdEstacion = Resolucion.IdEstacion
	where Estados.Texto = 'Activo'
	GROUP BY Resolucion.id


	update Resolucion set ConsecutivoActual = maxConsecutivo
	from Resolucion
	inner join @maxConsecutivoTable as mct on Resolucion.Id = mct.idResolucion
RETURN 0
GO
GO
CREATE PROCEDURE [dbo].[AddNuevaResolucion]
	@Resoluciones [dbo].[ResolucionType] READONLY
AS
BEGIN
	DECLARE @idEstadoAnulada INT, @idEstadoActivo INT, @idEstacion INT, @estacion UNIQUEIDENTIFIER
	
	SELECT @idEstadoAnulada = Estados.Id FROM Estados WHERE Texto = 'Anulada'
	SELECT @idEstadoActivo = Estados.Id FROM Estados WHERE Texto = 'Activo'

	UPDATE Resolucion SET IdEstado = @idEstadoAnulada
	From Resolucion
	JOIN Estaciones
		ON Estaciones.Id = Resolucion.IdEstacion
	JOIN @Resoluciones Resoluciones ON Estaciones.[Guid] = Resoluciones.IdEstacion and Resolucion.tipo = Resoluciones.tipo

	INSERT INTO Resolucion([Guid],ConsecutivoInicial,ConsecutivoFinal,FechaInicial,FechaFinal,IdEstado,ConsecutivoActual,
						   Fecha,[IdEstacion],[Autorizacion],[Habilitada],[Descripcion],tipo)
	SELECT NEWID(),ConsecutivoInicial,ConsecutivoFinal,FechaInicial,FechaFinal,@idEstadoActivo,ConsecutivoInicial,
		   GETDATE(),Estaciones.Id,[Autorizacion],[Habilitada],[Descripcion],tipo 
	FROM @Resoluciones Resoluciones
	JOIN Estaciones
		ON Estaciones.[Guid] = Resoluciones.IdEstacion
END
GO
GO
CREATE PROCEDURE [dbo].[AnularResolucion]
	@resolucion uniqueidentifier
AS
BEGIN
	DECLARE @idEstadoAnulada INT, @idEstadoActivo INT, @idEstacion INT, @estacion UNIQUEIDENTIFIER
	
	SELECT @idEstadoAnulada = Estados.Id FROM Estados WHERE Texto = 'Anulada'

	UPDATE Resolucion SET IdEstado = @idEstadoAnulada
	From Resolucion
	where [guid] = @resolucion

	
END
GO

ALTER PROCEDURE [dbo].[GetResolucionActiva]
	@estacion UNIQUEIDENTIFIER
AS
BEGIN
	DECLARE @idEstadoHabilitado INT, @idEstadoActivo INT, @idEstacion INT
	

	SELECT @idEstacion = Id FROM Estaciones WHERE Estaciones.Guid = @estacion
	
	SELECT @idEstadoActivo = Estados.Id FROM Estados WHERE Texto = 'Activo'
	SELECT @idEstadoHabilitado = Estados.Id  FROM Estados WHERE Texto = 'Habilitada'

	SELECT Resolucion.[Guid],ConsecutivoInicial,ConsecutivoFinal,FechaInicial,FechaFinal,Estados.[Guid] as IdEstado, Estados.Texto as Estado,ConsecutivoActual,
	Fecha,Estaciones.[Guid] as [IdEstacion],[Autorizacion],[Habilitada],[Descripcion],Resolucion.Tipo FROM Resolucion
	INNER JOIN Estaciones ON Estaciones.Id = Resolucion.IdEstacion
	INNER JOIN Estados ON Estados.Id = Resolucion.IdEstado
	WHERE Estaciones.Guid = @estacion
	AND (Resolucion.IdEstado = @idEstadoActivo OR Resolucion.IdEstado = @idEstadoHabilitado)
END
GO


CREATE procedure [dbo].[CrearFacturaCanastilla]
( 
	@fecha DateTime,
	@resolucion varchar(50),
	@consecutivo int, 
	@tercero varchar(250), 
	@forma varchar(50), 
	@subtotal float, 
	@descuento float, 
	@iva float, 
	@total float, 
	@estacion uniqueidentifier,
	
	@detalle CanastillaDetalleType readonly
)
as
begin try
    set nocount on;
	declare @facturaId int
	
	-- Verificar si ya existe una factura con el mismo consecutivo y estación
	select @facturaId = FacturasCanastillaId 
	from FacturasCanastilla 
	inner join Resolucion on FacturasCanastilla.resolucionId = Resolucion.Id
	inner join Estaciones on Resolucion.IdEstacion = Estaciones.Id
	where FacturasCanastilla.consecutivo = @consecutivo 
	and Estaciones.Guid = @estacion
	
	if @facturaId is not null
	Begin
		-- Ya existe una factura con este consecutivo para esta estación
		--raiserror('Ya existe una factura con el consecutivo %d para esta estación', 16, 1, @consecutivo)
		return @facturaId
	END
	else
	begin
	declare @ResolucionId int, @terceroId int, @formaId int;

	declare @estadoId int, @idEstacion int
			
	select @estadoId = Id
			from Estados
			Where Texto = 'Creada'

	select @ResolucionId = Id
			from Resolucion
			Where Autorizacion = @resolucion
			
	select @terceroId = Id
			from Terceros
			Where Identificacion = @tercero

	select @formaId = Id
			from FormaDePago
			Where Nombre = @forma

	SELECT @idEstacion = Estaciones.Id
			FROM  dbo.Estaciones
			WHERE Estaciones.Guid = @estacion
	
	insert into FacturasCanastilla(fecha, resolucionId, consecutivo, estado, terceroId, codigoFormaPago,subtotal,descuento,iva,total,idEstacion,[guid])
	values (@fecha, @ResolucionId, @consecutivo, @estadoId, @terceroId, @formaId,@subtotal,@descuento,@iva,@total,@idEstacion,newid())
	declare @IdFacturaCanastilla int
	select  @IdFacturaCanastilla = SCOPE_IDENTITY()

	
	insert into FacturasCanastillaDetalle(FacturasCanastillaId, canastillaId, cantidad, precio,subtotal,iva,total)
	select @IdFacturaCanastilla, Canastilla.CanastillaId, d.cantidad, d.precio, d.subtotal, d.iva, d.total
	from @detalle d
	inner join Canastilla on Canastilla.guid = d.guid
	WHERE Canastilla.estacion = @estacion OR Canastilla.estacion IS NULL
	
	declare @ConsecutivoActual int;
	select @ConsecutivoActual = max(FacturasCanastilla.consecutivo)
	from FacturasCanastilla
	where FacturasCanastilla.resolucionId = @ResolucionId

	update resolucion set ConsecutivoActual = @ConsecutivoActual
	from resolucion
	where  resolucion.Id = @ResolucionId

	return @IdFacturaCanastilla
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

--drop procedure CrearFacturaCanastilla
--drop procedure CrearFacturaDetalle
--drop TYPE [dbo].[CanastillaDetalleType]

create TYPE [dbo].[CanastillaDetalleType] AS TABLE 
(
    guid uniqueidentifier NOT NULL,
	cantidad float NOT NULL,
	precio float,
	subtotal float,
	iva float,
	total float
	);  
	
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CrearFacturaDetalle')
	DROP PROCEDURE [dbo].[CrearFacturaDetalle]
GO
CREATE procedure [dbo].CrearFacturaDetalle
( 
	@detalle CanastillaDetalleType readonly,
	@idFactura int,
	@estacion uniqueidentifier
)
as
begin try
    set nocount on;
	declare @facturaId int
	select @facturaId from FacturasCanastillaDetalle where @idFactura = FacturasCanastillaId
	if @facturaId is  null
	Begin
		
	insert into FacturasCanastillaDetalle(FacturasCanastillaId, canastillaId, cantidad, precio,subtotal,iva,total)

	select @idFactura, Canastilla.CanastillaId, d.cantidad, d.precio, d.subtotal, d.iva, d.total
	from @detalle d
	inner join Canastilla on Canastilla.guid = d.guid
	WHERE Canastilla.estacion = @estacion OR Canastilla.estacion IS NULL
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

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturasCanastilla')
	DROP PROCEDURE [dbo].[getFacturasCanastilla]
GO
CREATE procedure [dbo].[getFacturasCanastilla]
(

	@FechaInicial DATETIME = '1990-01-01',
	@FechaFinal DATETIME = '2100-01-01',
	@IdentificacionTercero NVARCHAR(250) = NULL,
	@NombreTercero NVARCHAR(250) = NULL,
	@Estacion UNIQUEIDENTIFIER = NULL
)
as
begin try
    set nocount on;

	select @FechaFinal = dateadd(day, 1, @FechaFinal)

	select 
	FacturasCanastilla.[FacturasCanastillaId],
	FacturasCanastilla.Guid,
      FacturasCanastilla.fecha, FacturasCanastilla.consecutivo, FacturasCanastilla.subtotal, FacturasCanastilla.descuento, FacturasCanastilla.iva, FacturasCanastilla.total,
	  terceros.Nombre,terceros.Segundo,terceros.Apellidos,terceros.identificacion, 
	  TipoIdentificacion.Texto as TipoIdentificacion, FormaDePago.Nombre as FormaDePago
	
	from dbo.FacturasCanastilla
	left join dbo.terceros on FacturasCanastilla.terceroId = terceros.Id
    left join dbo.TipoIdentificacion on terceros.tipoIdentificacion = TipoIdentificacion.Id
    left join dbo.FormaDePago on FormaDePago.Id = FacturasCanastilla.codigoFormaPago
    left join dbo.Estaciones on Estaciones.Id = FacturasCanastilla.idEstacion
	WHERE (@IdentificacionTercero IS NULL OR terceros.Identificacion = @IdentificacionTercero)
		  AND (@NombreTercero IS NULL OR terceros.Nombre LIKE @NombreTercero)
		  AND FacturasCanastilla.fecha >= CONVERT(date, @FechaInicial) AND FacturasCanastilla.fecha <=CONVERT(date, @FechaFinal) 
		  AND (@Estacion IS NULL OR Estaciones.[Guid] = @Estacion)
	ORDER BY Consecutivo DESC
	

    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaCanatillaDetalle')
	DROP PROCEDURE [dbo].[getFacturaCanatillaDetalle]
GO
CREATE procedure [dbo].[getFacturaCanatillaDetalle]
(@guid UNIQUEIDENTIFIER)
as
begin try
    set nocount on;



	select FacturasCanastillaDetalle.cantidad, FacturasCanastillaDetalle.precio, FacturasCanastillaDetalle. subtotal,
	FacturasCanastillaDetalle.iva, FacturasCanastillaDetalle.total,
	Canastilla.descripcion
	
	from dbo.FacturasCanastillaDetalle
	left join dbo.Canastilla on FacturasCanastillaDetalle.canastillaId = Canastilla.canastillaId
	left join dbo.FacturasCanastilla on FacturasCanastillaDetalle.facturasCanastillaId  = FacturasCanastilla.facturasCanastillaId
	
	WHERE FacturasCanastilla.[Guid] = @guid
	

    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaCanastilla')
	DROP PROCEDURE [dbo].[getFacturaCanastilla]
GO
CREATE procedure [dbo].[getFacturaCanastilla]
(

	@guid UNIQUEIDENTIFIER = NULL
)
as
begin try
    set nocount on;



	select 
	FacturasCanastilla.[FacturasCanastillaId],FacturasCanastilla.guid,
      FacturasCanastilla.fecha, FacturasCanastilla.consecutivo, FacturasCanastilla.subtotal, FacturasCanastilla.descuento, FacturasCanastilla.iva, FacturasCanastilla.total,
	  terceros.Nombre,terceros.Segundo,terceros.Apellidos,terceros.identificacion, 
	  TipoIdentificacion.Texto as TipoIdentificacion, FormaDePago.Nombre as FormaDePago
	
	from dbo.FacturasCanastilla
	left join dbo.terceros on FacturasCanastilla.terceroId = terceros.Id
    left join dbo.TipoIdentificacion on terceros.tipoIdentificacion = TipoIdentificacion.Id
    left join dbo.FormaDePago on FormaDePago.Id = FacturasCanastilla.codigoFormaPago
	WHERE FacturasCanastilla.[Guid] = @guid
	

    
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



IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CrearFacturaCanastilla')
	DROP PROCEDURE [dbo].[CrearFacturaCanastilla]
GO


select * from Canastilla
select * from Estaciones
update canastilla set estacion='F426EA66-32ED-46FA-9B27-55B330D633F6'
insert into Canastilla(Guid, descripcion, unidad, precio, deleted,iva, campoextra, estacion)
select NEWID(), descripcion, unidad, precio, deleted,iva, campoextra, 'FC65DBD2-118A-43A7-AB7A-4F6AFE6FD94D'
from Canastilla where estacion = 'B42F579A-06F5-4C7C-8C02-BF839CE7A84A'