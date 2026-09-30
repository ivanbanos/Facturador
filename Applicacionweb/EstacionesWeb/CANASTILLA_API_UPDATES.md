# Actualización de Servicios de Canastilla - OpenAPI Spec

## Resumen de Cambios

Se han actualizado los servicios relacionados con Canastilla para que coincidan con la nueva especificación OpenAPI. Los cambios principales incluyen nuevos endpoints, parámetros actualizados y estructuras de respuesta modificadas.

## Servicios Actualizados

### 1. CanastillasService.js

**Endpoints actualizados:**
- `GET /api/Canastilla` - Ahora acepta parámetro opcional `estacion` (UUID)
- `GET /api/Canastilla/{guid}` - Obtiene canastilla específica por GUID
- `POST /api/Canastilla` - Recibe array de canastillas, retorna entero

**Nuevos métodos:**
- `getCanastillas(estacion)` - Obtiene canastillas con filtro opcional por estación
- `getCanastilla(guid)` - Obtiene canastilla específica por GUID
- `createOrUpdateCanastillas(canastillas)` - Crea/actualiza múltiples canastillas
- `getCanastillasByEstacion(estacionGuid)` - Método de conveniencia
- `validateCanastilla(canastilla)` - Validación de datos
- `prepareCanastillaForAPI(canastilla)` - Preparación de datos para API

**Campos de datos actualizados:**
```javascript
{
  canastillaId: number,
  descripcion: string,
  unidad: string,
  precio: float,
  deleted: boolean,
  guid: UUID,
  iva: number,
  campoextra: string,
  estacion: UUID
}
```

### 2. FacturasCanastillasService.js

**Base URL actualizada:** `/api/FacturasCanastilla`

**Endpoints disponibles:**
- `POST /api/FacturasCanastilla/GetFactura` - Obtiene facturas con filtro
- `POST /api/FacturasCanastilla/GetFacturasReporte` - Obtiene reporte de facturas
- `GET /api/FacturasCanastilla/{idFactura}` - Obtiene factura específica
- `GET /api/FacturasCanastilla/detalle/{idFactura}` - Obtiene detalle de factura
- `GET /api/FacturasCanastilla/ColocarEspera/{idFactura}/Estacion/{idEstacion}` - Coloca en espera
- `GET /api/FacturasCanastilla/ObtenerParaImprimir/Estacion/{idEstacion}` - Para imprimir

**Nuevos métodos:**
- `validateFiltroBusqueda(filtroBusqueda)` - Validación de filtros
- Parámetro `estacion` añadido a `createFiltroBusqueda()`

### 3. GetCuposPorAutomotores.js

**Endpoint actualizado:** `/api/CuposInfo/Automotores/{estacion}`

**Cambios importantes:**
- Ahora retorna un entero (`int32`) en lugar de un array
- Acepta parámetro `estacionGuid` opcional
- URL actualizada según especificación OpenAPI

### 4. GetCuposPorClientes.js

**Endpoint actualizado:** `/api/CuposInfo/Clientes/{estacion}`

**Cambios importantes:**
- Ahora retorna un entero (`int32`) en lugar de un array
- Acepta parámetro `estacionGuid` opcional  
- URL actualizada según especificación OpenAPI

### 5. CuposInfoService.js (Nuevo)

**Endpoint base:** `/api/CuposInfo`

**Métodos disponibles:**
- `enviarCuposInfo(cuposRequest)` - POST para enviar cupos
- `getCuposPorAutomotores(estacion)` - GET cupos automotores
- `getCuposPorClientes(estacion)` - GET cupos clientes
- `validateCuposRequest(cuposRequest)` - Validación
- `createCupoAutomotor(cupo)` - Crear objeto cupo automotor
- `createCupoCliente(cupo)` - Crear objeto cupo cliente
- `createCuposRequest(cuposAutomotores, cuposClientes, estacionGuid)` - Request completo

