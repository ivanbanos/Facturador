/*
*************************************************************************    
 Author: Ivan Ba�os    
 Create date: 2020-11-07
 Description: StoreProcedures para Base de datos estacion
**************************************************************************
History:
2020-11-07 primera version
*/
USE Estacion
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='CombustiblePrecioEstacion' and xtype='U')
BEGIN
	create table dbo.CombustiblePrecioEstacion(
		Id INT PRIMARY KEY IDENTITY (1, 1),
		descripcion VARCHAR(100) NOT NULL,
		precio FLOAT NOT NULL,
		esGas BIT NOT NULL CONSTRAINT DF_CombustiblePrecioEstacion_EsGas DEFAULT 0,
		fechaActualizacion DATETIME NOT NULL CONSTRAINT DF_CombustiblePrecioEstacion_Fecha DEFAULT GETDATE()
	);
END
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerCombustibles')
	DROP PROCEDURE [dbo].[ObtenerCombustibles]
GO
CREATE procedure [dbo].[ObtenerCombustibles]
as
begin try
	set nocount on;
	select descripcion as Descripcion, precio as Precio, esGas as EsGas, fechaActualizacion
	from dbo.CombustiblePrecioEstacion
	order by descripcion;
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ActualizarPrecioCombustible')
	DROP PROCEDURE [dbo].[ActualizarPrecioCombustible]
GO
CREATE procedure [dbo].[ActualizarPrecioCombustible]
(
	@descripcion varchar(100),
	@precio float,
	@esGas bit = 0
)
as
begin try
	set nocount on;

	update dbo.CombustiblePrecioEstacion
	set precio = @precio,
		esGas = @esGas,
		fechaActualizacion = GETDATE()
	where LTRIM(RTRIM(LOWER(descripcion))) = LTRIM(RTRIM(LOWER(@descripcion)));

	if @@ROWCOUNT = 0
	begin
		insert into dbo.CombustiblePrecioEstacion(descripcion, precio, esGas)
		values(@descripcion, @precio, @esGas);
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerCaras')
	DROP PROCEDURE [dbo].[ObtenerCaras]
GO
CREATE procedure [dbo].[ObtenerCaras]
as
begin try
    set nocount on;
	select CARAS.COD_CAR, CARAS.DESCRIPCION, CARAS.POS, CARAS.NUM_POS, CARAS.COD_SUR, ISLAS.COD_ISL IdIsla, ISLAS.DESCRIPCION Isla from dbo.CARAS
	 
	 inner join dbo.ISLAS on CARAS.COD_ISL = ISLAS.COD_ISL
	 where CARAS.ESTADO = 'A'
     
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerIslas')
	DROP PROCEDURE [dbo].[ObtenerIslas]
GO
CREATE procedure [dbo].[ObtenerIslas]
as
begin try
    set nocount on;
	select COD_ISL, DESCRIPCION from dbo.ISLAS
    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerVentaPorCara')
	DROP PROCEDURE [dbo].[ObtenerVentaPorCara]
GO
CREATE procedure [dbo].[ObtenerVentaPorCara]
(
	@COD_CAR int
)
as
begin try
    set nocount on;
	select top(1)EMPLEADO.Nombre as VENDEDOR, EMPLEADO.CEDULA as CEDULA,MANGUERA.COD_MAN, MANGUERA.COD_TANQ, MANGUERA.COD_SUR, MANGUERA.COD_CAR, ARTICULO.DESCRIPCION, MANGUERA.DS_ROM,
	v.*, CLIENTES.*, IMPRESOR.*,dbo.Finteger(AUTOMOTO.FECH_PRMA) as MANTENIMIENTO,AUTOMOTO.*, dbo.finteger(v.FECHA) FechaReporte 
	
	from dbo.VENTAS v
	left join dbo.CLIENTES on v.COD_CLI = CLIENTES.COD_CLI
	left join dbo.MANGUERA on v.COD_MAN = MANGUERA.COD_MAN
	left join dbo.TANQUES on MANGUERA.COD_TANQ = TANQUES.COD_TANQ
	left join dbo.ARTICULO on v.Cod_art = ARTICULO.COD_ART
	left join dbo.POS on v.COD_ISL = POS.COD_ISL
	left join dbo.IMPRESOR on POS.NUM_POS = IMPRESOR.NUM_POS
	left join dbo.AUTOMOTO on (v.COD_CLI = AUTOMOTO.COD_CLI and AUTOMOTO.PLACA = v.PLACA)
	left join dbo.EMPLEADO on v.COD_EMP = EMPLEADO.COD_EMP 
	where @COD_CAR = v.COD_CAR
	order by v.CONSECUTIVO desc


    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getVentaPorId')
	DROP PROCEDURE [dbo].[getVentaPorId]
