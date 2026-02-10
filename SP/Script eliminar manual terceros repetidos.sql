use sigesso1_lamelissa
--Coloca en nombre el nombre del tercero una palabra es mejor
select * from terceros where nombre like '%netco%'
--Coloca el terceroid que quieres que quede
declare @id_tercero int = 581


update Facturas
set IdTercero = @id_tercero
from Facturas
where IdTercero in (701)--coloca separado por , los terceroid que quieres que se borren

update OrdenesDeDespacho
set IdTercero = @id_tercero
from OrdenesDeDespacho
where IdTercero in (701)--coloca separado por , los terceroid que quieres que se borren

delete from terceros  where Id in (701)--coloca separado por , los terceroid que quieres que se borren

--Luego me pasas todos los terceros que borraste para hacer lo mismo en la nube


