# Fase 0 Ejecutada - TurnoId en Facturas/Canastilla

Fecha de ejecucion: 2026-03-18

## 1. Inventario confirmado

### 1.1 Local SQL + EnviadorInformacionService
- Canastilla local:
  - SP y tabla incluyen isla/turno: `SP/Canastilla.sql`.
  - `FacturasCanastilla` ya tiene `isla`, `fechaturno`, `turno`.
  - `CrearFacturaCanastilla` ya calcula turno por isla activa.
- Enviador:
  - Modelo canastilla contiene `Isla` y `Empleado`: `EnviadorInformacionService/Models/FacturaCanastilla.cs`.
  - Request de asociacion separada existente: `EnviadorInformacionService/Models/Externos/RequestFacturaTurno.cs`.
  - Cliente remoto aun usa ruta separada para asociar turno:
    - `IConexionEstacionRemota.SetTurnoFactura`
    - `ConexionEstacionRemota.SetTurnoFactura` llama a `/api/Factura/AgregarTurnoAFactura`.

### 1.2 Web API + Negocio + Repositorio
- Modelos de negocio:
  - `Factura` y `OrdenDeDespacho` no exponen todavia contrato de turno canonico (`TurnoGuid`, `NumeroTurno`, `Isla`, `FechaTurno`, `Empleado`):
    - `FacturacionelectronicaCore.../Negocio/Modelo/Factura.cs`
    - `FacturacionelectronicaCore.../Negocio/Modelo/OrdenDeDespacho.cs`
  - `FacturaCanastilla` si expone `Isla` y `Empleado`:
    - `FacturacionelectronicaCore.../Negocio/Modelo/FacturaCanastilla.cs`
- Persistencia Mongo:
  - Wrappers incluyen `TurnoGuid`:
    - `FacturacionelectronicaCore.../Repositorio/Entities/FacturaMongo.cs`
    - `FacturacionelectronicaCore.../Repositorio/Entities/OrdenesMongo.cs`
- Brecha detectada:
  - Existe interfaz con metodos de turno en facturas (`IFacturasRepository`), pero no se encontro implementacion activa en el workspace para esa interfaz.
  - No se encontro controlador/ruta activa equivalente a `/api/Factura/AgregarTurnoAFactura`.

### 1.3 Frontend reportes (EstacionesWeb)
- Reporte actual de turnos es por rango:
  - Vista: `EstacionesWeb/src/views/Reportes/Turnos.js`
  - Servicio: `EstacionesWeb/src/services/FiltrarInfoTurnos.js`
- Request actual: `fechaInicial`, `fechaFinal`, `estacion`.
- Comportamiento requerido (pendiente fases siguientes): seleccionar dia y luego aplicar filtros dinamicos.

## 2. Contrato minimo comun acordado (canonico)
- TurnoGuid: string (GUID serializado).
- NumeroTurno: int (nullable en transicion).
- Isla: string.
- FechaTurno: DateTime (nullable en transicion).
- Empleado: string (nullable).

Regla de normalizacion:
- Campo de referencia principal: `TurnoGuid`.
- Campos de soporte para trazabilidad/reportes: `NumeroTurno`, `Isla`, `FechaTurno`, `Empleado`.

## 3. Decision de ruta unica (cerrada)
Se define una sola estrategia de asociacion de turno:
1. Resolver turno por isla al momento de creacion local de transaccion.
2. Enviar `TurnoGuid` y metadatos de turno dentro del payload principal.
3. Persistir en backend sin depender de llamada separada post-envio.

Implicacion para fases siguientes:
- La ruta `/api/Factura/AgregarTurnoAFactura` queda catalogada como legado/no canonica y se retirara de la estrategia en Fase 3.

## 4. Criterio de salida Fase 0
- Inventario levantado: Cumplido.
- Contrato minimo definido: Cumplido.
- Ruta de asociacion cerrada (sin endpoint huerfano en estrategia objetivo): Cumplido.

## 5. Insumos listos para Fase 1
- Agregar `turnoguid` en persistencia local donde aplique (Facturas y flujo equivalente en Canastilla/ordenes).
- Ajustar SPs de insercion/lectura para arrastrar el contrato de turno desde origen.
- Mantener degradacion controlada cuando no exista turno abierto por isla.