GO
CREATE procedure [dbo].[getVentaPorId]
(
	@CONSECUTIVO int
)
as
begin try
    set nocount on; 

	select top(1)EMPLEADO.Nombre as VENDEDOR, EMPLEADO.CEDULA as CEDULA,MANGUERA.COD_MAN, MANGUERA.COD_TANQ, MANGUERA.COD_SUR, MANGUERA.COD_CAR, ARTICULO.DESCRIPCION, MANGUERA.DS_ROM,
	v.*, CLIENTES.*, IMPRESOR.*,dbo.Finteger(AUTOMOTO.FECH_PRMA) as MANTENIMIENTO,AUTOMOTO.*, dbo.finteger(v.FECHA) FechaReporte 
	
	from dbo.VENTAS v
	left join dbo.CLIENTES on v.COD_CLI = CLIENTES.COD_CLI
	left join dbo.MANGUERA on v.COD_MAN = MANGUERA.COD_MAN
	left join dbo.TANQUES on MANGUERA.COD_TANQ = TANQUES.COD_TANQ
	left join dbo.ARTICULO on v.Cod_art = ARTICULO.COD_ART
	left join dbo.POS on v.COD_ISL = POS.COD_ISL
	left join dbo.IMPRESOR on POS.NUM_POS = IMPRESOR.NUM_POS
	left join dbo.EMPLEADO on v.COD_EMP = EMPLEADO.COD_EMP 
	left join dbo.AUTOMOTO on (v.COD_CLI = AUTOMOTO.COD_CLI and AUTOMOTO.PLACA = v.PLACA)
	where CONSECUTIVO = @CONSECUTIVO
    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerVentasPorIds')
	DROP PROCEDURE [dbo].[ObtenerVentasPorIds]
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CambiarEstadoTerceroEnviado')
	DROP PROCEDURE [dbo].[CambiarEstadoTerceroEnviado]

	
GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CambiarEstadoFactursEnviada')
	DROP PROCEDURE [dbo].[CambiarEstadoFactursEnviada]

	
GO
IF type_id('[dbo].[ventasIds]') IS NOT NULL
        DROP TYPE [dbo].[ventasIds];
GO
CREATE TYPE [dbo].[ventasIds] AS TABLE(
	[ventaId] [int] NOT NULL
)
GO

CREATE procedure [dbo].[ObtenerVentasPorIds]
(
	@ventas [ventasIds] readonly
)
as
begin try
    set nocount on;
	select EMPLEADO.Nombre as VENDEDOR, EMPLEADO.CEDULA as CEDULA,MANGUERA.COD_MAN, MANGUERA.COD_TANQ, ARTICULO.DESCRIPCION, MANGUERA.DS_ROM,
	v.*, CLIENTES.*, IMPRESOR.*, dbo.finteger(v.FECHA) FechaReporte 
	
	from dbo.VENTAS v
	left join dbo.CLIENTES on v.COD_CLI = CLIENTES.COD_CLI
	left join dbo.MANGUERA on v.COD_MAN = MANGUERA.COD_MAN
	left join dbo.TANQUES on MANGUERA.COD_TANQ = TANQUES.COD_TANQ
	left join dbo.ARTICULO on TANQUES.Cod_art = ARTICULO.COD_ART
	left join dbo.POS on v.COD_ISL = POS.COD_ISL
	left join dbo.IMPRESOR on POS.NUM_POS = IMPRESOR.NUM_POS
	left join dbo.EMPLEADO on v.COD_EMP = EMPLEADO.COD_EMP 
	inner join @ventas vv on vv.ventaId = V.CONSECUTIVO
    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerFormasPago')
	DROP PROCEDURE [dbo].[ObtenerFormasPago]