**Estructura CuposRequest:**
```javascript
{
  cuposAutomotores: [
    {
      cliente: string,
      coD_CLI: string,
      nit: string,
      placa: string,
      cupoAsignado: double,
      cupoDisponible: double,
      estacionGuid: string
    }
  ],
  cuposClientes: [
    {
      cliente: string,
      coD_CLI: string,
      nit: string,
      cupoAsignado: double,
      cupoDisponible: double,
      estacionGuid: string
    }
  ],
  estacionGuid: string
}
```

# Actualización de Servicios de Canastilla - OpenAPI Spec

## Resumen de Cambios

Se han actualizado los servicios relacionados con Canastilla para que coincidan con la nueva especificación OpenAPI. Los cambios principales incluyen nuevos endpoints, parámetros actualizados y estructuras de respuesta modificadas. **También se han actualizado todas las vistas que consumen estos servicios para usar correctamente la estación desde localStorage.**

## Servicios Actualizados

### 1. CanastillasService.js

**Base URL corregida:** `/api/Canastilla`

**Endpoints actualizados:**
- `GET /api/Canastilla` - Ahora acepta parámetro opcional `estacion` (UUID)
- `GET /api/Canastilla/{guid}` - Obtiene canastilla específica por GUID
- `POST /api/Canastilla` - Recibe array de canastillas, retorna entero

**Nuevos métodos:**
- `getCanastillas(estacion)` - Obtiene canastillas con filtro opcional por estación
- `getCanastilla(guid)` - Obtiene canastilla específica por GUID
- `createOrUpdateCanastillas(canastillas)` - Crea/actualiza múltiples canastillas
- `getCanastillasByEstacion(estacionGuid)` - Método de conveniencia
- `validateCanastilla(canastilla)` - Validación de datos
- `prepareCanastillaForAPI(canastilla)` - Preparación de datos para API

**Campos de datos actualizados:**
```javascript
{
  canastillaId: number,
  descripcion: string,
  unidad: string,
  precio: float,
  deleted: boolean,
  guid: UUID,
  iva: number,
  campoextra: string,
  estacion: UUID
}
```

### 2. FacturasCanastillasService.js

**Base URL actualizada:** `/api/FacturasCanastilla`

**Endpoints disponibles:**
- `POST /api/FacturasCanastilla/GetFactura` - Obtiene facturas con filtro
- `POST /api/FacturasCanastilla/GetFacturasReporte` - Obtiene reporte de facturas
- `GET /api/FacturasCanastilla/{idFactura}` - Obtiene factura específica
- `GET /api/FacturasCanastilla/detalle/{idFactura}` - Obtiene detalle de factura
- `GET /api/FacturasCanastilla/ColocarEspera/{idFactura}/Estacion/{idEstacion}` - Coloca en espera
- `GET /api/FacturasCanastilla/ObtenerParaImprimir/Estacion/{idEstacion}` - Para imprimir

**Nuevos métodos:**
- `validateFiltroBusqueda(filtroBusqueda)` - Validación de filtros
- Parámetro `estacion` añadido a `createFiltroBusqueda()`

### 3. GetCuposPorAutomotores.js

**Endpoint actualizado:** `/api/CuposInfo/Automotores/{estacion}`

**Cambios importantes:**
- Ahora retorna un entero (`int32`) en lugar de un array
- Acepta parámetro `estacionGuid` opcional
- URL actualizada según especificación OpenAPI

### 4. GetCuposPorClientes.js

**Endpoint actualizado:** `/api/CuposInfo/Clientes/{estacion}`

**Cambios importantes:**
- Ahora retorna un entero (`int32`) en lugar de un array
- Acepta parámetro `estacionGuid` opcional  
- URL actualizada según especificación OpenAPI

### 5. CuposInfoService.js (Nuevo)

**Endpoint base:** `/api/CuposInfo`

