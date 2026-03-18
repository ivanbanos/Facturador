# Plan de Ejecucion: TurnoId en Facturas/Canastilla y Rediseno de Reporte de Turnos

## 1. Objetivo
Implementar un flujo consistente para que facturas, facturas canastilla y entidades relacionadas queden enlazadas a un turno (TurnoId/TurnoGuid) desde su creacion usando la isla activa, y modernizar el reporte de turnos para trabajar por dia seleccionado con filtros dinamicos (empleado, turno, isla, combustible, etc.).

## 2. Hallazgos Actuales (base para ejecutar)
- Canastilla local ya guarda `isla`, `fechaturno` y `turno` (SP `CrearFacturaCanastilla`).
- Enviador de canastilla ya mapea y envia `Isla` al servidor.
- En Mongo existen campos de turno (`TurnoGuid`) en algunos wrappers, pero no se asignan de forma consistente en todos los flujos.
- El reporte actual de turnos consume un rango (`fechaInicial`, `fechaFinal`) y devuelve un modelo agregado sin campos suficientes para filtros finos por empleado/isla/numero.
- Existe referencia cliente a `/api/Factura/AgregarTurnoAFactura` que debe validarse/normalizarse en backend.

## 3. Alcance
### Incluye
- Propagacion de `TurnoGuid` y metadatos de turno en factura, canastilla y ordenes relacionadas.
- Regla de negocio para resolver turno abierto por isla al momento de creacion.
- Ajuste de contratos y persistencia local/remota.
- Nuevo flujo de reporte de turnos por dia + filtros.

### No incluye
- Cambios de diseno profundo del dominio de empleados.
- Reescritura total del modulo de reportes (solo Turnos).

## 4. Estrategia por Fases

## Fase 0 - Preparacion y contratos
1. Levantar inventario final de DTOs y entidades que representan factura/orden/canastilla en:
   - Local SQL + EnviadorInformacionService
   - FacturacionelectronicaCore.Web (controladores, negocio, repositorio)
   - EstacionesWeb (servicios + vista Turnos)
2. Definir contrato minimo comun de turno:
   - `TurnoGuid` (string/guid)
   - `NumeroTurno` (int/string segun origen)
   - `Isla` (string/int)
   - `FechaTurno` (DateTime)
   - `Empleado` (si esta disponible en origen)
3. Acordar una sola ruta para asociar turno a factura:
   - O implementar realmente `AgregarTurnoAFactura`
   - O remover llamada y resolver turno antes de enviar factura

**Criterio de salida Fase 0**: contrato de datos definido y ruta de asociacion cerrada (sin endpoints huerfanos).

### Estado Fase 0 (ejecutada - 2026-03-18)
- Inventario levantado para Local/Enviador, Web API/Repositorio y Frontend reportes.
- Contrato minimo acordado para propagacion de turno:
   - TurnoGuid: string (GUID serializado).
   - NumeroTurno: int (nullable en DTOs de transicion).
   - Isla: string.
   - FechaTurno: DateTime (nullable en DTOs de transicion).
   - Empleado: string (nullable).
- Ruta unica acordada para asociacion de turno:
   - Estrategia canonica: resolver turno por isla al crear la transaccion y enviar TurnoGuid dentro del payload principal (factura/orden/canastilla).
   - Estado del endpoint legado /api/Factura/AgregarTurnoAFactura: considerado huerfano en el estado actual del Web API y marcado para retiro funcional en la Fase 3.
   - Implicacion: no se seguira dependiendo de una llamada separada post-envio para asociar turno.

## Fase 1 - Base de datos local (estacion)
1. Agregar columna `turnoguid` (nullable) en tablas que apliquen:
   - `Facturas`
   - `FacturasCanastilla` (si no existe equivalente)
   - Otras tablas de envio que requieran trazabilidad de turno
2. Ajustar SPs de insercion/lectura:
   - `CrearFactura...`
   - `Get...Facturas...`
   - cualquier SP que alimente enviadores
3. Mantener compatibilidad retroactiva:
   - Si no hay turno abierto por isla, no romper insercion; guardar null + evento de log.

**Criterio de salida Fase 1**: facturas nuevas guardan `turnoguid` cuando existe turno abierto por isla.

### Estado Fase 1 (parcial ejecutada - 2026-03-18)
- `turnoguid` agregado en tablas de facturacion local:
   - `FacturasPOS`
   - `OrdenesDeDespacho`
   - `FacturasCanastilla`
- SPs de insercion ajustados para persistir `turnoguid`:
   - `CrearFactura`
   - `CrearFacturaCanastilla`
   - `AgregarFacturaPorIdVenta` ahora calcula y envia `turnoguid` a `CrearFactura`.