GO
CREATE procedure [dbo].[ObtenerFormasPago]
as
begin try
    set nocount on;
	SELECT [COD_FOR_PAG]
      ,[DESCRIPCION]
  FROM [dbo].[FORM_PAG]
  order by   CASE WHEN [COD_FOR_PAG]=4 THEN 0 ELSE 1 END ASC
    
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'AgregarFacturaPorIdVenta')
	DROP PROCEDURE [dbo].[AgregarFacturaPorIdVenta]
GO
CREATE procedure [dbo].[AgregarFacturaPorIdVenta]
(
	@ventaId int
)
as
begin try
    set nocount on;
	declare @terceroId int, @Placa varchar(50), @Kilometraje varchar(50), @COD_FOR_PAG smallint, @fecha datetime,
			@descripcionCombustible varchar(100), @precioCombustible float,
			@turnoGuid varchar(50), @fechaTurno int, @numeroTurno int, @islaTurno int;

	
	declare @COD_CLI varchar(15), @identificacion varchar(50);

	select @ventaId = i.CONSECUTIVO,
	@Placa = i.PLACA,
	@Kilometraje = i.KIL_ACT,
	@COD_FOR_PAG = i.COD_FOR_PAG,
	@COD_CLI = i.COD_CLI,
	@fecha = dbo.Finteger(i.FECHA_REAL) + dbo.HINTEGER(i.hora),
	@fechaTurno = i.FECHA_REAL,
	@numeroTurno = i.NUM_TUR,
	@islaTurno = i.COD_ISL
	from VENTAS i
	where i.CONSECUTIVO = @ventaId

	if @islaTurno is not null and (@fechaTurno is null or @numeroTurno is null)
	begin
		select top(1)
			@fechaTurno = isnull(@fechaTurno, t.FECHA),
			@numeroTurno = isnull(@numeroTurno, t.NUM_TUR)
		from TURN_EST t
		where t.COD_ISL = @islaTurno
		  and (
				(@fechaTurno is not null and t.FECHA = @fechaTurno)
				or @fechaTurno is null
			  )
		order by case when t.estado != 'C' then 0 else 1 end, t.FECHA desc, t.NUM_TUR desc;
	end

	if @fechaTurno is not null and @numeroTurno is not null and @islaTurno is not null
	begin
		select @turnoGuid = CONVERT(varchar(36), CONVERT(uniqueidentifier, HASHBYTES('MD5', CONCAT(CONVERT(varchar(20), @fechaTurno), '|', CONVERT(varchar(20), @islaTurno), '|', CONVERT(varchar(20), @numeroTurno)))))
	end
	
	select @identificacion = nit from CLIENTES WHERE @COD_CLI = COD_CLI

	select top(1)
		@descripcionCombustible = a.DESCRIPCION,
		@precioCombustible = convert(float, i.PRECIO_UNI)
	from VENTAS i
	left join ARTICULO a on a.COD_ART = i.COD_ART
	where i.CONSECUTIVO = @ventaId


    select @terceroId = terceroId
	from Facturacion_Electronica.dbo.terceros t
	where t.identificacion = @identificacion

	If @terceroId is null
	begin
		declare @tipoIdentificacion int,
		@nombre VARCHAR (50),
		@telefono VARCHAR (50),
		@correo VARCHAR (50),
		@direccion VARCHAR (50)

		
		declare @descTipoInt varchar(50)

		select @descTipoInt = c.TIPO_NIT
		from CLIENTES c
		inner join VENTAS i on i.COD_CLI = c.COD_CLI 
		where i.CONSECUTIVO = @ventaId

		select @tipoIdentificacion = TipoIdentificacionId
		from Facturacion_Electronica.dbo.TipoIdentificaciones ti
		where ti.descripcion = @descTipoInt
		
		select 
		@identificacion = c.NIT,
		@nombre = c.NOMBRE,
		@telefono = c.TEL_PARTICULAR,
		@direccion = c.DIR_PARTICULAR,
		@COD_CLI = c.COD_CLI
		from CLIENTES c 
		where c.NIT = @identificacion

		If @tipoIdentificacion is null
		Begin
			select @tipoIdentificacion = TipoIdentificacionId 
			from Facturacion_Electronica.dbo.TipoIdentificaciones ti
			where ti.descripcion = 'No especificada'
		End
		if @identificacion is not null
		begin
		select @terceroId = t.terceroId  from Facturacion_Electronica.dbo.terceros t
			where @identificacion = t.identificacion
			if @terceroId is null
			begin
			
		insert into Facturacion_Electronica.dbo.terceros(COD_CLI,correo,direccion,estado,identificacion,nombre,telefono,tipoIdentificacion)
		values(@COD_CLI, 'no informado', @direccion, 'AC', @identificacion, @nombre, @telefono, @tipoIdentificacion)
			select @terceroId = SCOPE_IDENTITY()
			end

		end
		else
		begin
			
			select @terceroId = t.terceroId from Facturacion_Electronica.dbo.terceros t
			where t.nombre like '%CONSUMIDOR FINAL%' and identificacion like '222222222222'
			if @terceroId is null
			begin
			insert into Facturacion_Electronica.dbo.terceros(COD_CLI,correo,direccion,estado,identificacion,nombre,telefono,tipoIdentificacion)
			values(@COD_CLI, 'no informado', 'no informado', 'AC', '222222222222', 'CONSUMIDOR FINAL', 'no informado', @tipoIdentificacion)

			select @terceroId = SCOPE_IDENTITY()
			end
		end
	end

	--if @descripcionCombustible is not null and @precioCombustible > 0
	--begin
	--	exec [dbo].[ActualizarPrecioCombustible] @descripcionCombustible, @precioCombustible,
	--		case
	--			when lower(replace(replace(@descripcionCombustible, '.', ''), ' ', '')) like '%gnvc%'
	--				or lower(@descripcionCombustible) like '%gas%'
	--			then 1 else 0
	--		end;
	--end
	exec Facturacion_Electronica.dbo.CrearFactura @ventaId, @terceroId, @Placa, @Kilometraje, @COD_FOR_PAG, @fecha, @turnoGuid = @turnoGuid
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'AgregarFacturaDesdeIdVenta')
	DROP PROCEDURE [dbo].AgregarFacturaDesdeIdVenta
