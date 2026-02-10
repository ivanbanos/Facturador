use cootranshuila
 

IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Mangueras' AND COLUMN_NAME = 'Manguera')
BEGIN
CREATE TABLE [dbo].[Mangueras]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Manguera] NVARCHAR(50) NULL, 
    [Cara] NVARCHAR(50) NULL, 
    [Surtidor] NVARCHAR(50) NULL
)
END
GO

IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Vendedor' AND COLUMN_NAME = 'Nombre')
BEGIN
CREATE TABLE [dbo].[Vendedor]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Nombre] NVARCHAR(50) NULL
)
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Vendedor' AND COLUMN_NAME = 'Cedula')
BEGIN
ALTER TABLE [dbo].[Vendedor]
Add [Cedula] NVARCHAR(50) NULL;
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'FormaDePago' AND COLUMN_NAME = 'Nombre')
BEGIN
CREATE TABLE [dbo].[FormaDePago]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Nombre] NVARCHAR(50) NULL
)
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Combustible' AND COLUMN_NAME = 'Nombre')
BEGIN
CREATE TABLE [dbo].[Combustible]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Nombre] NVARCHAR(50) NULL
)
END
GO
insert into Vendedor(Nombre)
select distinct Vendedor from Facturas
left join Vendedor on Vendedor.Nombre = Facturas.Vendedor
where Vendedor.Id is null
AND  Vendedor is not null
insert into Vendedor(Nombre)
select distinct Vendedor from OrdenesDeDespacho
left join Vendedor on Vendedor.Nombre = OrdenesDeDespacho.Vendedor
where Vendedor.Id is null
AND 
Vendedor is not null

GO
insert into Combustible(Nombre)
select distinct Combustible from Facturas
left join Combustible on Combustible.Nombre = Facturas.Combustible
where Combustible.Id is null
AND  Combustible is not null
insert into Combustible(Nombre)
select distinct Combustible from OrdenesDeDespacho
left join Combustible on Combustible.Nombre = OrdenesDeDespacho.Combustible
where Combustible.Id is null
AND 
Combustible is not null

GO
insert into FormaDePago(Nombre)
select distinct FormaDePago from Facturas
left join FormaDePago on FormaDePago.Nombre = Facturas.FormaDePago
where FormaDePago.Id is null
AND  FormaDePago is not null
insert into FormaDePago(Nombre)
select distinct FormaDePago from OrdenesDeDespacho
left join FormaDePago on FormaDePago.Nombre = OrdenesDeDespacho.FormaDePago
where FormaDePago.Id is null
AND 
FormaDePago is not null

GO

insert into Mangueras(Surtidor, Cara, Manguera)
select Distinct Facturas.Surtidor, Facturas.Cara, Facturas.Manguera from Facturas
left join Mangueras on Mangueras.Surtidor = Facturas.Surtidor 
and Mangueras.Cara = Facturas.Cara
and Mangueras.Manguera = Facturas.Manguera
where Mangueras.Id is null
AND  Facturas.Surtidor is not null
AND  Facturas.Cara is not null
AND  Facturas.Manguera is not null
insert into Mangueras(Surtidor, Cara, Manguera)
select Distinct OrdenesDeDespacho.Surtidor, OrdenesDeDespacho.Cara, OrdenesDeDespacho.Manguera from OrdenesDeDespacho
left join Mangueras on Mangueras.Surtidor = OrdenesDeDespacho.Surtidor 
and Mangueras.Cara = OrdenesDeDespacho.Cara
and Mangueras.Manguera = OrdenesDeDespacho.Manguera
where Mangueras.Id is null
AND  OrdenesDeDespacho.Surtidor is not null
AND  OrdenesDeDespacho.Cara is not null
AND  OrdenesDeDespacho.Manguera is not null
GO

IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Facturas' AND COLUMN_NAME = 'idVendedor')
BEGIN
ALTER TABLE [dbo].[Facturas]
Add idVendedor int Null;
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'idVendedor')
BEGIN
ALTER TABLE [dbo].[OrdenesDeDespacho]
Add idVendedor int Null;
END
GO

IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Facturas' AND COLUMN_NAME = 'idCombustible')
BEGIN
ALTER TABLE [dbo].[Facturas]
Add idCombustible int Null;
END
GO

IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Facturas' AND COLUMN_NAME = 'IdFormaDePago')
BEGIN
ALTER TABLE [dbo].[Facturas]
Add IdFormaDePago int Null;
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'idCombustible')
BEGIN
ALTER TABLE [dbo].[OrdenesDeDespacho]
Add idCombustible int Null;
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'IdFormaDePago')
BEGIN
ALTER TABLE [dbo].[OrdenesDeDespacho]
Add IdFormaDePago int Null;
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Facturas' AND COLUMN_NAME = 'idMangueras')
BEGIN
ALTER TABLE [dbo].[Facturas]
Add idMangueras int Null;
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'idMangueras')
BEGIN
ALTER TABLE [dbo].[OrdenesDeDespacho]
Add idMangueras int Null;
END
GO
Update Facturas set idCombustible = Combustible.Id
from Facturas 
inner join Combustible On Combustible.Nombre = Facturas.Combustible
GO
Update OrdenesDeDespacho set idCombustible = Combustible.Id
from OrdenesDeDespacho 
inner join Combustible On Combustible.Nombre = OrdenesDeDespacho.Combustible
GO

Update Facturas set IdFormaDePago = FormaDePago.Id
from Facturas 
inner join FormaDePago On FormaDePago.Nombre = Facturas.FormaDePago
GO
Update OrdenesDeDespacho set IdFormaDePago = FormaDePago.Id
from OrdenesDeDespacho 
inner join FormaDePago On FormaDePago.Nombre = OrdenesDeDespacho.FormaDePago
GO
Update Facturas set idMangueras = Mangueras.Id
from Facturas 
left join Mangueras on Mangueras.Surtidor = Facturas.Surtidor 
and Mangueras.Cara = Facturas.Cara
and Mangueras.Manguera = Facturas.Manguera
GO
Update OrdenesDeDespacho set idMangueras = Mangueras.Id
from OrdenesDeDespacho 
left join Mangueras on Mangueras.Surtidor = OrdenesDeDespacho.Surtidor 
and Mangueras.Cara = OrdenesDeDespacho.Cara
and Mangueras.Manguera = OrdenesDeDespacho.Manguera
GO

Update Facturas set idVendedor = Vendedor.Id
from Facturas 
inner join Vendedor On vendedor.Nombre = Facturas.Vendedor
GO
Update OrdenesDeDespacho set idVendedor = Vendedor.Id
from OrdenesDeDespacho 
inner join Vendedor On vendedor.Nombre = OrdenesDeDespacho.Vendedor
GO

IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'terceros' AND COLUMN_NAME = 'Segundo')
BEGIN
  
ALTER TABLE [dbo].[Terceros]
Add Segundo NVARCHAR(250) NOT NULL default 'No',
	Apellidos NVARCHAR(250) NOT NULL default 'No',
	TipoPersona int default 0,
	ResponsabilidadTributaria int default 0,
	Municipio NVARCHAR(250) NOT NULL default 'No',
	Departamento NVARCHAR(250) NOT NULL default 'No',
	Pais NVARCHAR(250) NULL,
	CodigoPostal NVARCHAR(250) NULL,
	Celular NVARCHAR(250) NULL,
	Telefono2 NVARCHAR(250) NULL,
	Correo2 NVARCHAR(250) NULL,
	Vendedor NVARCHAR(250) NULL,
	Comentarios NVARCHAR(500) NULL;
END;

GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'terceros' AND COLUMN_NAME = 'idFacturacion')
BEGIN
ALTER TABLE [dbo].[Terceros]
Add idFacturacion varchar(250) Null;
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'facturas' AND COLUMN_NAME = 'idFacturaElectronica')
BEGIN
ALTER TABLE [dbo].[Facturas]
Add idFacturaElectronica varchar(250),
razonAnulacion  varchar(250);
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'idFacturaElectronica')
BEGIN
ALTER TABLE [dbo].[OrdenesDeDespacho]
Add idFacturaElectronica varchar(250),
razonAnulacion  varchar(250);
END
GO
drop procedure AgregarOrdenDespacho
DROP type [OrdenesDeDespachoType]
CREATE TYPE [dbo].[OrdenesDeDespachoType] AS TABLE 
([Guid] UNIQUEIDENTIFIER NULL, 
	IdFactura INT NULL,
	Identificacion NVARCHAR(250),
	NombreTercero NVARCHAR(250),
	Combustible NVARCHAR(50),
	Cantidad Decimal(18,3),
	Precio Decimal(18,3),
	Total Decimal(18,3) NULL,
	Descuento Decimal(18,3) NULL,
	IdInterno NVARCHAR(50),
	Placa NVARCHAR(50),
	Kilometraje NVARCHAR(50),
	IdEstadoActual Int NULL,
	Surtidor NVARCHAR(50),
	Cara NVARCHAR(50),
	Manguera NVARCHAR(50),  
    Fecha DATETIME,
	Estado NVARCHAR(50),  
	IdentificacionTercero NVARCHAR(250) ,
	FormaDePago NVARCHAR(250) ,
	IdLocal Int  NULL,
	IdVentaLocal Int  NULL,
	IdTerceroLocal Int  NULL,
    [IdEstacion] INT NULL,
	[SubTotal] DECIMAL(18, 3) NOT NULL, 
    [FechaProximoMantenimiento] DATETIME NOT NULL, 
    [Vendedor] NVARCHAR(50) NULL,
	idFacturaElectronica varchar(250)
	);  
GO
drop procedure GetFacturasImprimir
drop procedure AgregarFactura
drop procedure SetIdFacturacion
drop type [FacturaType]
CREATE TYPE [dbo].[FacturaType] AS TABLE
(
	[Id] INT,
    [Guid] UNIQUEIDENTIFIER NOT NULL,
	[Consecutivo] INT  NULL,
	IdTercero UNIQUEIDENTIFIER NULL,
	Identificacion NVARCHAR(250),
	NombreTercero NVARCHAR(250),
	Combustible NVARCHAR(50),
	Cantidad Decimal(18,3),
	Precio Decimal(18,3),
	Total Decimal(18,3)  NULL,
	Descuento Decimal(18,3) NULL,
	IdInterno NVARCHAR(50),
	Placa NVARCHAR(50),
	Kilometraje NVARCHAR(50),
	IdResolucion UNIQUEIDENTIFIER  NULL,
	IdEstadoActual Int  NULL,
	Surtidor NVARCHAR(50),
	Cara NVARCHAR(50),
	Manguera NVARCHAR(50), 
    [Fecha] DATETIME NULL,
	Estado NVARCHAR(50),  
	IdentificacionTercero NVARCHAR(250),
	FormaDePago NVARCHAR(250) ,
	IdLocal Int  NULL,
	IdVentaLocal Int  NULL,
	IdTerceroLocal Int  NULL,
    [IdEstacion] INT NULL,
	[SubTotal] DECIMAL(18, 3) NULL, 
    [FechaProximoMantenimiento] DATETIME NULL, 
    [Vendedor] NVARCHAR(50) NULL,
    [DescripcionResolucion] varchar(50) NULL,
    [AutorizacionResolucion] varchar(50) NULL,
	idFacturaElectronica varchar(250)
	
)

GO
drop procedure UpdateOrCreateTerceros
DROP type [dbo].[TerceroType] 
drop PROCEDURE [dbo].[GetTercerosActualizados]
CREATE TYPE [dbo].[TerceroType] AS TABLE
(
	[Id] INT ,
    [Guid] uniqueidentifier NULL,
	Nombre NVARCHAR(250) NULL,
	Segundo NVARCHAR(250) NULL,
	Apellidos NVARCHAR(250) NULL,
	Municipio NVARCHAR(250) NULL,
	Departamento NVARCHAR(250) NULL,
	Direccion NVARCHAR(250) NULL,
	TipoPersona int null,
	ResponsabilidadTributaria int null,
	Pais NVARCHAR(250) NULL,
	CodigoPostal NVARCHAR(250) NULL,
	Celular NVARCHAR(250) NULL,
	Telefono NVARCHAR(250) NULL,
	Telefono2 NVARCHAR(250) NULL,
	Correo NVARCHAR(250) NULL,
	Correo2 NVARCHAR(250) NULL,
	Vendedor NVARCHAR(250) NULL,
	Comentarios NVARCHAR(500) NULL,
	TipoIdentificacion INT NULL,
	Identificacion NVARCHAR(250) NULL,
	DescripcionTipoIdentificacion nvarchar(200) NULL,
	IdLocal Int  NULL,
	IdContable int null,
	idFacturacion varchar(250) NULL
)
GO

create PROCEDURE [dbo].[GetTercerosActualizados]
	@estacion UNIQUEIDENTIFIER = null
AS
BEGIN

DECLARE @idTerceroTable as table (idTercero int)



SELECT [Terceros].[Guid], Nombre, segundo, apellidos, tipoPersona, responsabilidadTributaria, Municipio, departamento,
	Direccion, PAis, codigoPostal, celular, Telefono, Telefono2, correo2, vendedor, comentarios, Correo, 
		   [Terceros].[TipoIdentificacion], [Terceros].[Identificacion],[Terceros].[IdLocal], TipoIdentificacion.Texto AS DescripcionTipoIdentificacion
	FROM [dbo].[Terceros]
	JOIN [dbo].TipoIdentificacion
		ON [Terceros].TipoIdentificacion = TipoIdentificacion.Id
	JOIN @idTerceroTable O ON o.idTercero = [Terceros].IdLocal

END
GO

create PROCEDURE [dbo].[UpdateOrCreateTerceros]
	@Tercero dbo.[TerceroType] READONLY
AS
BEGIN
	Declare @IdTextoTipoIdentificcion as table(id int null, texto nvarchar(200) null)


	insert into @IdTextoTipoIdentificcion(id, texto)
	select Distinct TipoIdentificacion.Id,t.DescripcionTipoIdentificacion  FROM @Tercero t
	left join TipoIdentificacion ON TipoIdentificacion.Texto = t.DescripcionTipoIdentificacion

	insert into TipoIdentificacion([Guid],Texto)
	select NEWID(), texto
	from @IdTextoTipoIdentificcion
	where id is null

	UPDATE Terceros SET Terceros.Nombre =  isnull(Source.Nombre,'no informado'),
				   Terceros.segundo = isnull(Source.segundo,'no informado'),
				   Terceros.apellidos = isnull(Source.apellidos,'no informado'),
				   Terceros.tipoPersona = isnull(Source.tipoPersona,Terceros.TipoPersona),
				   Terceros.responsabilidadTributaria = isnull(Source.responsabilidadTributaria,Terceros.ResponsabilidadTributaria),
				   Terceros.Municipio = isnull(Source.Municipio,'no informado'),
				   Terceros.departamento = isnull(Source.departamento,'no informado'),
				   Terceros.Direccion = isnull(Source.Direccion,'no informado'),
				   Terceros.PAis = Source.PAis,
				   Terceros.codigoPostal = Source.codigoPostal,
				   Terceros.celular = Source.celular,
				   Terceros.Telefono = isnull(Source.Telefono,Terceros.Telefono),
				   Terceros.Telefono2 = Source.Telefono2,
				   Terceros.comentarios = Source.comentarios,
				   Terceros.vendedor = Source.vendedor,
				   Terceros.correo2 = isnull(Source.correo2,'no informado'),
				   Terceros.Correo = isnull(Source.Correo,'no informado'),
				   Terceros.IdLocal = Source.IdLocal,
				   Terceros.TipoIdentificacion = isnull(TipoIdentificacion.Id,1),
				   Terceros.IdFacturacion = isnull(source.IdFacturacion,Terceros.IdFacturacion)
		from [dbo].[Terceros] Terceros
		inner join  @Tercero source on Source.Identificacion = Terceros.Identificacion
		left join TipoIdentificacion ON TipoIdentificacion.Texto = source.DescripcionTipoIdentificacion
		 
		 INSERT into terceros([Guid], Nombre, segundo, apellidos, tipoPersona, responsabilidadTributaria, Municipio, departamento,
		Direccion, PAis, codigoPostal, celular, Telefono, Telefono2, correo2, vendedor, comentarios, Correo, TipoIdentificacion, Identificacion, IdLocal,IdFacturacion)
		select NEWID(),  isnull(Source.Nombre,'no informado'), isnull(Source.segundo,'no informado'),isnull(Source.apellidos,'no informado'),
		isnull(Source.tipoPersona,0),isnull(Source.responsabilidadTributaria,0),isnull(Source.Municipio,'no informado'),
		isnull(Source.departamento,'no informado'),isnull(Source.Direccion,'no informado'),Source.PAis,Source.codigoPostal,
		Source.celular,isnull(Source.Telefono,'no informado'),Source.Telefono2, Source.Correo2,Source.vendedor,Source.comentarios, Source.Correo, isnull(TipoIdentificacion.id,1), Source.Identificacion, Source.IdLocal, source.IdFacturacion
		from @Tercero source 
		left join terceros on Source.Identificacion = terceros.Identificacion
		left join TipoIdentificacion ON TipoIdentificacion.Texto = source.DescripcionTipoIdentificacion
		where terceros.Id is null

	