- SPs de lectura ajustados para exponer `turnoguid` en resultsets de facturas/ordenes (consumo de enviador y consultas relacionadas).
- Pendiente para cerrar Fase 1 al 100%:
   - Ejecutar scripts en entorno de BD de prueba y validar insercion/lectura real.

## Fase 2 - Asignacion de turno en creacion (regla de negocio)
1. Regla principal:
   - Buscar turno abierto por isla en `TURN_EST` al crear factura/canastilla.
2. Fallback controlado:
   - Si no hay abierto, buscar ultimo turno del dia por isla.
   - Si tampoco existe, persistir null y registrar advertencia estructurada.
3. Implementar funcion reusable para resolver turno (evitar logica duplicada por modulo).

**Criterio de salida Fase 2**: 100% de nuevas transacciones con isla valida intentan resolver turno por la misma regla.

### Estado Fase 2 (parcial ejecutada - 2026-03-18)
- Regla aplicada en creacion de factura combustible (`AgregarFacturaPorIdVenta`):
   - Usa datos de venta (`FECHA_REAL`, `NUM_TUR`, `COD_ISL`) como fuente principal.
   - Si falta info de turno, busca en `TURN_EST` para la isla (priorizando turno abierto y luego ultimo disponible).
- Regla aplicada en creacion de canastilla (`CrearFacturaCanastilla`):
   - Primero intenta turno abierto por isla.
   - Si no existe, aplica fallback al ultimo turno disponible de la isla.
- Regla aplicada en retroactivo (`PrepararRetroactivoTurnosPendientes`):
   - Cuando falta `NUM_TUR` en venta antigua, intenta resolverlo con `TURN_EST` por isla/fecha y prioridad de estado.
   - Si no se puede resolver, mantiene `turnoguid` nulo sin romper el proceso.

## Fase 3 - EnviadorInformacionService y contratos HTTP
1. Extender modelos de envio:
   - Factura
   - FacturaCanastilla
   - Ordenes de despacho relacionadas
2. Garantizar mapeo desde DataReader/SP hacia nuevos campos de turno.
3. En envio al servidor, incluir `TurnoGuid` y metadatos de soporte.
4. Revisar y corregir flujo `SetTurnoFactura` para que no dependa de endpoint inexistente.

**Criterio de salida Fase 3**: payloads enviados al servidor incluyen turno de forma consistente.

### Estado Fase 3 (parcial ejecutada - 2026-03-18)
- Contratos de salida ajustados para incluir `TurnoGuid` en ordenes enviadas al backend.
- Backend web ya persiste `TurnoGuid` en modelo/entidad/repositorio de ordenes durante ingesta y actualizacion.
- `GetFacturaSinEnviarTurno` y mapeo local ya arrastran `turnoguid`.
- Flujo legado `SetTurnoFactura` queda como contingencia: si la factura ya trae `TurnoGuid`, se marca `turnoEnviado` sin depender del endpoint legado.

## Fase 4 - Backend Web (ingesta y persistencia Mongo)
1. Actualizar modelos/entidades para recibir y guardar `TurnoGuid` + metadatos.
2. Ajustar repositorios de Factura, Canastilla y Ordenes:
   - Upsert sin perder campos de turno.
3. Implementar validacion de consistencia:
   - Si llega `TurnoGuid` vacio pero llega isla/fecha, permitir enriquecimiento opcional o registrar inconsistencia.
4. Corregir/crear endpoint de asociacion de turno si sigue siendo necesario.

**Criterio de salida Fase 4**: documentos nuevos y actualizados en Mongo conservan enlace de turno.

### Estado Fase 4 (parcial ejecutada - 2026-03-18)
- Canastilla:
   - Se propaga `TurnoGuid` desde SQL local al modelo de envio.
   - El backend web ahora recibe y persiste `TurnoGuid` en `FacturaCanastilla`.
   - Se agrega advertencia de consistencia cuando llega `Isla` sin `TurnoGuid` en ingesta.
- Factura:
   - Se normaliza `TurnoGuid` en modelo de negocio y entidades de repositorio (`Factura`/`FacturaMongo`) para persistencia consistente.
- Validacion tecnica:
   - Build Enviador y Build Web en verde (solo warnings existentes).

## Fase 5 - Reporte de Turnos (nuevo comportamiento)
1. Backend:
   - Crear endpoint orientado a un dia base (ej. `GetTurnoReporteDia`) con filtros opcionales.
   - Incluir en respuesta campos filtrables: `Empleado`, `Isla`, `NumeroTurno`, `Surtidor`, `Manguera`, `Combustible`, `Total`, `Galones`.
