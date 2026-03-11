# Sistema de Facturas Consolidadas

## Descripción
Este sistema permite consolidar múltiples órdenes de despacho de un mismo cliente en una sola factura para ser enviada a facturación electrónica.

## Funcionalidades Implementadas

### 1. Listar Facturas Consolidadas
- **Endpoint**: `GET /api/facturaconsolidada/listar`
- **Parámetros**: idEstacion, fechaInicio, fechaFin, estado (opcional)
- **Descripción**: Lista todas las facturas consolidadas con filtros por estación, fechas y estado.

### 2. Obtener Órdenes Pendientes
- **Endpoint**: `GET /api/facturaconsolidada/ordenes-pendientes`
- **Parámetros**: idEstacion, fechaInicio, fechaFin, identificacionCliente (opcional)
- **Descripción**: Obtiene las órdenes de despacho que están pendientes de consolidación.

### 3. Crear Factura Consolidada
- **Endpoint**: `POST /api/facturaconsolidada/crear`
- **Body**: CrearFacturaConsolidadaRequest
- **Descripción**: Crea una nueva factura consolidada agrupando las órdenes especificadas.

### 4. Enviar a Facturación Electrónica
- **Endpoint**: `POST /api/facturaconsolidada/{guid}/enviar-facturacion`
- **Descripción**: Envía la factura consolidada al sistema de facturación electrónica.

### 5. Ver Información Detallada
- **Endpoint**: `GET /api/facturaconsolidada/{guid}/detalle`
- **Descripción**: Obtiene el detalle completo de una factura consolidada incluyendo todas las órdenes.

### Endpoints Adicionales
- **Órdenes Agrupadas**: `GET /api/facturaconsolidada/ordenes-agrupadas` - Agrupa órdenes por cliente para facilitar selección
- **Validar Consolidación**: `POST /api/facturaconsolidada/validar-consolidacion` - Valida si un grupo de órdenes puede ser consolidado

## Arquitectura

### Entidades (MongoDB)
- `FacturaConsolidadaEntity`: Entidad principal que almacena la factura consolidada
- `TotalesFacturaEntity`: Totales calculados de la factura
- `DetallePorCombustibleEntity`: Detalle agrupado por tipo de combustible

### Modelos de Negocio
- `FacturaConsolidada`: Modelo para la capa de negocio
- `TotalesFactura`: Totales de la factura
- `DetallePorCombustible`: Detalle por combustible
- `CrearFacturaConsolidadaRequest`: Request para crear consolidación
- `FiltroOrdenesConsolidacion`: Filtros para búsqueda de órdenes

### Repositorio
- `IFacturaConsolidadaRepository`: Interface del repositorio
- `FacturaConsolidadaRepository`: Implementación para MongoDB usando IMongoHelper

### Lógica de Negocio
- `IFacturaConsolidadaNegocio`: Interface de la lógica de negocio
- `FacturaConsolidadaNegocio`: Implementación con validaciones y cálculos

### AutoMapper
- `FacturaConsolidadaProfile`: Perfil de mapeo entre entidades y modelos

## Validaciones Implementadas

### Validaciones en CrearFacturaConsolidada:
1. Las órdenes deben pertenecer al mismo cliente
2. Las órdenes no deben estar ya consolidadas
3. Las órdenes deben pertenecer a la misma estación
4. Debe haber al menos una orden seleccionada

### Cálculos Automáticos:
- Subtotal por tipo de combustible
- Cantidad total por tipo de combustible
- Descuentos totales
- IVA calculado
- Total general de la factura

## Integración con Facturación Electrónica
- Se crea una "orden virtual" que representa toda la consolidación
- Se envía al sistema existente de facturación electrónica
- Se actualiza el estado según el resultado del envío

## Estados de Factura Consolidada
- `Borrador`: Recién creada, no enviada
- `Enviada`: En proceso de facturación electrónica
- `Exitosa`: Facturación electrónica exitosa
- `Error`: Error en la facturación electrónica

## Configuración Necesaria

### Dependencias en Startup.cs/Program.cs:
```csharp
// Repositorio
services.AddScoped<IFacturaConsolidadaRepository, FacturaConsolidadaRepository>();

// Lógica de negocio
services.AddScoped<IFacturaConsolidadaNegocio, FacturaConsolidadaNegocio>();

// AutoMapper
services.AddAutoMapper(typeof(FacturaConsolidadaProfile));
```

### Dependencias Existentes Requeridas:
- `IMongoHelper`: Para acceso a MongoDB
- `IOrdenDeDespachoRepository`: Para obtener órdenes
- `ITerceroRepositorio`: Para información de clientes
- `IFacturacionElectronicaFacade`: Para envío a facturación electrónica
- `IMapper` (AutoMapper): Para mapeos entre entidades y modelos

## Archivos Creados
1. **Entidades**: `FacturaConsolidadaEntity.cs`
2. **Modelos**: `FacturaConsolidada.cs` (agregado al archivo existente)
3. **Repositorio**: `IFacturaConsolidadaRepository.cs`, `FacturaConsolidadaRepository.cs`
4. **Negocio**: `IFacturaConsolidadaNegocio.cs`, `FacturaConsolidadaNegocio.cs`
5. **AutoMapper**: `FacturaConsolidadaProfile.cs`
6. **Controller**: `FacturaConsolidadaController.cs`

## Uso del Sistema

### Flujo Típico:
1. **Consultar órdenes agrupadas** por cliente para ver candidatos a consolidación
2. **Validar** que las órdenes seleccionadas pueden ser consolidadas
3. **Crear** la factura consolidada con las órdenes validadas
4. **Enviar** la factura a facturación electrónica
5. **Consultar detalle** para verificar el resultado

### Ejemplo de Uso:
```javascript
// 1. Obtener órdenes agrupadas
GET /api/facturaconsolidada/ordenes-agrupadas?idEstacion=guid&fechaInicio=2024-01-01&fechaFin=2024-01-31

// 2. Crear consolidación
POST /api/facturaconsolidada/crear
{
  "guidsOrdenes": ["guid1", "guid2", "guid3"],
  "identificacionCliente": "12345678",
  "idEstacion": "estacion-guid",
  "usuarioCreacion": "usuario123",
  "observaciones": "Consolidación mensual"
}

// 3. Enviar a facturación
POST /api/facturaconsolidada/{guid-factura}/enviar-facturacion
```

## Consideraciones Técnicas
- Usa MongoDB para almacenamiento persistente
- Integra con el sistema existente de facturación electrónica
- Mantiene la arquitectura de repositorio establecida
- Compatible con la estructura existente de órdenes de despacho
- Maneja errores de facturación electrónica preservando trazabilidad