END

GO
declare @idLocales as table(Identificacion NVARCHAR(250), maxID int)
insert into @idLocales(Identificacion, maxID)
SELECT top(10)  Identificacion, max(id)
FROM Terceros
GROUP BY Identificacion
HAVING COUNT(Identificacion) >1
ORDER BY Identificacion desc ;

update Facturas
set IdTercero = maxID
from Facturas
inner join Terceros on Facturas.IdTercero = Terceros.Id
inner join @idLocales idl on Terceros.Identificacion = idl.Identificacion

update OrdenesDeDespacho
set IdTercero = maxID
from OrdenesDeDespacho
inner join Terceros on OrdenesDeDespacho.IdTercero = Terceros.Id
inner join @idLocales idl on Terceros.Identificacion = idl.Identificacion


delete Terceros from Terceros
inner join @idLocales idl on Terceros.Identificacion = idl.Identificacion
where Terceros.Id != idl.maxid
GO
ALTER PROCEDURE [dbo].[GetTercero]
	@IdTercero UNIQUEIDENTIFIER
AS
BEGIN
	SELECT idFacturacion,[Terceros].[Guid], Nombre, segundo, apellidos, tipoPersona, responsabilidadTributaria, Municipio, departamento,
	Direccion, PAis, codigoPostal, celular, Telefono, Telefono2, correo2, vendedor, comentarios, Correo, 
		   [Terceros].[TipoIdentificacion], [Terceros].[Identificacion],[Terceros].[IdLocal], TipoIdentificacion.Texto AS DescripcionTipoIdentificacion
	FROM [dbo].[Terceros]
	JOIN [dbo].TipoIdentificacion
		ON [Terceros].TipoIdentificacion = TipoIdentificacion.Id
	WHERE [Terceros].[Guid] = @IdTercero
END
GO
ALTER PROCEDURE [dbo].[GetTerceros]
AS
BEGIN
	SELECT idFacturacion,[Terceros].[Guid], Nombre, segundo, apellidos, tipoPersona, responsabilidadTributaria, Municipio, departamento,
	Direccion, PAis, codigoPostal, celular, Telefono, Telefono2, correo2, vendedor, comentarios, Correo, 
		   [Terceros].[TipoIdentificacion], [Terceros].[Identificacion],[Terceros].[IdLocal], TipoIdentificacion.Texto AS DescripcionTipoIdentificacion
	FROM [dbo].[Terceros]
	JOIN [dbo].TipoIdentificacion
		ON [Terceros].TipoIdentificacion = TipoIdentificacion.Id
END
GO

CREATE PROCEDURE [dbo].[SetIdFacturacion]
(
@idFacturacion varchar(250),
@guid uniqueidentifier
)
AS
BEGIN
	Update Terceros
	set idFacturacion = @idFacturacion
	from terceros 
	where [GUID] = @guid

	
	return 0;
END
GO

drop PROCEDURE [dbo].[SetIdFacturaElectronicaFactura]
GO
create PROCEDURE [dbo].[SetIdFacturaElectronicaFactura]
(
@idFacturaElectronica varchar(250),
@guid uniqueidentifier
)
AS
BEGIN
	
	DECLARE @IdEstadoAnulado int;

	SELECT @IdEstadoAnulado = [Estados].Id FROM [dbo].[Estados] WHERE [Estados].Texto = 'Anulado'

	Update Facturas
	set idFacturaElectronica = @idFacturaElectronica,
	Facturas.IdEstadoActual = @IdEstadoAnulado
	from facturas 
	where [GUID] = @guid
	
	return 0;
END
GO
drop PROCEDURE [dbo].[SetIdFacturaElectronicaOrdenesdeDespacho]
GO
create PROCEDURE [dbo].[SetIdFacturaElectronicaOrdenesdeDespacho]
(
@idFacturaElectronica varchar(250),
@guid uniqueidentifier
)
AS
BEGIN
	
	DECLARE @IdEstadoAnulado int;

	SELECT @IdEstadoAnulado = [Estados].Id FROM [dbo].[Estados] WHERE [Estados].Texto = 'Anulado'

	Update OrdenesDeDespacho
	set idFacturaElectronica = @idFacturaElectronica,
	OrdenesDeDespacho.IdEstadoActual = @IdEstadoAnulado
	from OrdenesDeDespacho 
	where [GUID] = @guid

	return 0;
END
GO

ALTER PROCEDURE [dbo].[GetFacturas]
	@FechaInicial DATETIME = '1990-01-01',
	@FechaFinal DATETIME = '2100-01-01',
	@IdentificacionTercero NVARCHAR(250) = NULL,
	@NombreTercero NVARCHAR(250) = NULL,
	@Estacion UNIQUEIDENTIFIER = NULL
AS
BEGIN


declare @idLocales as table(id int, maxID int)


	DECLARE @NombreBuscar NVARCHAR(250), @IdentificacionTerceroBuscar NVARCHAR(250)

	SET @NombreBuscar = ISNULL('%'+@NombreTercero+'%', NULL)
	SET @IdentificacionTerceroBuscar = ISNULL('%'+@IdentificacionTercero+'%', NULL)
	
	--SELECT @FechaInicial = DATEADD(HOUR,6,@FechaInicial), @FechaFinal = DATEADD(DAY,1,@FechaFinal);
	--SELECT @FechaFinal = DATEADD(HOUR,6,@FechaFinal);

	SELECT  convert(varchar(40),Factura.[Guid]) as Guid, Factura.Consecutivo, convert(varchar(40),Tercero.[Guid]) AS IdTercero, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, Combustible.Nombre as Combustible, Factura.Cantidad,
			Factura.Precio, Factura.Total,Factura.Descuento, Factura.IdInterno, Factura.Placa, Factura.Kilometraje,
			Mangueras.Surtidor, Mangueras.Cara, Mangueras.Manguera, Factura.Fecha, Factura.IdLocal, Factura.IdVentaLocal,
			Estado.Texto AS Estado, Factura.FechaProximoMantenimiento, Factura.SubTotal, Vendedor.Nombre AS  Vendedor,Resolucion.Descripcion AS [DescripcionResolucion],
			FormaDePago.Nombre as FormaDePago, Factura.idFacturaElectronica
	FROM [dbo].[Facturas] AS Factura
	JOIN [dbo].Estados AS Estado
		ON Estado.Id = Factura.IdEstadoActual
	JOIN [dbo].[Terceros] AS Tercero
		ON Tercero.Id = Factura.IdTercero
	JOIN [dbo].Estaciones AS Estaciones
		ON Estaciones.Id = Factura.IdEstacion
	JOIN Resolucion 
		ON Resolucion.Id = Factura.IdResolucion
	LEFT JOIN Vendedor ON Vendedor.Id = Factura.IdVendedor
	LEFT JOIN Combustible ON Combustible.Id = Factura.IdCombustible
	LEFT JOIN FormaDePago ON FormaDePago.Id = Factura.IdFormaDePago
	LEFT JOIN Mangueras ON Mangueras.Id = Factura.IdMangueras
	WHERE (@IdentificacionTercero IS NULL OR Tercero.Identificacion = @IdentificacionTerceroBuscar)
		  AND (@NombreTercero IS NULL OR Tercero.Nombre LIKE @NombreBuscar)
		  AND ((Factura.FechaReporte is not null and ( Factura.FechaReporte >= CONVERT(date, @FechaInicial) AND Factura.FechaReporte <=CONVERT(date, @FechaFinal)) )
		  or (Factura.FechaReporte is null and ( Factura.Fecha >= CONVERT(date, @FechaInicial) AND Factura.Fecha <=CONVERT(date, @FechaFinal)) ))AND (@Estacion IS NULL OR Estaciones.[Guid] = @Estacion)
	ORDER BY Consecutivo DESC
