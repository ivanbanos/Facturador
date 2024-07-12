USE Facturacion_Electronica

GO

	IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Canastilla' and xtype='U')
BEGIN
    create table dbo.Canastilla(
    CanastillaId INT PRIMARY KEY IDENTITY (1, 1),
    descripcion VARCHAR (50) NOT NULL,
	unidad VARCHAR (50) NOT NULL,
	precio float NOT NULL,
	deleted bit not null default 0,
	iva int not null default 0,
	[guid] uniqueidentifier
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
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FacturasCanastilla' and xtype='U')
BEGIN
    create table dbo.FacturasCanastilla(
    FacturasCanastillaId INT PRIMARY KEY IDENTITY (1, 1),
    fecha DATETIME NOT NULL,
    resolucionId int NOT NULL,
    consecutivo int NOT NULL,
    estado CHAR(2),
	terceroId int,
	impresa int default 0,
	enviada bit default 0,
	codigoFormaPago int not null default 4,
	subtotal float not null,
	descuento  float NOT NULL,
	iva float not null,
	total float NOT NULL,
    FOREIGN KEY (resolucionId) REFERENCES dbo.Resoluciones (ResolucionId)
);

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

declare @config int

select @config=configId from configuracionEstacion where descripcion = 'ivaCanastila'

IF @config is null
begin
INSERT INTO configuracionEstacion(descripcion,valor) values('ivaCanastila','19')
end
GO

GO
declare @config int

select @config=configId from configuracionEstacion where descripcion = 'mismaResolucion'

IF @config is null
begin
INSERT INTO configuracionEstacion(descripcion,valor) values('mismaResolucion','SI')
end

GO
IF type_id('[dbo].[canastillaType]') IS NOT NULL
begin
DROP PROCEDURE [dbo].[CrearFacturaCanastilla]
DROP PROCEDURE UpdateOrCreateCanastilla
        DROP TYPE [dbo].[canastillaType];
		end
GO

create TYPE [dbo].[CanastillaType] AS TABLE 
(CanastillaId INT null,
    [Guid] uniqueidentifier NULL,
    descripcion VARCHAR (50) NOT NULL,
	unidad VARCHAR (50) NOT NULL,
	precio float NOT NULL,
	deleted bit not null default 0,
	iva int not null default 0,
	cantidad float NULL
	);  
GO
IF type_id('[dbo].[UpdateOrCreateCanastilla]') IS NOT NULL
begin
DROP PROCEDURE [dbo].[UpdateOrCreateCanastilla]
		end
GO
create PROCEDURE [dbo].[UpdateOrCreateCanastilla]
	@canastillas dbo.[CanastillaType] READONLY
AS
BEGIN
	

	UPDATE Canastilla SET Canastilla.descripcion =  c.descripcion,
	Canastilla.unidad =  c.unidad,
	Canastilla.precio =  c.precio,
	Canastilla.deleted =  c.deleted,
	Canastilla.iva =  c.iva
	
		from @canastillas c
		inner join Canastilla on  Canastilla.[Guid] = c.[Guid]

	Insert into Canastilla([Guid],descripcion,unidad,precio,deleted,iva)
	select c.[Guid],c.descripcion,c.unidad,c.precio,0,c.iva
	from @canastillas c
	left join Canastilla on  Canastilla.[Guid] = c.[Guid]
		where Canastilla.[Guid] is null

	UPDATE Canastilla SET 
	Canastilla.deleted =  1
	from Canastilla
		left join @canastillas c on  Canastilla.[Guid] = c.[Guid]
		where c.[Guid] is null

END
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CrearFacturaCanastilla')
	DROP PROCEDURE [dbo].[CrearFacturaCanastilla]
GO
CREATE procedure [dbo].[CrearFacturaCanastilla]
( 
	@terceroId int,
	@COD_FOR_PAG smallint,
	@canastillaIds [canastillaType] readonly,

	@descuento float,
	@imprimir bit=1
)
as
begin try
    set nocount on;
	declare @ResolucionId int, @consecutivoActual int, @fechafinal DATETIME, @facturaCanastillaId int, @ConsecutivoFinal int,
	
	@cantidadCanastillas INT, @verificarConsecutivo int, @mismaResolucion VARCHAR (50), @subtotal float = 0, @iva float, @total float;
	declare @ivaCanastila INT = 0, @ivaPorcentaje bit = 0;
	
	if @COD_FOR_PAG=0
	begin set @COD_FOR_PAG = 4 end 
	select @ivaPorcentaje = case  when c.iva < 20 then 1 else 
	0 end
	from @canastillaIds c
	if @ivaPorcentaje = 1
	begin
	select @ivaCanastila+= (c.precio*c.iva/(100.0+c.iva)) * c.cantidad
	from @canastillaIds c
	
	select @subtotal+=cids.cantidad* (Canastilla.precio*((100.0)/(100.0+cids.iva))) from @canastillaIds cids
	inner join Canastilla on  cids.canastillaId = Canastilla.canastillaId
	end
	else 
	begin
	
	select @ivaCanastila+= c.iva * c.cantidad
	from @canastillaIds c
	
	select @subtotal+=cids.cantidad* (Canastilla.precio-cids.iva) from @canastillaIds cids
	inner join Canastilla on  cids.canastillaId = Canastilla.canastillaId
	end
    
	select @cantidadCanastillas = count(ResolucionId) from Resoluciones where esPos = 'S' and estado = 'AC'

	if @cantidadCanastillas = 1
	begin
		select @mismaResolucion='SI'
	end
	else
	begin
		select @mismaResolucion=valor from configuracionEstacion where descripcion = 'mismaResolucion'
	end

	if @subtotal != 0
	begin
		select @ResolucionId = ResolucionId, @consecutivoActual = consecutivoActual, @fechafinal = fechafinal, @ConsecutivoFinal = consecutivoFinal
		from Resoluciones where esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 1)
		update Resoluciones set consecutivoActual = @consecutivoActual+1 WHERE esPos = 'S' and estado = 'AC'
		if @fechafinal is null or @fechafinal < GETDATE() or @ConsecutivoFinal <= @consecutivoActual
		begin
			update Resoluciones set estado = 'VE' WHERE esPos = 'S' and estado = 'AC' and (@mismaResolucion = 'SI' or tipo = 1)
			select @facturaCanastillaId as facturaPOSId
		end
		else
		begin
			
			insert into FacturasCanastilla (fecha,resolucionId,consecutivo,estado,terceroid, enviada, codigoFormaPago, subtotal, descuento, iva, total,impresa)
					select GETDATE(), @ResolucionId, @consecutivoActual, 'CR',@terceroId, 0, @COD_FOR_PAG, @subtotal, @descuento, @ivaCanastila, @subtotal- @descuento +@ivaCanastila,-1
					from Resoluciones 
					WHERE @ResolucionId = ResolucionId
					
			
					select @facturaCanastillaId = SCOPE_IDENTITY()

					if @ivaPorcentaje = 1
	begin
	
					insert into FacturasCanastillaDetalle (FacturasCanastillaId,canastillaId,cantidad,precio,subtotal,iva,total)
					select @facturaCanastillaId, cids.canastillaId, cids.cantidad, cids.precio, (cids.precio*((100.0)/(100.0+cids.iva)))*cids.cantidad,(cids.precio*cids.iva/(100.0+cids.iva))*cids.cantidad,cids.precio*cids.cantidad 
					from @canastillaIds cids
					inner join Canastilla on  cids.canastillaId = Canastilla.canastillaId 
	end
	else 
	begin
	
					insert into FacturasCanastillaDetalle (FacturasCanastillaId,canastillaId,cantidad,precio,subtotal,iva,total)
					select @facturaCanastillaId, cids.canastillaId, cids.cantidad, cids.precio, (cids.precio-cids.iva)*cids.cantidad,cids.iva*cids.cantidad,cids.precio*cids.cantidad
					from @canastillaIds cids
					inner join Canastilla on  cids.canastillaId = Canastilla.canastillaId 
	end
    

			
				--exec MandarImprimirCanastilla @facturaCanastillaId
				end
				select consecutivo as facturaCanastillaId from FacturasCanastilla where @facturaCanastillaId = facturasCanastillaId
			
		
		end
		else 
		begin 
		select 0 as facturaCanastillaId
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

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'SetFacturaCanastillaImpresa')
	DROP PROCEDURE [dbo].[SetFacturaCanastillaImpresa]