2. Frontend (`EstacionesWeb`):
   - Reemplazar filtro por rango por selector de dia.
   - Cargar dataset del dia y aplicar filtros combinables en UI (empleado/turno/isla/combustible).
   - Mantener exportacion PDF/Excel sobre la vista filtrada actual.
3. UX esperada (como Automatizacion REPORTE.html):
   - Paso 1: seleccionar dia historico.
   - Paso 2: refinar por filtros sin recargar manualmente todo el modulo.

**Criterio de salida Fase 5**: usuario puede consultar un dia historico y refinar interactivamente por empleado/turno/isla.

### Estado Fase 5 (parcial ejecutada - 2026-03-18)
- Backend:
   - Nuevo endpoint `POST /api/Turnos/GetTurnoReporteDia` con filtros opcionales (`Empleado`, `Isla`, `NumeroTurno`, `Surtidor`, `Manguera`, `Combustible`).
   - `TurnoReporte` enriquecido con campos filtrables (`FechaTurno`, `Empleado`, `Isla`, `NumeroTurno`).
- Frontend (`EstacionesWeb`):
   - `Turnos.js` migrado a flujo por dia (selector unico de fecha).
   - Filtros dinamicos en UI sobre dataset cargado sin recargar el modulo.
   - Exportacion PDF/Excel ajustada al contexto de dia y datos filtrados.
- Validacion tecnica:
   - Build backend web OK.
   - Build frontend `EstacionesWeb` OK.

## Fase 6 - QA, validacion y despliegue
1. Pruebas funcionales minimas:
   - Crear factura combustible con isla y validar `TurnoGuid` en SQL + payload + Mongo.
   - Crear factura canastilla y validar `Isla` + `TurnoGuid` extremo a extremo.
   - Reenvio de pendientes: no perder enlace de turno en actualizaciones.
   - Reporte: seleccionar dia pasado, aplicar filtros y validar totales exportados.
2. Pruebas de regresion:
   - Cierre/apertura de turnos existente.
   - Facturacion sin turno abierto (debe degradar con warning, no romper).
3. Plan de despliegue:
   - Scripts SQL primero.
   - Deploy backend.
   - Deploy frontend.
   - Verificacion post-deploy con checklist.

**Criterio de salida Fase 6**: flujo operativo estable y reporte validado por negocio.

### Estado Fase 6 (iniciada - 2026-03-18)
- Se creo la checklist operativa `FASE6_QA_VALIDACION_DESPLIEGUE.md` para ejecutar validacion funcional, regresion y despliegue controlado.
- Se creo guia detallada de ejecucion en produccion: `GUIA_DESPLIEGUE_TURNOGUID_REPORTE_TURNOS.md` con orden obligatorio, smoke test y rollback.
- Guia de despliegue aterrizada con parametros de entorno y comandos operativos (SQL, IIS/PowerShell, API, servicio Windows) lista para ejecucion en ventana.
- Validacion tecnica de arranque confirmada:
   - Build backend web OK.
   - Build frontend `EstacionesWeb` OK con warnings historicos de lint, sin errores de compilacion.
- Pendiente para cierre:
   - Ejecutar pruebas con datos reales de estacion en SQL local, enviador, backend y reporte web.

## 5. Riesgos y mitigaciones
- Riesgo: diferencias de tipos (`Guid`, `string`, `int`) entre capas.
  - Mitigacion: normalizar en contrato de DTO y hacer conversion centralizada.
- Riesgo: endpoint de asociacion de turno incompleto/obsoleto.
  - Mitigacion: decidir una sola estrategia y eliminar camino duplicado.
- Riesgo: datos historicos sin `TurnoGuid`.
  - Mitigacion: aceptar null historico y aplicar solo a nuevos registros; opcional script de backfill.
- Riesgo: impacto en reportes actuales.
  - Mitigacion: liberar nuevo endpoint sin romper el anterior durante transicion.

## 6. Entregables
1. Scripts SQL versionados para nuevas columnas y SPs.
2. Cambios en EnviadorInformacionService para enviar turno en facturas/canastilla.
3. Cambios en backend web para persistir/enriquecer turno.
4. Nuevo flujo de reporte diario filtrable en EstacionesWeb.
5. Documento de pruebas ejecutadas con evidencia.

## 7. Orden recomendado de implementacion
1. Fase 0 y Fase 1
2. Fase 2 y Fase 3
3. Fase 4
4. Fase 5
5. Fase 6

Este plan esta preparado para ejecutar en iteraciones cortas, validando extremo a extremo en cada fase antes de avanzar.