END
GO
CREATE PROCEDURE [dbo].[GetFacturasImprimir]
	@estacion UNIQUEIDENTIFIER
AS
BEGIN
BEGIN TRAN
	DECLARE @idVentaLocalTable AS TABLE (Id INT)

	INSERT INTO @idVentaLocalTable(id)
	SELECT O.IdObjeto FROM ObjetoImprimir O 
	JOIN [dbo].[Facturas] AS Factura
		ON O.IdObjeto = Factura.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	WHERE Estaciones.[Guid] = @estacion
	AND O.Tipo = 'Factura'

	DELETE ObjetoImprimir FROM ObjetoImprimir O 
	JOIN [dbo].[Facturas] AS Factura
		ON O.IdObjeto = Factura.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	WHERE Estaciones.[Guid] = @estacion
	AND O.Tipo = 'Factura'

	SELECT Factura.[Guid], Factura.Consecutivo, Tercero.[Guid] AS IdTercero, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, Combustible.Nombre as Combustible, Factura.Cantidad,
		   Factura.Precio, Factura.Total,Factura.Descuento, Factura.IdInterno, Factura.Placa, Factura.Kilometraje,
		   Mangueras.Surtidor, Mangueras.Cara, Mangueras.Manguera, Factura.Fecha, Factura.IdLocal, Factura.IdVentaLocal,
		   Estado.Texto AS Estado, Factura.FechaProximoMantenimiento, Factura.SubTotal, Vendedor.Nombre as Vendedor,Resolucion.Descripcion AS [DescripcionResolucion], Factura.idFacturaElectronica,
		   FormaDePago.Nombre as FormaDePago
	FROM [dbo].[Facturas] AS Factura
	JOIN [dbo].[Terceros] Tercero
		ON Tercero.Id = Factura.IdTercero
	JOIN [dbo].Estados AS Estado
		ON Estado.Id = Factura.IdEstadoActual
	JOIN @idVentaLocalTable O
		ON O.Id = Factura.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	JOIN Resolucion 
		ON Resolucion.Id = Factura.IdResolucion
		
	LEFT JOIN Vendedor ON Vendedor.Id = Factura.IdVendedor
	LEFT JOIN Combustible ON Combustible.Id = Factura.IdCombustible
	
	LEFT JOIN FormaDePago ON FormaDePago.Id = Factura.IdFormaDePago
	LEFT JOIN Mangueras ON Mangueras.Id = Factura.IdMangueras
	WHERE Estaciones.[Guid] = @estacion
COMMIT TRAN
END
GO
ALTER PROCEDURE [dbo].[GetOrdenesDeDespacho]
	@FechaInicial DATETIME = '1990-01-01',
	@FechaFinal DATETIME = '2100-01-01',
	@IdentificacionTercero NVARCHAR(250) = NULL,
	@NombreTercero NVARCHAR(250) = NULL,
	@Estacion UNIQUEIDENTIFIER = NULL
AS
BEGIN

declare @idLocales as table(id int, maxID int)
declare @idEstacion int

select @idEstacion = id from Estaciones where Guid = @Estacion

insert into @idLocales(id, maxID)
SELECT top(10)  idVentaLocal, max(id)
FROM ORdenesDeDespacho
GROUP BY idVentaLocal,IdEstacion
HAVING COUNT(idVentaLocal) >1
AND IdEstacion = @idEstacion
ORDER BY idVentaLocal desc ;

insert into @idLocales(id, maxID)
SELECT top(10)  idVentaLocal, max(id)
FROM Facturas
GROUP BY idVentaLocal,IdEstacion
HAVING COUNT(idVentaLocal) >1
AND IdEstacion = @idEstacion
ORDER BY idVentaLocal desc ;

--select count(id) from @idLocales
--select max(maxID) from @idLocales
delete ORdenesDeDespacho from ORdenesDeDespacho 
inner join @idLocales il on il.id = ORdenesDeDespacho.idVentaLocal
where ORdenesDeDespacho.id != maxid

delete Facturas from Facturas 
inner join @idLocales il on il.id = Facturas.idVentaLocal
where Facturas.id != maxid

	DECLARE @NombreBuscar NVARCHAR(250), @IdentificacionTerceroBuscar NVARCHAR(250)

	SET @NombreBuscar = ISNULL('%'+@NombreTercero+'%', NULL)
	SET @IdentificacionTerceroBuscar = ISNULL('%'+@IdentificacionTercero+'%', NULL)
	--SELECT @FechaInicial = DATEADD(HOUR,6,@FechaInicial), @FechaFinal = DATEADD(DAY,1,@FechaFinal);
	--SELECT @FechaFinal = DATEADD(HOUR,6,@FechaFinal);

	SELECT convert(varchar(40),OrdenDeDespacho.[Guid]), Factura.Consecutivo, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, Combustible.Nombre as Combustible, OrdenDeDespacho.Cantidad,
			OrdenDeDespacho.Precio, OrdenDeDespacho.Total,OrdenDeDespacho.Descuento, OrdenDeDespacho.IdInterno, OrdenDeDespacho.Placa, OrdenDeDespacho.Kilometraje,
			Mangueras.Surtidor, Mangueras.Cara, Mangueras.Manguera, OrdenDeDespacho.Fecha,
			Estado.Texto AS Estado, OrdenDeDespacho.IdLocal, OrdenDeDespacho.IdVentaLocal, OrdenDeDespacho.FechaProximoMantenimiento,
			OrdenDeDespacho.SubTotal, Vendedor.Nombre AS Vendedor, FormaDePago.Nombre as FormaDePago, OrdenDeDespacho.idFacturaElectronica, FormaDePago.Nombre as FormaDePago
	FROM [dbo].[OrdenesDeDespacho] AS OrdenDeDespacho
	JOIN [dbo].[Terceros] AS Tercero
		ON Tercero.Id = OrdenDeDespacho.IdTercero
	JOIN [dbo].[Estados] AS Estado
		ON OrdenDeDespacho.IdEstadoActual = Estado.Id
	LEFT JOIN [dbo].[Facturas] AS Factura
		ON Factura.Id = OrdenDeDespacho.IdFactura
	JOIN [dbo].Estaciones AS Estaciones
		ON Estaciones.Id = OrdenDeDespacho.IdEstacion
	
	LEFT JOIN Vendedor ON Vendedor.Id = OrdenDeDespacho.IdVendedor
	
	LEFT JOIN Combustible ON Combustible.Id = OrdenDeDespacho.IdCombustible
	LEFT JOIN FormaDePago ON FormaDePago.Id = Factura.IdFormaDePago
	LEFT JOIN Mangueras ON Mangueras.Id = OrdenDeDespacho.IdMangueras
	WHERE (@IdentificacionTercero IS NULL OR Tercero.Identificacion = @IdentificacionTerceroBuscar)
		  AND (@NombreTercero IS NULL OR Tercero.Nombre LIKE @NombreBuscar)
		  AND OrdenDeDespacho.Fecha >= CONVERT(date, @FechaInicial)  AND OrdenDeDespacho.Fecha <= CONVERT(date, @FechaFinal) 
		  AND (@Estacion IS NULL OR Estaciones.[Guid] = @Estacion)
	ORDER BY OrdenDeDespacho.IdVentaLocal DESC
