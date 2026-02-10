-- =============================================
-- Author:      Auto-generated based on SPSiesa and EstacionSIGES
-- Description: Procedimientos y tablas para integración Siesa SIGES
-- =============================================

-- Agregar columna para control de envío a Siesa en tablas SIGES
IF COL_LENGTH('Terceros', 'enviadoSiesa') IS NULL
BEGIN
    ALTER TABLE Terceros ADD enviadoSiesa bit NULL;
END
GO

IF COL_LENGTH('OrdenesDeDespacho', 'EnviadaSiesa') IS NULL
BEGIN
    ALTER TABLE OrdenesDeDespacho ADD EnviadaSiesa bit DEFAULT 0;
END
GO

-- Procedimiento para marcar terceros enviados a Siesa
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'CambiarEstadoTerceroEnviadoSiesaSiges')
    DROP PROCEDURE [dbo].[CambiarEstadoTerceroEnviadoSiesaSiges]
GO
CREATE PROCEDURE [dbo].[CambiarEstadoTerceroEnviadoSiesaSiges]
(
    @terceros [ventasIds] READONLY
)
AS
BEGIN TRY
    SET NOCOUNT ON;
    UPDATE Terceros SET enviadoSiesa = 1
    FROM Terceros
    INNER JOIN @terceros t ON t.ventaId = Terceros.terceroId
END TRY
BEGIN CATCH
    DECLARE 
        @errorMessage VARCHAR(2000),
        @errorProcedure VARCHAR(255),
        @errorLine INT;
    SELECT  
        @errorMessage = ERROR_MESSAGE(),
        @errorProcedure = ERROR_PROCEDURE(),
        @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH;
GO

-- Procedimiento para buscar terceros no enviados a Siesa
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'BuscarTercerosNoEnviadosSiesaSiges')
    DROP PROCEDURE [dbo].[BuscarTercerosNoEnviadosSiesaSiges]
GO
CREATE PROCEDURE [dbo].[BuscarTercerosNoEnviadosSiesaSiges]
AS
BEGIN TRY
    SET NOCOUNT ON;
    DECLARE @tercerosTemp AS TABLE(id INT)
    INSERT INTO @tercerosTemp (id)
    SELECT TOP(25) terceroId FROM Terceros WHERE enviadoSiesa = 0 OR enviadoSiesa IS NULL
    SELECT  terceroId, tipoIdentificacion, ISNULL(NULLIF(identificacion, ''), 'No informado') AS identificacion, ISNULL(NULLIF(nombre, ''), 'No informado') AS nombre, ISNULL(NULLIF(telefono, ''), 'No informado') AS telefono, ISNULL(NULLIF(correo, ''), 'No informado') AS correo, ISNULL(NULLIF(direccion, ''), 'No informado') AS direccion, estado, COD_CLI, enviadoSiesa
    FROM Terceros
    INNER JOIN @tercerosTemp tmp ON tmp.id = Terceros.terceroId
END TRY
BEGIN CATCH
    DECLARE 
        @errorMessage VARCHAR(2000),
        @errorProcedure VARCHAR(255),
        @errorLine INT;
    SELECT  
        @errorMessage = ERROR_MESSAGE(),
        @errorProcedure = ERROR_PROCEDURE(),
        @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH;
GO

-- Procedimiento para marcar facturas enviadas a Siesa
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ActuralizarEnviadasSiesaSiges')
    DROP PROCEDURE [dbo].[ActuralizarEnviadasSiesaSiges]
GO
CREATE PROCEDURE [dbo].[ActuralizarEnviadasSiesaSiges]
(
    @facturas [ventasIds] READONLY
)
AS
BEGIN TRY
    SET NOCOUNT ON;
    UPDATE OrdenesDeDespacho SET EnviadaSiesa = 1
    FROM OrdenesDeDespacho
    INNER JOIN @facturas f ON f.ventaId = OrdenesDeDespacho.ventaId
END TRY
BEGIN CATCH
    DECLARE 
        @errorMessage VARCHAR(2000),
        @errorProcedure VARCHAR(255),
        @errorLine INT;
    SELECT  
        @errorMessage = ERROR_MESSAGE(),
        @errorProcedure = ERROR_PROCEDURE(),
        @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH;
GO

-- Procedimiento para obtener facturas no enviadas a Siesa
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'getFacturaSinEnviarSiesaSiges')
    DROP PROCEDURE [dbo].[getFacturaSinEnviarSiesaSiges]
GO
CREATE PROCEDURE [dbo].[getFacturaSinEnviarSiesaSiges]
AS
BEGIN TRY
    SET NOCOUNT ON;
    DECLARE @facturasTemp AS TABLE(id INT)
    INSERT INTO @facturasTemp (id)
    SELECT TOP(10) ventaId FROM OrdenesDeDespacho WHERE (enviada =1 and (EnviadaSiesa = 0 OR EnviadaSiesa IS NULL)) ORDER BY ventaId desc
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
	
	,venta.*, empleado.Nombre as Empleado, Combustible.descripcion as combustible, Manguera.Descripcion as Manguera, Cara.descripcion as Cara, Surtidor.descripcion as Surtidor
	,Vehiculos.fechafin as fechaProximoMantenimiento
	from OrdenesDeDespacho
	inner join venta on venta.Id = OrdenesDeDespacho.ventaId 
	left join vehiculos on Venta.Ibutton = Vehiculos.idrom
	left join turno on venta.idturno = turno.id 
	left join empleado on turno.idEmpleado = empleado.id 
	inner join Combustible on Combustible.Id = IdCombustible
	inner join Manguera on Manguera.Id = IdManguera
	inner join Cara on Cara.Id = Manguera.IdCara
	inner join Surtidor on Surtidor.Id = Cara.IDSurtidor

    inner join @facturasTemp tmp on tmp.id = OrdenesDeDespacho.ventaId 
	left join dbo.Resoluciones on OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId
	left join dbo.terceros on OrdenesDeDespacho.terceroId = terceros.terceroId
    left join dbo.TipoIdentificaciones on terceros.tipoIdentificacion = TipoIdentificaciones.TipoIdentificacionId
END TRY
BEGIN CATCH
    DECLARE 
        @errorMessage VARCHAR(2000),
        @errorProcedure VARCHAR(255),
        @errorLine INT;
    SELECT  
        @errorMessage = ERROR_MESSAGE(),
        @errorProcedure = ERROR_PROCEDURE(),
        @errorLine = ERROR_LINE();
    RAISERROR (N'<message>Error occurred in %s :: %s :: Line number: %d</message>', 16, 1, @errorProcedure, @errorMessage, @errorLine);
END CATCH;
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
--alter table Auxiliares add cruce bit not null default 0
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

IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'ObtenerTurnoIslaCerrado')
	DROP PROCEDURE [dbo].[ObtenerTurnoIslaCerrado]
GO
