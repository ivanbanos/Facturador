# Guia de despliegue: Web central vs Local estacion

Esta guia resume donde vive cada cambio y el orden recomendado de despliegue para la funcionalidad de combustibles/precios, reporte fiscal y reportes de ordenes.

## 1. Que es Web central y que es Local estacion

## Web central (API + Frontend)

Estos componentes viven en el servidor central y exponen datos para todas las estaciones:

- API y negocio:
  - FacturacionelectronicaCore/FacturacionelectronicaCore.Web
- Base de datos central (proyecto SQL):
  - FacturacionelectronicaCore/FacturacionelectronicaCore.Database
- Frontend web:
  - EstacionesWeb

Cambios principales en esta capa:

- Nueva persistencia de combustibles por estacion (tabla y SP en DB central).
- Endpoints de estacion para consultar/actualizar combustibles.
- Guardado automatico del precio combustible al recibir ordenes.
- Reporte fiscal con precio combustible.
- Dashboard para editar precio de combustible por estacion (input + confirmacion modal).
- Exportes de Ordenes (PDF/Excel) con fecha y hora.

## Local estacion (servicios y BD local)

Estos componentes viven en cada estacion:

- Servicio worker moderno:
  - SigesServicio
- Servicio legado:
  - EnviadorInformacionService
- Repositorio local para acceso SQL:
  - FacturadorEstacionesRepositorio
- Scripts SQL locales:
  - SP/EstacionSIGES.sql
  - SP/StoreProceduresVentas.sql

Cambios principales en esta capa:

- Sincronizacion de combustibles desde central en cada ciclo del worker/enviador.
- Actualizacion local de precios de combustible en cada ciclo.
- SP locales para mantener/consultar precios de combustible.

## 2. Orden recomendado de despliegue

IMPORTANTE: ejecutar en este orden minimiza fallas por contratos/API no disponibles.

1. Desplegar DB central (FacturacionelectronicaCore.Database)
- Aplicar primero el proyecto SQL o scripts equivalentes en la base central.
- Validar que existan:
  - tabla dbo.CombustiblesEstacion
  - SP dbo.GetCombustiblesEstacion
  - SP dbo.UpsertCombustibleEstacion
  - columna EsGas en dbo.Estaciones

2. Desplegar API central (FacturacionelectronicaCore.Web)
- Publicar API con:
  - endpoints /api/Estaciones/{guid}/Combustibles (GET/POST)
  - negocio/repositorio de combustibles
  - guardado automatico de precio al recibir ordenes
- Validar con Swagger o Postman:
  - GET /api/Estaciones/{guid}/Combustibles responde 200
  - POST /api/Estaciones/{guid}/Combustibles actualiza/inserta

3. Desplegar frontend web (EstacionesWeb)
- Publicar frontend con:
  - Dashboard de combustibles por estacion
  - precio combustible en Reporte Fiscal
  - fecha y hora en exportes de Ordenes (PDF/Excel)
- Validar en UI:
  - input de precio + boton Guardar + modal de confirmacion

4. Aplicar SQL local de estacion
- En cada estacion, ejecutar scripts locales en este orden:
  1) SP/EstacionSIGES.sql
  2) SP/StoreProceduresVentas.sql
- Validar que existan:
  - SP ActualizarPrecioCombustible
  - SP ObtenerCombustibles
  - (segun script) tabla de precios local

5. Desplegar servicios locales
- Publicar/reiniciar en cada estacion:
  - SigesServicio
  - EnviadorInformacionService (si aplica en esa estacion)
  - FacturadorEstacionesRepositorio (si se distribuye como binario separado)
- Validar logs de ciclo:
  - sincronizacion de combustibles ejecutada sin error

## 3. Smoke test post-despliegue

1. En Dashboard, actualizar precio de un combustible en una estacion.
2. Confirmar via API que el precio quedo guardado.
3. Esperar ciclo del worker/enviador y validar en la BD local que el precio se sincronizo.
4. Generar reporte fiscal y confirmar columna Precio por combustible.
5. Exportar Ordenes a PDF/Excel y confirmar Fecha y Hora.

## 4. Plan de rollback rapido

Si falla UI/API:
- Revertir deploy de EstacionesWeb y/o FacturacionelectronicaCore.Web a version anterior.

Si falla capa local:
- Detener workers locales temporalmente y volver al binario anterior.

Si falla SQL:
- Restaurar backup previo de la base afectada (central o local) antes de reintentar.

## 5. Nota operativa

Durante una ventana de mantenimiento, evita ejecutar cambios de schema central y local al mismo tiempo en horas pico. Primero estabiliza central (DB + API + web), luego avanza estacion por estacion en local.