**Métodos disponibles:**
- `enviarCuposInfo(cuposRequest)` - POST para enviar cupos
- `getCuposPorAutomotores(estacion)` - GET cupos automotores
- `getCuposPorClientes(estacion)` - GET cupos clientes
- `validateCuposRequest(cuposRequest)` - Validación
- `createCupoAutomotor(cupo)` - Crear objeto cupo automotor
- `createCupoCliente(cupo)` - Crear objeto cupo cliente
- `createCuposRequest(cuposAutomotores, cuposClientes, estacionGuid)` - Request completo

## Vistas Actualizadas

### 1. CanastillaItem.js

**Cambios realizados:**
- `loadCanastillaItems()` ahora obtiene `estacionGuid` de localStorage y lo pasa al servicio
- `openAddModal()` y `openEditModal()` incluyen el campo `estacion` en formData
- `handleSave()` usa `prepareCanastillaForAPI()` y `createOrUpdateCanastillas()`
- `handleDelete()` y `handleRestore()` usan los nuevos métodos del servicio
- Validación de estación antes de cargar datos

### 2. PorAutomotores.js

**Cambios realizados:**
- `fetchCupos()` ahora pasa `estacionGuid` al servicio
- Manejo del nuevo tipo de respuesta (entero en lugar de array)
- Estado adicional `totalCupos` para almacenar el valor retornado
- Validación de estación antes de realizar llamadas
- Manejo mejorado de errores

### 3. PorCliente.js

**Cambios realizados:**
- `fetchCupos()` ahora pasa `estacionGuid` al servicio
- Manejo del nuevo tipo de respuesta (entero en lugar de array)
- Estado adicional `totalCupos` para almacenar el valor retornado
- Validación de estación antes de realizar llamadas
- Manejo mejorado de errores

## Integración con localStorage

Todos los servicios y vistas ahora utilizan correctamente:
- `localStorage.getItem('estacionGuid')` - Para identificar la estación actual
- Validación de que existe la estación antes de realizar llamadas a la API
- Redirección al login si no se encuentra información de estación

## Compatibilidad

- **Retrocompatibilidad:** Los servicios mantienen métodos de compatibilidad donde es posible
- **Migración gradual:** Las vistas actualizadas pueden funcionar con la nueva API sin afectar otras partes del sistema
- **Fallbacks:** Manejo de casos donde los datos pueden estar en formatos antiguos

## Validación y Manejo de Errores

- Validación de parámetros obligatorios (especialmente estación)
- Manejo consistente de errores de autenticación
- Logs detallados para debugging
- Validación de estructura de datos según OpenAPI spec
- Redirección automática al login en casos de error de autenticación

## Estructura CuposRequest (Nueva)

```javascript
{
  cuposAutomotores: [
    {
      cliente: string,
      coD_CLI: string,
      nit: string,
      placa: string,
      cupoAsignado: double,
      cupoDisponible: double,
      estacionGuid: string
    }
  ],
  cuposClientes: [
    {
      cliente: string,
      coD_CLI: string,
      nit: string,
      cupoAsignado: double,
      cupoDisponible: double,
      estacionGuid: string
    }
  ],
  estacionGuid: string
}
```

## Configuración

Los servicios utilizan `window.SERVER_URL` como base URL del servidor, configurada globalmente en la aplicación.

## Estado de Migración

✅ **Servicios actualizados:** CanastillasService, FacturasCanastillasService, GetCuposPorAutomotores, GetCuposPorClientes, CuposInfoService
✅ **Vistas actualizadas:** CanastillaItem.js, PorAutomotores.js, PorCliente.js
✅ **URLs corregidas:** Todas las URLs base usan el prefijo `/api/`
✅ **Integración localStorage:** Todas las vistas usan estación desde localStorage
✅ **Validación:** Implementada en servicios y vistas
✅ **Documentación:** Actualizada con todos los cambios

---

*Fecha de actualización: July 3, 2025*
*Versión OpenAPI: 3.0.1*
*Estado: Migración completa de servicios y vistas*
