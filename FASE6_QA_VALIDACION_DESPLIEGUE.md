# Fase 6 - QA, Validacion y Despliegue

Fecha de inicio: 2026-03-18

## 1. Estado de arranque
- Backend web: compilacion validada previamente en verde.
- Frontend `EstacionesWeb`: compilacion validada en verde el 2026-03-18 con warnings historicos de lint, sin errores de build.
- Alcance de esta fase: validar extremo a extremo `TurnoGuid`, reenvio, reporte diario y preparar despliegue controlado.

## 2. Prerrequisitos
- Scripts SQL de `SP/StoreProceduresFacturas.sql`, `SP/StoreProceduresVentas.sql` y `SP/Canastilla.sql` aplicados en ambiente de prueba.
- Servicio web desplegado con endpoint `POST /api/Turnos/GetTurnoReporteDia`.
- `EnviadorInformacionService` actualizado en la estacion de prueba.
- Acceso a:
  - SQL local de estacion.
  - Logs del enviador.
  - Mongo/colecciones del backend web.
  - UI web `EstacionesWeb`.

## 3. Checklist funcional

### Caso 1 - Factura combustible con turno
Objetivo: confirmar que una factura nueva con isla quede enlazada a turno de punta a punta.

Pasos:
1. Abrir o identificar un turno vigente por isla en la estacion.
2. Generar una factura combustible nueva sobre esa isla.
3. Validar en SQL local que el registro en `FacturasPOS` u `OrdenesDeDespacho` tenga `turnoguid` no nulo.
4. Validar en el payload enviado o en logs del enviador que la factura/orden salga con `TurnoGuid`.
5. Validar en Mongo/backend que el documento persistido conserve `TurnoGuid`.

Resultado esperado:
- `turnoguid` no nulo y consistente entre SQL local, payload y Mongo.

Evidencia:
- VentaId:
- Isla:
- NumeroTurno:
- TurnoGuid:
- Resultado: Pendiente / OK / Falla

### Caso 2 - Factura canastilla con turno
Objetivo: confirmar que canastilla preserve `Isla` y `TurnoGuid` extremo a extremo.

Pasos:
1. Crear una factura canastilla en una isla con turno resoluble.
2. Validar en `FacturasCanastilla` que `isla` y `turnoguid` queden almacenados.
3. Validar que el enviador incluya `Isla` y `TurnoGuid` en el contrato saliente.
4. Validar en backend/Mongo que el documento persistido conserve ambos campos.

Resultado esperado:
- `Isla` y `TurnoGuid` presentes en origen y destino.

Evidencia:
- FacturaCanastillaId:
- Isla:
- TurnoGuid:
- Resultado: Pendiente / OK / Falla

### Caso 3 - Retroactivo y reenvio de pendientes
Objetivo: asegurar que el reproceso no pierda el enlace de turno.

Pasos:
1. Tomar una factura/orden pendiente de `turnoEnviado`.
2. Ejecutar el ciclo del enviador o el reproceso programado.
3. Verificar que si la factura ya trae `TurnoGuid`, se marque `turnoEnviado` sin depender del endpoint legado.
4. Verificar que si no trae `TurnoGuid`, solo entonces use la ruta de contingencia.
5. Confirmar que no se marquen como enviadas facturas cuyo update de turno haya fallado.

Resultado esperado:
- No hay falsos positivos de sincronizacion y el enlace de turno se conserva.

Evidencia:
- VentaId:
- Tenia TurnoGuid inicial: Si / No
- Resultado: Pendiente / OK / Falla

### Caso 4 - Reporte diario de turnos
Objetivo: validar la UX y la consistencia del nuevo flujo diario con filtros.

Pasos:
1. Abrir el modulo de reporte de turnos.
2. Seleccionar un dia historico con informacion conocida.
3. Confirmar que la consulta responde sin pedir rango de fechas.
4. Aplicar filtros individualmente:
   - Empleado
   - Isla
   - Turno
   - Surtidor
   - Manguera
   - Combustible
5. Aplicar filtros combinados y verificar que los datos visibles cambien sin recargar el modulo completo.
6. Exportar PDF y Excel con una vista filtrada.
7. Comparar totales exportados contra los totales visibles en pantalla.

Resultado esperado:
- El reporte carga por dia, filtra en UI y exporta exactamente la vista filtrada actual.

Evidencia:
- Fecha consultada:
- Filtros usados:
- Total pantalla:
- Total exportado:
- Resultado: Pendiente / OK / Falla

## 4. Checklist de regresion

### Regresion 1 - Apertura/cierre de turnos
Objetivo: confirmar que la sincronizacion mensual no rompio el flujo historico de estados.

Pasos:
1. Abrir un turno.
2. Confirmar sincronizacion al backend.
3. Cerrar el turno.
4. Confirmar que el backend actualice estado y datos de cierre de surtidores.

Resultado esperado:
- El mismo turno pasa de abierto a cerrado sin duplicarse ni dejar datos viejos.

### Regresion 2 - Facturacion sin turno abierto
Objetivo: validar degradacion controlada.

Pasos:
1. Generar una transaccion en una isla sin turno abierto resoluble.
2. Verificar que la insercion local no falle.
3. Verificar que el sistema deje `TurnoGuid` nulo y registre advertencia cuando aplique.

Resultado esperado:
- El proceso no se rompe; la ausencia de turno queda registrada sin bloqueo operativo.

## 5. Plan de despliegue
1. Ejecutar scripts SQL en ventana controlada.
2. Desplegar backend web.
3. Desplegar frontend `EstacionesWeb`.
4. Actualizar `EnviadorInformacionService` en estaciones objetivo.
5. Ejecutar humo post-despliegue:
   - una factura combustible
   - una factura canastilla
   - una consulta de reporte diario
6. Registrar incidencias y rollback solo si hay afectacion operativa real.

## 6. Registro de ejecucion
- Caso 1 Factura combustible:
- Caso 2 Factura canastilla:
- Caso 3 Reenvio pendientes:
- Caso 4 Reporte diario:
- Regresion apertura/cierre:
- Regresion sin turno abierto:
- Estado final Fase 6: En progreso