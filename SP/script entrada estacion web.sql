
insert into [dbo].[Estados](GUID, texto)
  values('4FEEDF54-A277-4383-9D1C-EF9D6924BAC4',	'Creada'),
('43FE5F2F-8586-4709-A0D1-0D083485AD06',	'Pendiente'),
('C80B53C4-D529-4621-9CAA-1CBDA0F48DB0',	'Activo'),
('C80B53C4-D529-4621-9CAA-1ABDA0F48DB0',	'Anulada'),
('C80B53C4-D529-4621-9C1A-1CBDA0F48DB0',	'Anulado'),
('C80B53C4-D529-4621-9C1A-1CBDA0F48DB0',	'Vencida'),
('C80B53C4-D529-4621-9C1A-1CBDA0F48DB0',	'Habilitada'),
('C80B53C4-D529-4621-9C1A-1CBDA0F48DB1',	'Borrada')

insert into [dbo].[Usuario](guid,nombre,usuario,contrasena)
  values('3FA85F64-5717-4562-B3FC-2C963F66AFA5',	'8090078011',	'8090078011',	'8011')

delete from facturas
delete from OrdenesDeDespacho
delete from terceros
delete from Resolucion
delete from Estaciones
delete from Usuario