GO
CREATE procedure [dbo].[AgregarFacturaDesdeIdVenta]
as
begin try
    set nocount on;
	declare @ventaMin int, @ventaMax int, @ventaActual int
	select @ventaMin = min(ventaId)
	from Facturacion_Electronica.dbo.FacturasPOS
	where fecha > DATEADD(day, -1,  CAST( GETDATE() AS Date ) )

	
	select @ventaMin = case when @ventaMin > min(ventaId) then  min(ventaId) else @ventaMin end
	from Facturacion_Electronica.dbo.OrdenesDeDespacho
	where fecha > DATEADD(day, -1,  CAST( GETDATE() AS Date ) )

	WHILE @ventaMin is not null
	BEGIN
		select @ventaMin = MIN(VENTAS.CONSECUTIVO)
		FROM VENTAS
		left join Facturacion_Electronica.dbo.FacturasPOS as FacturasPOS on FacturasPOS.ventaId = VENTAS.CONSECUTIVO
		left join Facturacion_Electronica.dbo.OrdenesDeDespacho as OrdenesDeDespacho on OrdenesDeDespacho.ventaId = VENTAS.CONSECUTIVO
		Where VENTAS.CONSECUTIVO > @ventaMin
		and FacturasPOS.facturaPOSId is null
		and OrdenesDeDespacho.facturaPOSId is null
		
		if @ventaMin is not null
		begin
			exec [AgregarFacturaPorIdVenta] @ventaMin
		end


	END
	
		select 'OK'
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
declare @tipoIdentificacion int
declare @tercerosAcrear as table(identificacion nvarchar(50), nombre nvarchar(50), telefono nvarchar(50), direccion nvarchar(50), cod_cli char(15))
select @tipoIdentificacion = TipoIdentificacionId 
			from Facturacion_Electronica.dbo.TipoIdentificaciones ti
			where ti.descripcion = 'No especificada'
insert into @tercerosAcrear(identificacion, nombre, telefono , direccion, cod_cli)
select CONVERT(varchar,c.NIT), CONVERT(varchar,c.NOMBRE), isnull(c.TEL_OFICINA, isnull(c.TEL_MOVIL, 'No informado')),
 CONVERT(varchar,isnull(c.DIR_PARTICULAR, isnull(c.DIR_OFICINA, 'No informado'))), c.COD_CLI
from CLIENTES c 

select * from @tercerosAcrear