GO
CREATE procedure [dbo].[SetFacturaCanastillaImpresa]
(
	@facturaCanastillaId int )
as
begin try
    
			
		Update FacturasCanastilla
				set impresa = 1
				from FacturasCanastilla
				where FacturasCanastilla.facturasCanastillaId = @facturaCanastillaId
			
    
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

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'MandarImprimirCanastilla')
	DROP PROCEDURE [dbo].[MandarImprimirCanastilla]
GO
CREATE procedure [dbo].[MandarImprimirCanastilla]
(
	@facturaCanastillaId int )
as
begin try
		declare @impresa int
		
		select @impresa = impresa from FacturasCanastilla
				where FacturasCanastilla.facturasCanastillaId = @facturaCanastillaId

		if @impresa >=0
		begin
		Update FacturasCanastilla
				set impresa = -1
				from FacturasCanastilla
				where FacturasCanastilla.facturasCanastillaId = @facturaCanastillaId
		end
		else begin
		
		Update FacturasCanastilla
				set impresa = impresa-1
				from FacturasCanastilla
				where FacturasCanastilla.facturasCanastillaId = @facturaCanastillaId
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaImprimirCanastilla')
	DROP PROCEDURE [dbo].[getFacturaImprimirCanastilla]
GO
CREATE procedure [dbo].[getFacturaImprimirCanastilla]
as
begin try
    set nocount on;



	select top(1)
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasCanastilla.[FacturasCanastillaId]
      ,FacturasCanastilla.*, terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasCanastilla
	left join dbo.Resoluciones on FacturasCanastilla.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasCanastilla.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where FacturasCanastilla.impresa <= -1
	

    
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
(@FacturaCanastillaId int)
as
begin try
    set nocount on;



	select *
	
	from dbo.FacturasCanastillaDetalle
	left join dbo.Canastilla on FacturasCanastillaDetalle.canastillaId = Canastilla.canastillaId
	where FacturasCanastillaDetalle.facturasCanastillaId  = @FacturaCanastillaId
	

    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'GetCanastilla')
	DROP PROCEDURE [dbo].[GetCanastilla]