END
GO
ALTER PROCEDURE [dbo].[GetOrdenesDeDespachoByFactura]
	@FacturaGuid UNIQUEIDENTIFIER
AS
BEGIN
	SELECT OrdenDeDespacho.[Guid], Factura.Consecutivo, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, Combustible.Nombre as Combustible, OrdenDeDespacho.Cantidad,
			OrdenDeDespacho.Precio, OrdenDeDespacho.Total,OrdenDeDespacho.Descuento, OrdenDeDespacho.IdInterno, OrdenDeDespacho.Placa, OrdenDeDespacho.Kilometraje,
			Mangueras.Surtidor, Mangueras.Cara, Mangueras.Manguera, OrdenDeDespacho.Fecha,
			Estado.Texto AS Estado, OrdenDeDespacho.IdLocal, OrdenDeDespacho.IdVentaLocal, OrdenDeDespacho.FechaProximoMantenimiento,
			OrdenDeDespacho.SubTotal, Vendedor.Nombre as Vendedor, OrdenDeDespacho.idFacturaElectronica, FormaDePago.Nombre as FormaDePago
	FROM [dbo].[OrdenesDeDespacho] AS OrdenDeDespacho
	JOIN [dbo].[Terceros] AS Tercero
		ON Tercero.Id = OrdenDeDespacho.IdTercero
	JOIN [dbo].[Estados] AS Estado
		ON OrdenDeDespacho.IdEstadoActual = Estado.Id
	JOIN [dbo].[Facturas] AS Factura
		ON Factura.Id = OrdenDeDespacho.IdFactura
	JOIN [dbo].Estaciones AS Estaciones
		ON Estaciones.Id = OrdenDeDespacho.IdEstacion
		
	LEFT JOIN Vendedor ON Vendedor.Id = OrdenDeDespacho.IdVendedor
	
	LEFT JOIN Combustible ON Combustible.Id = OrdenDeDespacho.IdCombustible
	LEFT JOIN FormaDePago ON FormaDePago.Id = Factura.IdFormaDePago
	LEFT JOIN Mangueras ON Mangueras.Id = OrdenDeDespacho.IdMangueras
	WHERE Factura.[Guid] = @FacturaGuid
END
GO
ALTER PROCEDURE [dbo].[GetOrdenesImprimir]
	@estacion UNIQUEIDENTIFIER
AS
BEGIN

	DECLARE @idVentaLocalTable AS TABLE (Id int)

	INSERT INTO @idVentaLocalTable(Id)
	SELECT O.IdObjeto FROM ObjetoImprimir O 
	JOIN [dbo].[OrdenesDeDespacho] AS Factura
		ON O.IdObjeto = Factura.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	WHERE Estaciones.[Guid] = @estacion
	and O.Tipo = 'Orden'

	DELETE ObjetoImprimir FROM ObjetoImprimir O 
	JOIN [dbo].[OrdenesDeDespacho] AS Factura
		ON O.IdObjeto = Factura.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	WHERE Estaciones.[Guid] = @estacion
	and O.Tipo = 'Orden'

	SELECT OrdenDeDespacho.[Guid], Factura.Consecutivo, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, Combustible.Nombre as Combustible, OrdenDeDespacho.Cantidad,
				OrdenDeDespacho.Precio, OrdenDeDespacho.Total,OrdenDeDespacho.Descuento, OrdenDeDespacho.IdInterno, OrdenDeDespacho.Placa, OrdenDeDespacho.Kilometraje,
				Mangueras.Surtidor, Mangueras.Cara, Mangueras.Manguera, OrdenDeDespacho.Fecha,
				Estado.Texto AS Estado, OrdenDeDespacho.IdLocal, OrdenDeDespacho.IdVentaLocal, OrdenDeDespacho.FechaProximoMantenimiento,
				OrdenDeDespacho.SubTotal, Vendedor.Nombre as Vendedor, OrdenDeDespacho.idFacturaElectronica, FormaDePago.Nombre as FormaDePago
	FROM [dbo].[OrdenesDeDespacho] AS OrdenDeDespacho
	JOIN [dbo].[Terceros] Tercero
		ON Tercero.Id = OrdenDeDespacho.IdTercero
	LEFT JOIN [dbo].[Facturas] AS Factura
		ON Factura.Id = OrdenDeDespacho.IdFactura
	JOIN [dbo].Estados AS Estado
		ON Estado.Id = OrdenDeDespacho.IdEstadoActual
	JOIN @idVentaLocalTable O
		ON O.Id = OrdenDeDespacho.Id
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = OrdenDeDespacho.IdEstacion
		
	LEFT JOIN Vendedor ON Vendedor.Id = OrdenDeDespacho.IdVendedor
	
	LEFT JOIN Combustible ON Combustible.Id = OrdenDeDespacho.IdCombustible
	LEFT JOIN FormaDePago ON FormaDePago.Id = Factura.IdFormaDePago
	LEFT JOIN Mangueras ON Mangueras.Id = OrdenDeDespacho.IdMangueras
	WHERE Estaciones.[Guid] = @estacion
END
GO
create PROCEDURE [dbo].[AgregarFactura]
	@facturas [dbo].[FacturaType] readonly,
	@estacion UNIQUEIDENTIFIER
AS
BEGIN

declare @estadoId int, @idEstacion int
			select @estadoId = Id
			from Estados
			Where Texto = 'Creada'

declare @estadoActivoId int
			select @estadoActivoId = Id
			from Estados
			Where Texto = 'Activo'
			declare @estadoHabilitadaId int
			select @estadoHabilitadaId = Id
			from Estados
			Where Texto = 'Habilitada'

			declare @IdResolucion int

			SELECT @IdResolucion = Resolucion.Id, @idEstacion = Estaciones.Id
			FROM dbo.Resolucion
			JOIN dbo.Estados
				ON Estados.Id = Resolucion.IdEstado
			JOIN dbo.Estaciones
				ON Estaciones.Id = Resolucion.IdEstacion
			WHERE (Estados.Texto = 'Activo' or Estados.Texto = 'Habilitada')
			AND Estaciones.Guid = @estacion
			AND Resolucion.Tipo=0
	
declare @IdTercero int, @idTipoIdentificacion int, @idTipoIdentificacioncc int
	select @IdTercero = Id from terceros where Identificacion = '222222222222'
	select @idTipoIdentificacion = Id from TipoIdentificacion where Texto ='No especificada'
	select @idTipoIdentificacioncc = Id from TipoIdentificacion where Texto ='Cédula Ciudadanía'
	if @idTipoIdentificacion is null
	begin
		INSERT INTO tipoTipoIdentificacion(Guid, Texto)
		VALUES(NEWID(), 'No especificada');
		select @idTipoIdentificacion = @@identity
	end
	if @idTipoIdentificacioncc is null
	begin
		INSERT INTO tipoTipoIdentificacion(Guid, Texto)
		VALUES(NEWID(), 'Cédula Ciudadanía');
		select @idTipoIdentificacioncc = @@identity
	end
	if @IdTercero is null
	begin
		INSERT INTO Terceros ([Guid], Nombre, Direccion, Telefono, Correo, TipoIdentificacion, Identificacion, IdLocal)
		VALUES(NEWID(), 'CONSUMIDOR FINAL', 'no informado', 'no informado', 'no informado', @idTipoIdentificacion, '222222222222', 1);
		select @IdTercero = @@identity
	end

	INSERT INTO Terceros ([Guid], Nombre, Direccion, Telefono, Correo, TipoIdentificacion, Identificacion, IdLocal)
		select NEWID(), f.NombreTercero, 'no informado', 'no informado', 'no informado', @idTipoIdentificacioncc, f.Identificacion, 1
		from @facturas f
		left join Terceros t on t.Identificacion = f.Identificacion
		where t.Id is null

	insert into Vendedor(Nombre)