insert into Facturacion_Electronica.dbo.terceros(COD_CLI,correo,direccion,estado,identificacion,nombre,telefono,tipoIdentificacion)
select c.COD_CLI, 'No informado', c.direccion,'AC', c.identificacion, c.nombre,
c.telefono, @tipoIdentificacion
		from @tercerosAcrear c 
		left join Facturacion_Electronica.dbo.terceros t on t.identificacion collate Modern_Spanish_CI_AS = c.identificacion
		where t.terceroId is null
GO

GO
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'BuscarFechasReportesNoEnviadas')
	DROP PROCEDURE [dbo].BuscarFechasReportesNoEnviadas
GO
CREATE procedure [dbo].[BuscarFechasReportesNoEnviadas]
as
begin try
    set nocount on;
	select top(50) v.CONSECUTIVO as IdVentaLocal, dbo.finteger(v.FECHA) FechaReporte from VENTAS v
	left JOIN  Facturacion_Electronica.dbo.FacturasPOS f ON f.ventaId = v.CONSECUTIVO
	left JOIN  Facturacion_Electronica.dbo.ORdenesdedespacho o ON o.ventaId = v.CONSECUTIVO
	
	WHERE (f.ventaID is not null and (f.reporteEnviado is null or f.reporteEnviado = 0) and f.enviada = 1 	) or
	(o.ventaID is not null and (o.reporteEnviado is null or o.reporteEnviado = 0) and o.enviada = 1 	)
	order by v.consecutivo desc 
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerCodigoInterno')
	DROP PROCEDURE [dbo].[ObtenerCodigoInterno]
GO
CREATE procedure [dbo].[ObtenerCodigoInterno]
(
	@PLACA char(9),
	@identificacion varchar(20)

)
as
begin try
    set nocount on;
	select top(1)a.COD_INT
	from dbo.AUTOMOTO a
	left join dbo.CLIENTES c on a.COD_CLI = c.COD_CLI
    where a.PLACA = @PLACA and c.NIT = @identificacion
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
use Ventas
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'setKilimetrajeVenta')
	DROP PROCEDURE [dbo].[setKilimetrajeVenta]
GO
CREATE procedure [dbo].[setKilimetrajeVenta]
(
	@ventaId int,
	@Kilometraje varchar(50) = null,
	@placa varchar(9) = null,
	@codigoFormaPago int = null

)
as
begin try
    set nocount on;
	declare @kilometrajeNum decimal(14,2)

	select @kilometrajeNum =CONVERT(float, @Kilometraje)
	where ISNUMERIC(@Kilometraje+ 'e0')=1 
	
	update VENTAS set KIL_ACT = CONVERT(DECIMAL(14, 2), @kilometrajeNum) 
	where @kilometrajeNum<100000000000.00 and CONSECUTIVO = @ventaId
	
	update VENTAS set PLACA = @placa
	where CONSECUTIVO = @ventaId

	
	update VENTAS set COD_FOR_PAG = @codigoFormaPago
	where CONSECUTIVO = @ventaId
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

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerTurnoIsla')
	DROP PROCEDURE [dbo].[ObtenerTurnoIsla]
GO
CREATE procedure [dbo].[ObtenerTurnoIsla]
(@IdIsla int)
as
begin try
    set nocount on;
	select  NUM_TUR as numero, EMPLEADO.NOMBRE as empleado, ISLAS.DESCRIPCION as Isla, 0 IdEstado, dbo.Finteger(FECHA) as FechaApertura ,dbo.Finteger(FECHA)  as FechaCierre ,  FECHA
 from TURN_EST
inner join EMPLEADO On EMPLEADO.COD_EMP = TURN_EST.COD_EMP
inner join ISLAS On ISLAS.COD_ISL = TURN_EST.COD_ISL
 where estado != 'C' and ISLAS.COD_ISL = @IdISla
     
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
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerTurnoIslaPorVentaId')
	DROP PROCEDURE [dbo].[ObtenerTurnoIslaPorVenta]
GO
CREATE procedure [dbo].[ObtenerTurnoIslaPorVenta]
(@ventaId int)
as
begin try
    set nocount on;
	select  TURN_EST.NUM_TUR as Numero, EMPLEADO.NOMBRE as empleado, ISLAS.DESCRIPCION as Isla, 0 IdEstado, dbo.Finteger(TURN_EST.FECHA) as FechaApertura ,dbo.Finteger(TURN_EST.FECHA) as FechaCierre, TURN_EST.FECHA 
 from TURN_EST
