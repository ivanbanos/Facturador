# Per-Station Canastilla Configuration - Implementation Summary

## Overview
This document summarizes the implementation of per-station configuration for Canastilla (basket) items and per-station combustibles configuration in the FacturacionelectronicaCore system.

## Database Changes

### 1. Canastilla Table
- **Added Fields:**
  - `campoextra` (varchar(50) NULL) - Extra field for additional configuration
  - `estacion` (uniqueidentifier NULL) - Station identifier for per-station filtering

### 2. Updated Table Types
- **CanastillaType**: Updated to include `campoextra` and `estacion` fields
- **CanastillaDetalleType**: Used for stored procedure parameters

### 3. Updated Stored Procedures
- **UpdateOrCreateCanastilla**: Now handles `estacion` and `campoextra` fields
- **GetCanastilla**: Added optional `@estacion` parameter for filtering by station
- **CrearFacturaCanastilla**: Updated to filter Canastilla items by station
- **CrearFacturaDetalle**: Updated to join Canastilla with station filtering

## C# Backend Changes

### 1. Entity Models
- **Repositorio.Entities.Canastilla**: Added `estacion` property (Guid?)
- **Negocio.Modelo.Canastilla**: Added `estacion` property (Guid?)

### 2. Repository Layer
- **ICanastillaRepositorio**: Updated `GetCanastillas` method to accept optional `estacion` parameter
- **CanastillaRepositorio**: Implementation now passes `estacion` parameter to stored procedure

### 3. Business Logic Layer
- **ICanastillaNegocio**: Updated `GetCanastillas` method to accept optional `estacion` parameter
- **CanastillaNegocio**: Implementation now forwards `estacion` parameter to repository

### 4. Controller Layer
- **CanastillaController**: Updated GET endpoint to accept optional `estacion` query parameter

### 5. Per-Station Combustibles Configuration
- **EstacionCombustibles Class**: New class containing fuel-specific properties
  ```csharp
  public class EstacionCombustibles
  {
      public string Corriente { get; set; }
      public string Extra { get; set; }
      public string Acpm { get; set; }
  }
  ```
- **Alegra Class**: Added `Estaciones` property (Dictionary<string, EstacionCombustibles>)
- **DatosFactura Class**: Updated to use per-station configuration with fallback to global values

## Configuration Example

### appsettings.json Structure
```json
{
  "Alegra": {
    "BaseUrl": "https://app.alegra.com/api/v1/",
    "Authorization": "Basic base64credentials",
    "Corriente": "1", 
    "Extra": "2", 
    "Acpm": "3",
    "Estaciones": {
      "estacion1-guid": {
        "Corriente": "101",
        "Extra": "102", 
        "Acpm": "103"
      },
      "estacion2-guid": {
        "Corriente": "201",
        "Extra": "202",
        "Acpm": "203"
      }
    }
  }
}
```

## Usage Examples

### 1. Filtering Canastilla by Station
```csharp
// Get all canastillas
var allCanastillas = await canastillaNegocio.GetCanastillas();

// Get canastillas for specific station
var stationCanastillas = await canastillaNegocio.GetCanastillas(stationGuid);
```

### 2. API Endpoint Usage
```http
GET /api/canastilla                           # All canastillas
GET /api/canastilla?estacion={station-guid}  # Station-specific canastillas
```

### 3. Per-Station Combustibles
The system automatically uses station-specific combustible codes when available, falling back to global configuration:
```csharp
// In DatosFactura constructor
if (factura.Combustible.ToLower().Contains("corriente"))
{
    Combustible = options.Estaciones?.ContainsKey(estacion) == true ? 
        options.Estaciones[estacion].Corriente ?? options.Corriente : 
        options.Corriente;
}
```

## Key Features
1. **Backward Compatibility**: System works with existing data (estacion field can be NULL)
2. **Flexible Filtering**: Can retrieve all canastillas or filter by specific station
3. **Per-Station Configuration**: Each station can have unique combustible mappings
4. **Fallback Logic**: If station-specific config isn't available, uses global settings
5. **Database Performance**: Added proper indexes for efficient querying

## Testing Recommendations
1. Test canastilla retrieval with and without station filtering
2. Verify per-station combustible configuration works correctly
3. Test fallback to global combustible configuration
4. Validate that existing functionality remains unaffected
5. Test invoice creation with station-specific canastilla items

## Migration Notes
- All database changes are backward compatible
- Existing canastilla records will have NULL estacion values
- Global combustible configuration remains as fallback
- No breaking changes to existing API endpoints
