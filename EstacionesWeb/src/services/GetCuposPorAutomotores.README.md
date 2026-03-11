# GetCuposPorAutomotores Service

## Overview
This service fetches vehicle credit limits (cupos) for the current station from the backend API. It provides information about assigned credit limits, available credit, and outstanding debt for each vehicle registered in the system.

## Usage

```javascript
import GetCuposPorAutomotores from '../../services/GetCuposPorAutomotores'

const fetchData = async () => {
  const cupos = await GetCuposPorAutomotores()
  
  if (cupos === 'fail') {
    // Handle authentication failure - redirect to login
    navigate('/Login', { replace: true })
  } else {
    // Use the cupos data
    setCupos(cupos)
  }
}
```

## API Endpoint

The service calls: `GET /api/Reportes/CuposPorAutomotores/{estacionGuid}`

Where `estacionGuid` is retrieved from `localStorage.getItem('estacionGuid')`

## Response Format

The service returns an array of objects with the following structure:

```javascript
[
  {
    cliente: "Client Name",              // Client company name
    nit: "123456789-1",                 // Tax ID or document number
    placa: "ABC123",                    // Vehicle license plate
    cupoAsignado: 2500000,              // Total assigned credit limit
    cupoDisponible: 1800000,            // Available credit amount
    cupoUtilizado: 700000,              // Used credit amount
    telefono: "318-555-0001",           // Phone number (optional)
    direccion: "Address",               // Address (optional)
    email: "client@email.com",          // Email (optional)
    activo: true,                       // Active status (optional)
    marca: "CHEVROLET",                 // Vehicle brand (optional)
    modelo: "NPR",                      // Vehicle model (optional)
    año: "2020",                        // Vehicle year (optional)
    tipoVehiculo: "CAMION"              // Vehicle type (optional)
  }
]
```

## Error Handling

- Returns `'fail'` for authentication errors (401/Unauthorized)
- Returns `'fail'` for missing station GUID
- Returns `'fail'` for network errors in production
- Returns mock data in development mode when API is unavailable

## Field Mapping

The service handles multiple possible field names from the API response:

| Service Field | Possible API Fields |
|---------------|-------------------|
| `cliente` | `cliente`, `nombreCliente`, `Cliente` |
| `nit` | `nit`, `identificacion`, `Nit`, `CC` |
| `placa` | `placa`, `Placa`, `numeroPlaca`, `licencePlate` |
| `cupoAsignado` | `cupoAsignado`, `cupoTotal`, `CupoAsignado` |
| `cupoDisponible` | `cupoDisponible`, `cupoRestante`, `CupoDisponible` |
| `marca` | `marca`, `Marca` |
| `modelo` | `modelo`, `Modelo` |
| `año` | `año`, `anio`, `year`, `Año` |
| `tipoVehiculo` | `tipoVehiculo`, `tipo`, `TipoVehiculo` |

## Key Differences from GetCuposPorClientes

1. **Additional Field**: Includes `placa` (license plate) as a required field
2. **Vehicle Info**: Additional optional fields for vehicle details (brand, model, year, type)
3. **API Endpoint**: Different endpoint (`/CuposPorAutomotores` vs `/CuposPorClientes`)
4. **Mock Data**: Includes vehicle-specific sample data

## Dependencies

- `HttpService.js` - For making HTTP requests
- `localStorage` - For retrieving station GUID
- `window.SERVER_URL` - For API base URL

## Development Features

- Mock data with realistic vehicle information
- Detailed console logging for debugging
- Robust error handling and fallbacks
- Flexible field mapping for API compatibility

## Component Integration

This service is specifically designed for the `PorAutomotores.js` component, which displays:
- Client name and identification
- Vehicle license plate
- Credit limit information
- Debt calculations
- PDF report generation

## Notes

- All monetary values are parsed as floats
- The service calculates debt as `cupoAsignado - cupoDisponible`
- Mock data includes multiple vehicles per client to simulate real scenarios
- Vehicle information fields are optional and won't break if not provided by the API