select distinct Vendedor from @facturas f
left join Vendedor on Vendedor.Nombre = f.Vendedor
where Vendedor.Id is null and f.Vendedor is not null

insert into Combustible(Nombre)
select distinct Combustible from @facturas f
left join Combustible on Combustible.Nombre = f.Combustible
where Combustible.Id is null and f.Combustible is not null

insert into FormaDePago(Nombre)
select distinct FormaDePago from @facturas f
left join FormaDePago on FormaDePago.Nombre = f.FormaDePago
where FormaDePago.Id is null
AND  FormaDePago is not null and f.FormaDePago is not null

insert into Mangueras(Surtidor, Cara, Manguera)
select Distinct Facturas.Surtidor, Facturas.Cara, Facturas.Manguera from @facturas Facturas
left join Mangueras on Mangueras.Surtidor = Facturas.Surtidor 
and Mangueras.Cara = Facturas.Cara
and Mangueras.Manguera = Facturas.Manguera
where Mangueras.Id is null
AND  Facturas.Surtidor is not null
AND  Facturas.Cara is not null
AND  Facturas.Manguera is not null and Facturas.Manguera is not null and Facturas.Cara is not null and Facturas.Surtidor is not null

	insert into Facturas([Guid],[Consecutivo],IdTercero,IdCombustible,Cantidad,Precio,Total,Descuento,IdInterno,Placa,
	Kilometraje,IdResolucion,IdEstadoActual,idMangueras ,[Fecha], IdFormaDePago, IdLocal, IdVentaLocal, 
	FechaProximoMantenimiento, SubTotal, idVendedor, IdEstacion)
	select newID(),f.[Consecutivo],isnull(t.Id,@IdTercero),Combustible.Id,f.Cantidad,f.Precio,f.Total,f.Descuento,f.IdInterno,f.Placa,
	f.Kilometraje, @IdResolucion, @estadoId, Mangueras.Id, f.[Fecha], FormaDePago.Id, f.IdLocal, f.IdVentaLocal,
	f.FechaProximoMantenimiento, f.SubTotal, Vendedor.Id, @idEstacion
	
	from @facturas f
	left join Terceros t on t.Identificacion = f.Identificacion
	left join Facturas on f.Consecutivo = Facturas.Consecutivo  and Facturas.IdEstacion = @idEstacion
	
	LEFT JOIN Vendedor ON Vendedor.Nombre = f.Vendedor
	LEFT JOIN Combustible ON Combustible.Nombre = f.Combustible
	LEFT JOIN FormaDePAgo ON FormaDePAgo.Nombre = f.FormaDePAgo
	LEFT JOIN Mangueras ON Mangueras.Surtidor = f.Surtidor 
and Mangueras.Cara = f.Cara
and Mangueras.Manguera = f.Manguera
	where Facturas.Id is null or facturas.IdResolucion != @IdResolucion

	update Facturas
	set Facturas.IdTercero = isnull(t.Id,@IdTercero),
	Facturas.Placa = f.Placa,
	Facturas.Kilometraje = f.Kilometraje
	from @facturas f
	left join Terceros t on t.Identificacion = f.Identificacion
	left join Facturas on f.Consecutivo = Facturas.Consecutivo and Facturas.IdEstacion = @idEstacion
	where facturas.IdResolucion = @IdResolucion
	declare @ConsecutivoActual int;
	select @ConsecutivoActual = max(consecutivo) from facturas where idresolucion = @IdResolucion;
	if @ConsecutivoActual is not null
	begin
		update resolucion set ConsecutivoActual = @ConsecutivoActual
		from resolucion
		where  resolucion.Id = @IdResolucion;
	end
END
GO
create PROCEDURE [dbo].[AgregarOrdenDespacho]
	@ordenes [dbo].[OrdenesDeDespachoType] readonly,
	@estacion UNIQUEIDENTIFIER
AS
BEGIN
	DECLARE @estadoId INT, @idEstacion int

	SELECT @estadoId = Id FROM Estados WHERE Texto = 'Creada'

	SELECT @idEstacion = Estaciones.Id
			FROM Estaciones 
			WHERE Estaciones.Guid = @estacion


	declare @IdTercero int, @idTipoIdentificacion int
	select @IdTercero = Id from terceros where Identificacion = '222222222222'
	select @idTipoIdentificacion = Id from TipoIdentificacion where Texto ='No especificada'
	if @idTipoIdentificacion is null
	begin
		INSERT INTO tipoTipoIdentificacion(Guid, Texto)
		VALUES(NEWID(), 'No especificada');
		select @idTipoIdentificacion = @@identity
	end
	if @IdTercero is null
	begin
		INSERT INTO Terceros ([Guid], Nombre, Direccion, Telefono, Correo, TipoIdentificacion, Identificacion, IdLocal)
		VALUES(NEWID(), 'CONSUMIDOR FINAL', 'no informado', 'no informado', 'no informado', @idTipoIdentificacion, '222222222222', 1);
		select @IdTercero = @@identity
	end
	
	insert into Vendedor(Nombre)
select distinct Vendedor from @ordenes f
left join Vendedor on Vendedor.Nombre = f.Vendedor
where Vendedor.Id is null and f.Vendedor is not null


	insert into Combustible(Nombre)
select distinct Combustible from @ordenes f
left join Combustible on Combustible.Nombre = f.Combustible
where Combustible.Id is null and f.Combustible is not null

insert into FormaDePago(Nombre)
select distinct FormaDePago from @ordenes f
left join FormaDePago on FormaDePago.Nombre = f.FormaDePago
where FormaDePago.Id is null
AND  FormaDePago is not null and f.FormaDePago is not null

insert into Mangueras(Surtidor, Cara, Manguera)
select Distinct Facturas.Surtidor, Facturas.Cara, Facturas.Manguera from @ordenes Facturas
left join Mangueras on Mangueras.Surtidor = Facturas.Surtidor 
and Mangueras.Cara = Facturas.Cara
and Mangueras.Manguera = Facturas.Manguera
where Mangueras.Id is null
AND  Facturas.Surtidor is not null
AND  Facturas.Cara is not null
AND  Facturas.Manguera is not null and Facturas.Surtidor is not null and Facturas.Cara is not null and Facturas.Manguera is not null

	INSERT INTO OrdenesDeDespacho([Guid],IdTercero,IdCombustible,Cantidad,Precio,Total,Descuento,IdInterno,Placa,
	Kilometraje,IdEstadoActual,IdMangueras ,[Fecha], IdFormaDePago, IdLocal, IdVentaLocal, 
	FechaProximoMantenimiento, SubTotal, idVendedor,IdEstacion)

	SELECT NewID(),isnull(t.Id,@IdTercero),Combustible.Id,o.Cantidad,o.Precio,o.Total,o.Descuento,o.IdInterno,o.Placa,
	o.Kilometraje,@estadoId,Mangueras.Id ,o.[Fecha], FormaDePago.Id, o.IdLocal, o.IdVentaLocal,
	o.FechaProximoMantenimiento, o.SubTotal, Vendedor.Id,@idEstacion
	FROM @ordenes o
	left JOIN Terceros t 
		ON t.Identificacion = o.Identificacion
	left join OrdenesDeDespacho on o.IdVentaLocal = OrdenesDeDespacho.IdVentaLocal and OrdenesDeDespacho.IdEstacion = @idEstacion
	LEFT JOIN Vendedor ON Vendedor.Nombre = o.Vendedor
	LEFT JOIN Combustible ON Combustible.Nombre = o.Combustible
	LEFT JOIN FormaDePago ON FormaDePago.Nombre = o.FormaDePAgo
	
	LEFT JOIN Mangueras ON Mangueras.Surtidor = o.Surtidor 
