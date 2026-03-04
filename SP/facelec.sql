use cootranshuila
go
IF NOT EXISTS (
  SELECT
    *
  FROM
    INFORMATION_SCHEMA.COLUMNS
  WHERE
    TABLE_NAME = 'facturaelectronica')
BEGIN
CREATE TABLE [dbo].[facturaelectronica]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [prefijo] NVARCHAR(50) NULL, 
    [resolucion] NVARCHAR(50) NULL, 
    [numeroActual] int NULL
)
ALTER TABLE [dbo].[facturaelectronica] Add Estacion uniqueidentifier Null;
ALTER TABLE [dbo].[facturaelectronica] Add correo nvarchar(50) Null;
ALTER TABLE [dbo].[facturaelectronica] Add token nvarchar(50) Null;
ALTER TABLE [dbo].[facturaelectronica] Add idNumeracion nvarchar(50) Null;
END
GO
GO
create or ALTER   PROCEDURE [dbo].[getNumeration]
(@Estacion uniqueidentifier)
AS
BEGIN
	Select [numeroActual],[resolucion] ,[prefijo],correo,token,idNumeracion from [facturaelectronica] where @Estacion = Estacion
END
Go
create or ALTER PROCEDURE [dbo].[setNumeration]
(@Estacion uniqueidentifier, @numeroActual int)
AS
BEGIN
	update [facturaelectronica] set [numeroActual]=@numeroActual from [facturaelectronica] where @Estacion = Estacion
END
go


insert into [facturaelectronica] ([prefijo],[resolucion],[numeroActual],Estacion,correo) 
values('EDTN','18764071237561',1946,'0442F71B-0CC0-49AD-B838-D8CBD8D0C550',''),
('EDTN','18764071237561',1946,'0442F71B-0CC0-49AD-B838-D8CBD8D0C550',''),
--('EADM','18764071237561',1946),
--('ETOM','18764071237561',1946),('EDSI','18764071237561',1946),('FPE','18764071237561',1946),('FPE','18764071237561',1946)
select guid, nombre from estaciones
select * from facturaelectronica
--
--
--update facturaelectronica set prefijo = 'FEBO', resolucion = '18764071417299', numeroActual=1,Estacion = 'E71F7C8B-4073-458D-BC88-2B77FCBB663C',correo='edscalle122@grupomat.com.co',token='5c80c6862135810ab0924c458c3895b3',idNumeracion='01905979-e171-823b-8258-f47d64a1a47e' where id=1
--update facturaelectronica set Estacion = 'F6A3C48F-ACC9-4E9A-9B3C-206A0E52C302',correo='edsbanderas1@gmail.com"',token=' ',idNumeracion=' ' where id=1
--update facturaelectronica set Estacion = '4E8E1941-D8E4-4732-A3C8-27A2DFEE9048',correo='edsbanderas1@gmail.com"',token=' ',idNumeracion=' ' where id=3
update facturaelectronica set prefijo = 'EDTN', resolucion = '18764071237561', numeroActual=1,Estacion = '8BFFE8C3-178F-4A30-B239-3D7A1A3979B9',correo='edsbanderas1@gmail.com"',token='F6A3C48F-ACC9-4E9A-9B3C-206A0E52C302',idNumeracion=' ' where id=5
update facturaelectronica set prefijo = 'EDSI', resolucion = '18764071237561', numeroActual=1,Estacion = '0442F71B-0CC0-49AD-B838-D8CBD8D0C550',correo='edsbanderas1@gmail.com"',token='BF5D6034-82FC-484D-A362-65EFB0E4C1C6',idNumeracion=' ' where id=6
--update facturaelectronica set Estacion = '9DC61E74-AEB4-4E3A-8121-3F20F391C4A3',correo='edsbanderas1@gmail.com"',token=' ',idNumeracion=' ' where id=2
go
update facturaelectronica set token ='' where token=' '