# GetCuposPorClientes Service

## Overview
This service fetches client credit limits (cupos) for the current station from the backend API. It provides information about assigned credit limits, available credit, and outstanding debt for each client.

## Usage

```javascript
import GetCuposPorClientes from '../../services/GetCuposPorClientes'

const fetchData = async () => {
  const cupos = await GetCuposPorClientes()
  
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

The service calls: `GET /api/Reportes/CuposPorClientes/{estacionGuid}`

Where `estacionGuid` is retrieved from `localStorage.getItem('estacionGuid')`

## Response Format

The service returns an array of objects with the following structure:

```javascript
[
  {
    cliente: "Client Name",              // Client company name
    nit: "123456789-1",                 // Tax ID or document number
    cupoAsignado: 5000000,              // Total assigned credit limit
    cupoDisponible: 3200000,            // Available credit amount
    cupoUtilizado: 1800000,             // Used credit amount
    telefono: "318-555-0001",           // Phone number (optional)
    direccion: "Address",               // Address (optional)
    email: "client@email.com",          // Email (optional)
    activo: true                        // Active status (optional)
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
| `cupoAsignado` | `cupoAsignado`, `cupoTotal`, `CupoAsignado` |
| `cupoDisponible` | `cupoDisponible`, `cupoRestante`, `CupoDisponible` |

## Dependencies

- `HttpService.js` - For making HTTP requests
- `localStorage` - For retrieving station GUID
- `window.SERVER_URL` - For API base URL

## Development Features

- Mock data support when API is unavailable
- Detailed console logging for debugging
- Robust error handling and fallbacks
- Flexible field mapping for API compatibility

## Notes

- All monetary values are parsed as floats
- The service calculates debt as `cupoAsignado - cupoDisponible`
- Mock data is only returned in development mode with localhost API URLs
