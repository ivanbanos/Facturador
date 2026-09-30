CREATE PROCEDURE [dbo].[GetFacturas]
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

	SELECT top(300) Factura.[Guid], Factura.Consecutivo, Tercero.[Guid] AS IdTercero, Tercero.Identificacion, Tercero.Nombre AS NombreTercero, Factura.Combustible, Factura.Cantidad,
			Factura.Precio, Factura.Total,Factura.Descuento, Factura.IdInterno, Factura.Placa, Factura.Kilometraje,
			Factura.Surtidor, Factura.Cara, Factura.Manguera, Factura.Fecha, Factura.IdLocal, Factura.IdVentaLocal,
			Estado.Texto AS Estado, Factura.FechaProximoMantenimiento, Factura.SubTotal, Factura.Vendedor,Resolucion.Descripcion AS [DescripcionResolucion],
			Factura.FormaDePago
	FROM [dbo].[Facturas] AS Factura
	JOIN [dbo].Estados AS Estado
		ON Estado.Id = Factura.IdEstadoActual
	JOIN [dbo].[Terceros] AS Tercero
		ON Tercero.Id = Factura.IdTercero
	JOIN [dbo].Estaciones AS Estaciones
		ON Estaciones.Id = Factura.IdEstacion
	JOIN Resolucion 
		ON Resolucion.Id = Factura.IdResolucion
	WHERE (@IdentificacionTercero IS NULL OR Tercero.Identificacion = @IdentificacionTerceroBuscar)
		  AND (@NombreTercero IS NULL OR Tercero.Nombre LIKE @NombreBuscar)
		  AND Factura.FechaReporte >= CONVERT(date, @FechaInicial) AND Factura.FechaReporte <=CONVERT(date, @FechaFinal) 
		  AND (@Estacion IS NULL OR Estaciones.[Guid] = @Estacion)
	ORDER BY Consecutivo DESC
END