inner join EMPLEADO On EMPLEADO.COD_EMP = TURN_EST.COD_EMP
inner join ISLAS On ISLAS.COD_ISL = TURN_EST.COD_ISL
inner join VENTAS On VENTAS.FECHA_REAL = TURN_EST.FECHA and VENTAS.NUM_TUR = TURN_EST.NUM_TUR and VENTAS.COD_ISL = TURN_EST.COD_ISL
 where VENTAS.CONSECUTIVO = @ventaId

 select TURN_LEC.COD_MAN as Manguera, TURN_LEC.COD_SUR as Surtidor, LECT_INI1 as Apertura, LECT_FIN1 as Cierre, ARTICULO.Descripcion as  Combustible, TURN_LEC.PRECIO as precioCombustible
 from TURN_LEC
inner join VENTAS On VENTAS.FECHA_REAL = TURN_LEC.FECHA and VENTAS.NUM_TUR = TURN_LEC.NUM_TUR
inner join ARTICULO On ARTICULO.COD_ART = TURN_LEC.COD_ART1
 where VENTAS.CONSECUTIVO = @ventaId
     
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


IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerTurnoIslaCerrado')
	DROP PROCEDURE [dbo].[ObtenerTurnoIslaCerrado]
GO
CREATE procedure [dbo].[ObtenerTurnoIslaCerrado]
(@IdIsla int)
as
begin try
    set nocount on;
	select  NUM_TUR as Id, EMPLEADO.NOMBRE, ISLAS.DESCRIPCION as Isla, 0 IdEstado, dbo.Finteger(FECHA) as FechaApertura ,null as FechaCierre , FECHA
 from TURN_EST
inner join EMPLEADO On EMPLEADO.COD_EMP = TURN_EST.COD_EMP
inner join ISLAS On ISLAS.COD_ISL = TURN_EST.COD_ISL
 where estado = 'C' and ISLAS.COD_ISL = @IdISla 
 order by FechaApertura desc, NUM_TUR desc
     
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
ALTER procedure [dbo].[ObtenerTurnoIslaYFecha]
(@IdIsla int, @num_tur int, @fecha datetime)
as
begin try
    set nocount on;
	select  NUM_TUR as Numero, EMPLEADO.NOMBRE as empleado, ISLAS.DESCRIPCION as Isla, 0 IdEstado, dbo.Finteger(FECHA) + dbo.HINTEGER(HORA_INI)  as FechaApertura , dbo.Finteger(FECHA) + dbo.HINTEGER(HORA_FIN) as FechaCierre , FECHA
 from TURN_EST
inner join EMPLEADO On EMPLEADO.COD_EMP = TURN_EST.COD_EMP
inner join ISLAS On ISLAS.COD_ISL = TURN_EST.COD_ISL
 where  ISLAS.COD_ISL = @IdISla and NUM_TUR = @num_tur and dbo.Finteger(FECHA) = @fecha
      
	  select TURN_LEC.COD_MAN as Manguera, TURN_LEC.COD_SUR as Surtidor, LECT_INI1 as Apertura, LECT_FIN1 as Cierre, ARTICULO.Descripcion as  Combustible, TURN_LEC.PRECIO as precioCombustible
 from TURN_LEC
inner join ARTICULO On ARTICULO.COD_ART = TURN_LEC.COD_ART1
inner join ISLAS On ISLAS.COD_ISL = TURN_LEC.COD_ISL
 where ISLAS.COD_ISL = @IdISla and NUM_TUR = @num_tur and dbo.Finteger(FECHA) = @fecha

 select dbo.Finteger(FECHA) as Fecha, Consecutivo, NUM_TUR as NumeroTurno,ISLAS.DESCRIPCION as Isla,
EMPLEADO.Nombre as Empleado, VR_MONEDA as Moneda, VR_BILLETE as Billete
from BOLS_TUR

inner join EMPLEADO On EMPLEADO.COD_EMP = BOLS_TUR.COD_EMP
inner join ISLAS On ISLAS.COD_ISL = BOLS_TUR.COD_ISL
where dbo.Finteger(BOLS_TUR.FECHA) = @fecha and
BOLS_TUR.COD_ISL = @IdISla
and BOLS_TUR.NUM_TUR =@num_tur
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