and Mangueras.Cara = o.Cara
and Mangueras.Manguera = o.Manguera
	where OrdenesDeDespacho.Id is null

	update OrdenesDeDespacho
	set OrdenesDeDespacho.IdTercero = isnull(t.Id,@IdTercero),
	OrdenesDeDespacho.Placa = f.Placa,
	OrdenesDeDespacho.Kilometraje = f.Kilometraje
	

	
	from @ordenes f
	left join Terceros t on t.Identificacion = f.Identificacion
	left join OrdenesDeDespacho on f.IdVentaLocal = OrdenesDeDespacho.IdVentaLocal and OrdenesDeDespacho.IdEstacion = @idEstacion
END
GO
drop procedure dbo.ObtenerOrdenDespachoPorGuid
GO
create procedure dbo.ObtenerOrdenDespachoPorGuid
(@guid uniqueidentifier)
AS
Begin
	



	SELECT OrdenDeDespacho.[Guid], Factura.Consecutivo, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, Combustible.Nombre as Combustible, OrdenDeDespacho.Cantidad,
				OrdenDeDespacho.Precio, OrdenDeDespacho.Total,OrdenDeDespacho.Descuento, OrdenDeDespacho.IdInterno, OrdenDeDespacho.Placa, OrdenDeDespacho.Kilometraje,
				Mangueras.Surtidor, Mangueras.Cara, Mangueras.Manguera, OrdenDeDespacho.Fecha,
				Estado.Texto AS Estado, OrdenDeDespacho.IdLocal, OrdenDeDespacho.IdVentaLocal, OrdenDeDespacho.FechaProximoMantenimiento,
				OrdenDeDespacho.SubTotal, Vendedor.Nombre as Vendedor, OrdenDeDespacho.idFacturaElectronica, FormaDePago.Nombre as FormaDePago
	FROM [dbo].[OrdenesDeDespacho] AS OrdenDeDespacho
	JOIN [dbo].[Terceros] Tercero
		ON Tercero.Id = OrdenDeDespacho.IdTercero
	LEFT JOIN [dbo].[Facturas] AS Factura
		ON Factura.Id = OrdenDeDespacho.IdFactura
	JOIN [dbo].Estados AS Estado
		ON Estado.Id = OrdenDeDespacho.IdEstadoActual
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = OrdenDeDespacho.IdEstacion
		
	LEFT JOIN Vendedor ON Vendedor.Id = OrdenDeDespacho.IdVendedor
	LEFT JOIN Combustible ON Combustible.Id = OrdenDeDespacho.IdCombustible
	LEFT JOIN FormaDePago ON FormaDePago.Id = OrdenDeDespacho.IdFormaDePago
	LEFT JOIN Mangueras ON Mangueras.Id = OrdenDeDespacho.IdMangueras
	WHERE OrdenDeDespacho.[Guid] = @guid 
END
GO
drop procedure dbo.ObtenerFacturaPorGuid
GO
create procedure dbo.ObtenerFacturaPorGuid
(@guid uniqueidentifier )
AS
BEgin
SELECT Factura.[Guid], Factura.Consecutivo, Tercero.[Guid] AS IdTercero, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, Combustible.Nombre as Combustible, Factura.Cantidad,
		   Factura.Precio, Factura.Total,Factura.Descuento, Factura.IdInterno, Factura.Placa, Factura.Kilometraje,
		   Mangueras.Surtidor, Mangueras.Cara, Mangueras.Manguera, Factura.Fecha, Factura.IdLocal, Factura.IdVentaLocal,
		   Estado.Texto AS Estado, Factura.FechaProximoMantenimiento, Factura.SubTotal, Vendedor.Nombre as Vendedor,Resolucion.Descripcion AS [DescripcionResolucion], Factura.idFacturaElectronica,
		   FormaDePago.Nombre as FormaDePago
	FROM [dbo].[Facturas] AS Factura
	JOIN [dbo].[Terceros] Tercero
		ON Tercero.Id = Factura.IdTercero
	JOIN [dbo].Estados AS Estado
		ON Estado.Id = Factura.IdEstadoActual
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	JOIN Resolucion 
		ON Resolucion.Id = Factura.IdResolucion
		
	LEFT JOIN Vendedor ON Vendedor.Id = Factura.IdVendedor
	LEFT JOIN Combustible ON Combustible.Id = Factura.IdCombustible
	LEFT JOIN Mangueras ON Mangueras.Id = Factura.IdMangueras
	LEFT JOIN FormaDePago ON FormaDePago.Id = Factura.IdFormaDePago
	WHERE Factura.[Guid] = @guid
END
GO
drop procedure dbo.ObtenerTerceroPorIdentificacion
GO
create procedure dbo.ObtenerTerceroPorIdentificacion(
@identificacion NVARCHAR(250))
AS BEGIN
SELECT [Terceros].[Guid], Nombre, segundo, apellidos, tipoPersona, responsabilidadTributaria, Municipio, departamento,
	Direccion, PAis, codigoPostal, celular, Telefono, Telefono2, correo2, vendedor, comentarios, Correo, 
		   [Terceros].[TipoIdentificacion], [Terceros].[Identificacion],[Terceros].[IdLocal], TipoIdentificacion.Texto AS DescripcionTipoIdentificacion,
		   terceros.idFacturacion
	FROM [dbo].[Terceros]
	JOIN [dbo].TipoIdentificacion
		ON [Terceros].TipoIdentificacion = TipoIdentificacion.Id
		where Terceros.Identificacion = @Identificacion
END
GO
drop procedure dbo.ObtenerOrdenDespachoPorIdVentaLocal
GO
create procedure dbo.ObtenerOrdenDespachoPorIdVentaLocal
(@IdVentaLocal int,@estacion uniqueidentifier)
AS
Begin
	SELECT OrdenDeDespacho.[Guid], Factura.Consecutivo, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, Combustible.Nombre as Combustible, OrdenDeDespacho.Cantidad,
				OrdenDeDespacho.Precio, OrdenDeDespacho.Total,OrdenDeDespacho.Descuento, OrdenDeDespacho.IdInterno, OrdenDeDespacho.Placa, OrdenDeDespacho.Kilometraje,
				Mangueras.Surtidor, Mangueras.Cara, Mangueras.Manguera, OrdenDeDespacho.Fecha,
				Estado.Texto AS Estado, OrdenDeDespacho.IdLocal, OrdenDeDespacho.IdVentaLocal, OrdenDeDespacho.FechaProximoMantenimiento,
				OrdenDeDespacho.SubTotal, Vendedor.Nombre as Vendedor, OrdenDeDespacho.idFacturaElectronica, FormaDePago.Nombre as FormaDePago
	FROM [dbo].[OrdenesDeDespacho] AS OrdenDeDespacho
	JOIN [dbo].[Terceros] Tercero
		ON Tercero.Id = OrdenDeDespacho.IdTercero
	LEFT JOIN [dbo].[Facturas] AS Factura
		ON Factura.Id = OrdenDeDespacho.IdFactura
	JOIN [dbo].Estados AS Estado
		ON Estado.Id = OrdenDeDespacho.IdEstadoActual
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = OrdenDeDespacho.IdEstacion
	LEFT JOIN Vendedor ON Vendedor.Id = OrdenDeDespacho.IdVendedor
	LEFT JOIN Combustible ON Combustible.Id = OrdenDeDespacho.IdCombustible
	LEFT JOIN FormaDePago ON FormaDePago.Id = OrdenDeDespacho.IdFormaDePago
	LEFT JOIN Mangueras ON Mangueras.Id = OrdenDeDespacho.IdMangueras
	WHERE OrdenDeDespacho.IdVentaLocal = @IdVentaLocal
	and @estacion = Estaciones.[Guid]