GO
CREATE procedure [dbo].[GetCanastilla]
as
begin try
    set nocount on;



	select *
	
	from dbo.Canastilla
	where deleted = 0

    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaImprimirCanastilla')
	DROP PROCEDURE [dbo].[getFacturaEnviarCanastilla]
GO
CREATE procedure [dbo].[getFacturaEnviarCanastilla]
as
begin try
    set nocount on;



	select top(10)
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasCanastilla.[FacturasCanastillaId]
      ,FacturasCanastilla.*, terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasCanastilla
	left join dbo.Resoluciones on FacturasCanastilla.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasCanastilla.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where FacturasCanastilla.enviada = 0
	order by FacturasCanastilla.FacturasCanastillaId desc
	

    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'SetFacturaCanastillaEnviada')
	DROP PROCEDURE [dbo].[SetFacturaCanastillaEnviada]
GO
CREATE procedure [dbo].[SetFacturaCanastillaEnviada]
(
	
	@facturas [ventasIds] readonly )
as
begin try
    
			
		Update FacturasCanastilla
				set enviada = 1
				from FacturasCanastilla
    inner join @facturas f on f.ventaId = FacturasCanastilla.FacturasCanastillaId
			
    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'BuscarFacturaCanastillaPorConsecutivo')
	DROP PROCEDURE [dbo].[BuscarFacturaCanastillaPorConsecutivo]
GO
CREATE procedure [dbo].[BuscarFacturaCanastillaPorConsecutivo]
(@consecutivo INT)
as
begin try
    set nocount on;



	select 
	Resoluciones.descripcion as descripcionRes, Resoluciones.autorizacion, Resoluciones.consecutivoActual,
	Resoluciones.consecutivoFinal, Resoluciones.consecutivoInicio, Resoluciones.esPOS, Resoluciones.estado,
	Resoluciones.fechafinal, Resoluciones.fechaInicio, Resoluciones.ResolucionId, Resoluciones.habilitada, FacturasCanastilla.[FacturasCanastillaId]
      ,FacturasCanastilla.*, terceros.*, TipoIdentificaciones.*
	
	from dbo.FacturasCanastilla
	left join dbo.Resoluciones on FacturasCanastilla.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on FacturasCanastilla.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
	where FacturasCanastilla.consecutivo = @consecutivo
	

    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'FacturasCanastillaPorImprimir')
	DROP PROCEDURE [dbo].[FacturasCanastillaPorImprimir]
GO
CREATE procedure [dbo].[FacturasCanastillaPorImprimir]
as
begin try
    set nocount on;



	select FacturasCanastillaId
	from FacturasCanastilla
	where FacturasCanastilla.impresa <= -1
	

    
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


delete FacturasCanastilla from FacturasCanastilla fc
left join FacturasCanastillaDetalle fcd
on fc.FacturasCanastillaId = fcd.FacturasCanastillaId
where fcd.FacturasCanastillaDetalleId is null