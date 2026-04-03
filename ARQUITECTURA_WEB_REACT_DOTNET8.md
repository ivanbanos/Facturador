# Arquitectura objetivo: React + .NET 8 + RabbitMQ Web STOMP

## Objetivo
Estandarizar Facturador como una aplicacion web bien estructurada con:

- Frontend React para UI operativa.
- Backend ASP.NET Core .NET 8 para API, reglas de negocio y seguridad.
- Mensajeria en tiempo real via RabbitMQ Web STOMP (como ya usa `facturador-web`).

## Estado actual (resumen)
- Ya existe frontend React en `facturador-web`.
- Ya existe backend web en `FacturadorAPI/FacturadorApiSP`, pero en `net6.0`.
- Existe consumo STOMP desde frontend (`rabbitWebSocket.js`) directo al broker.

## Arquitectura recomendada (well formed)

### 1) Capas
1. Frontend (React)
- Presentacion, enrutamiento, estado de UI y consumo API.
- Suscripcion STOMP para eventos operativos en tiempo real.

2. Backend API (.NET 8)
- Endpoints REST versionados (`/api/v1/...`).
- Validacion de entrada, autenticacion/autorizacion y orquestacion de casos de uso.
- Acceso a datos via capa de repositorios (reutilizar `FacturadorEstacionesRepositorio` donde aplique).

3. Infraestructura
- SQL Server.
- RabbitMQ con plugin `web_stomp` habilitado.

### 2) Estructura sugerida de frontend
`facturador-web/src`
- `app/`
  - `router.jsx`
  - `providers.jsx`
- `features/`
  - `reportes/`
  - `turnos/`
  - `surtidores/`
  - `facturas/`
- `components/`
  - `shared/`
  - `modals/`
- `services/`
  - `apiClient.js`
  - `stompClient.js`
- `hooks/`
  - `useVehiculosSicom.js`
- `config/`
  - `env.js`

### 3) Estructura sugerida de backend
`FacturadorAPI/FacturadorApiSP` (migrado a .NET 8)
- `Controllers/` (o Minimal APIs por feature)
- `Application/`
  - `Commands/`
  - `Queries/`
  - `DTOs/`
- `Domain/`
  - Entidades y reglas
- `Infrastructure/`
  - Repositorios SQL
  - Integraciones externas (fidelizacion, etc.)
- `Contracts/`
  - Request/Response models

## Contratos API recomendados

### Convenciones
- Base path: `/api/v1`
- Respuesta de exito:
```json
{
  "success": true,
  "data": {},
  "error": null
}
```
- Respuesta de error:
```json
{
  "success": false,
  "data": null,
  "error": "mensaje"
}
```

### Endpoints minimos
- `GET /api/v1/estacion/info`
- `GET /api/v1/surtidores`
- `GET /api/v1/reportes/lecturas?desde=...&hasta=...`
- `GET /api/v1/reportes/ventas?desde=...&hasta=...`
- `GET /api/v1/turnos?desde=...&hasta=...`

## Tiempo real con RabbitMQ Web STOMP

### Opcion A (directa desde React) - rapida
React se conecta al broker por Web STOMP:
- URL broker: `ws://<host>:15674/ws`
- Topic/queue STOMP configurable por entorno.

Ventaja:
- Menor latencia y simple de implementar.

Riesgo:
- Exponer credenciales o topologia del broker al frontend.

### Opcion B (recomendada para produccion)
- Backend .NET 8 consume RabbitMQ (AMQP).
- Backend publica al frontend por SignalR/WebSocket.

Ventaja:
- Credenciales de broker no salen al cliente.
- Mejor control de autorizacion por usuario.

## Seguridad minima requerida
1. No hardcodear credenciales de RabbitMQ en React.
2. Mover secretos a variables de entorno en backend.
3. CORS restringido por origen real (no `*` en produccion).
4. JWT + autorizacion por endpoint sensible.
5. Validacion de entrada en todos los endpoints.

## Migracion recomendada por fases

### Fase 1: Baseline tecnico
- Migrar `FacturadorApiSP` de `net6.0` a `net8.0`.
- Mantener endpoints actuales para no romper frontend.
- Estandarizar `appsettings` por entorno.

### Fase 2: Contratos y limpieza
- Versionar API (`/api/v1`).
- Normalizar respuestas `success/data/error`.
- Refactor frontend a `features + services + hooks`.

### Fase 3: Tiempo real robusto
- Consolidar cliente STOMP reutilizable (reconexion + manejo errores).
- Definir canales por feature (`VehiculosSICOM`, `SurtidoresEstado`, etc.).

### Fase 4: Hardening
- Autenticacion/autorizacion real en backend.
- Rate limiting y observabilidad (logs, correlacion, health checks).

## Definicion de listo (DoD)
1. Backend en .NET 8 compilando y corriendo con Swagger.
2. Frontend React consumiendo API versionada.
3. Eventos STOMP funcionando sin credenciales hardcoded.
4. Error handling consistente en UI y API.
5. Documento de despliegue local y productivo actualizado.

## Nota importante para este repositorio
Ya tienes gran parte de funcionalidad implementada. El camino mas seguro no es reescribir desde cero, sino:

1. Migrar `FacturadorApiSP` a .NET 8.
2. Ordenar el frontend por modulos.
3. Centralizar STOMP y configuracion por entorno.

Eso te da una web app bien formada sin detener la operacion actual.