END
GO
drop procedure dbo.ObtenerFacturaPorIdVentaLocal
GO
create procedure dbo.ObtenerFacturaPorIdVentaLocal
(@IdVentaLocal int,@estacion uniqueidentifier)
AS
BEgin
SELECT Factura.[Guid], Factura.Consecutivo, Tercero.[Guid] AS IdTercero, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, Combustible.Nombre as Combustible, Factura.Cantidad,
		   Factura.Precio, Factura.Total,Factura.Descuento, Factura.IdInterno, Factura.Placa, Factura.Kilometraje,
		   Mangueras.Surtidor, Mangueras.Cara, Mangueras.Manguera, Factura.Fecha, Factura.IdLocal, Factura.IdVentaLocal,
		   Estado.Texto AS Estado, Factura.FechaProximoMantenimiento, Factura.SubTotal, Vendedor.Nombre as Vendedor,Resolucion.Descripcion AS [DescripcionResolucion], Factura.idFacturaElectronica,
		   FormaDePago.Nombre as FormaDePago
	FROM [dbo].[Facturas] AS Factura
	JOIN [dbo].[Terceros] Tercero
		ON Tercero.Id = Factura.IdTercero
	JOIN [dbo].Estados AS Estado
		ON Estado.Id = Factura.IdEstadoActual
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.[Id] = Factura.IdEstacion
	JOIN Resolucion 
		ON Resolucion.Id = Factura.IdResolucion
		
	LEFT JOIN Vendedor ON Vendedor.Id = Factura.IdVendedor
	LEFT JOIN Combustible ON Combustible.Id = Factura.IdCombustible
	LEFT JOIN FormaDePago ON FormaDePago.Id = Factura.IdFormaDePago
	LEFT JOIN Mangueras ON Mangueras.Id = Factura.IdMangueras
	WHERE Factura.IdVentaLocal = @IdVentaLocal
	and @estacion = Estaciones.[Guid]
END
GO
DROP PROCEDURE [dbo].[GetTiposIdentificaciones]
GO
CREATE PROCEDURE [dbo].[GetTiposIdentificaciones]
	
AS
	Begin
		select Texto
		from [TipoIdentificacion]
	End
GO
drop PROCEDURE [dbo].[GetTercerosActualizados]
GO
CREATE PROCEDURE [dbo].[GetTercerosActualizados]
	@estacion UNIQUEIDENTIFIER = null
AS
BEGIN

DECLARE @idTerceroTable as table (idTercero int)



SELECT [Terceros].[Guid], Nombre, segundo, apellidos, tipoPersona, responsabilidadTributaria, Municipio, departamento,
	Direccion, PAis, codigoPostal, celular, Telefono, Telefono2, correo2, vendedor, comentarios, Correo, 
		   [Terceros].[TipoIdentificacion], [Terceros].[Identificacion],[Terceros].[IdLocal], TipoIdentificacion.Texto AS DescripcionTipoIdentificacion
	FROM [dbo].[Terceros]
	JOIN [dbo].TipoIdentificacion
		ON [Terceros].TipoIdentificacion = TipoIdentificacion.Id
	JOIN @idTerceroTable O ON o.idTercero = [Terceros].Id

END
GO
IF EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Facturas' AND COLUMN_NAME = 'Vendedor')
BEGIN
ALTER TABLE [dbo].[Facturas]
DROP COLUMN Vendedor ;
END
GO
IF EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'Vendedor')
BEGIN
ALTER TABLE [dbo].[OrdenesDeDespacho]
DROP COLUMN Vendedor ;
END
GO
IF EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Facturas' AND COLUMN_NAME = 'Combustible')
BEGIN
ALTER TABLE [dbo].[Facturas]
DROP COLUMN Combustible ;
END
GO
IF EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'Combustible')
BEGIN
ALTER TABLE [dbo].[OrdenesDeDespacho]
DROP COLUMN Combustible ;
END
GO

IF EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Facturas' AND COLUMN_NAME = 'Surtidor')
BEGIN
ALTER TABLE [dbo].[Facturas]
DROP COLUMN Surtidor ;
ALTER TABLE [dbo].[Facturas]
DROP COLUMN Cara ;
ALTER TABLE [dbo].[Facturas]
DROP COLUMN Manguera ;
END
GO
IF EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'Surtidor')
BEGIN
ALTER TABLE [dbo].[OrdenesDeDespacho]
DROP COLUMN Surtidor ;
ALTER TABLE [dbo].[OrdenesDeDespacho]
DROP COLUMN Cara ;
ALTER TABLE [dbo].[OrdenesDeDespacho]
DROP COLUMN Manguera ;
END
GO

IF EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Facturas' AND COLUMN_NAME = 'FormaDePago')
BEGIN
ALTER TABLE [dbo].[Facturas]
DROP COLUMN FormaDePago ;
END
GO
IF EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'OrdenesDeDespacho' AND COLUMN_NAME = 'FormaDePago')
BEGIN
ALTER TABLE [dbo].[OrdenesDeDespacho]
DROP COLUMN FormaDePago ;
END
GO
ALTER PROCEDURE [dbo].[GetFacturaByEstado]
	@estado NVARCHAR(200),
	@estacion UNIQUEIDENTIFIER
AS
BEGIN

	DECLARE @estadoId INT

	SELECT @estadoId = Id FROM Estados WHERE Texto = @estado
	
	SELECT Factura.[Guid], Factura.Consecutivo, Factura.IdTercero, Combustible.Nombre as Combustible, Factura.Cantidad,
			Factura.Precio, Factura.Total, Factura.IdInterno, Factura.Placa, Factura.Kilometraje,
			Mangueras.Surtidor, Mangueras.Cara, Mangueras.Manguera, Factura.Fecha, Factura.FechaProximoMantenimiento, 
			Factura.SubTotal, Vendedor.Nombre as Vendedor,Resolucion.Descripcion AS [DescripcionResolucion],
			FormaDePago.Nombre as FormaDePago
	FROM [dbo].[Facturas] AS Factura
	JOIN [dbo].[Terceros] Tercero
		ON Tercero.Id = Factura.IdTercero
	JOIN [dbo].Estaciones Estaciones
		ON Estaciones.Id = Factura.IdEstacion
	JOIN Resolucion 
		ON Resolucion.Id = Factura.IdResolucion
		
	LEFT JOIN Vendedor ON Vendedor.Id = Factura.IdVendedor
	LEFT JOIN Combustible ON Combustible.Id = Factura.IdCombustible
	LEFT JOIN FormaDePago ON FormaDePago.Id = Factura.IdFormaDePago
	LEFT JOIN Mangueras ON Mangueras.Id = Factura.IdMangueras
	WHERE Factura.IdEstadoActual = @estadoId AND Estaciones.[Guid] = @estacion
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Resolucion' AND COLUMN_NAME = 'Razon')
BEGIN
ALTER TABLE [dbo].Resolucion
Add Razon nvarchar(250) Null;
END
GO
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'Resolucion' AND COLUMN_NAME = 'Nit')
BEGIN
ALTER TABLE [dbo].Resolucion
Add Nit nvarchar(250) Null;
END
GO
update resolucion set Razon = estaciones.razon, Nit = estaciones.Nit
from resolucion
inner join Estaciones on  Resolucion.IdEstacion = Estaciones.Id

GO
GO
/****** Object:  StoredProcedure [dbo].[AddNuevaResolucion]    Script Date: 12/28/2022 3:29:17 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[AddNuevaResolucion]
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
						   Fecha,[IdEstacion],[Autorizacion],[Habilitada],[Descripcion],tipo, Razon, Nit)
	SELECT NEWID(),ConsecutivoInicial,ConsecutivoFinal,FechaInicial,FechaFinal,@idEstadoActivo,ConsecutivoInicial,
		   GETDATE(),Estaciones.Id,[Autorizacion],[Habilitada],[Descripcion],tipo , Estaciones.Razon, Estaciones.Nit
	FROM @Resoluciones Resoluciones
	JOIN Estaciones
		ON Estaciones.[Guid] = Resoluciones.IdEstacion
END
GO
create or ALTER PROCEDURE [dbo].[GetEmpleadosByNombre]
(@Nombre varchar(50))
AS
BEGIN
declare @numeroActual int
	Select Nombre, Cedula from Vendedor where Nombre like '%'+@Nombre+'%' and cedula is not null
END
Go--exec [GetEmpleadosByNombre] @Nombre = 'SERGIO  ANTONIO  SANABRIA V'

CREATE INDEX tercerosidentificacion ON terceros (identificacion);
go