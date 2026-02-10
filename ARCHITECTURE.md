# Facturador - Architecture Context

## Overview
Gas station (EDS) invoicing system. Manages fuel sales (combustible), convenience store items (canastilla), third-party clients (terceros), shift management (turnos), loyalty (fidelización), and electronic invoicing. Connects to a remote SIGES platform for data sync.

---

## Project Structure

```
Facturador/
├── FacturadorAPI/                    # .NET 6 Solution (two API projects)
│   ├── FacturadorAPI/                # API project - SIGES single-DB scheme
│   └── FacturadorApiSP/             # API project - SP multi-DB scheme (Estacion + Facturacion)
├── facturador-web/                   # React 18 SPA (main UI)
├── VisualizadorEstatico/            # React 18 SPA (read-only visualizer, simplified)
├── *.sql                            # SQL scripts (stored procedures, schema)
└── empleados.rpt                    # Crystal Report
```

---

## 1. Backend: FacturadorAPI (SIGES single-DB)

### Tech Stack
- .NET 6 Web API (minimal hosting)
- MediatR 10 (CQRS pattern)
- System.Data.SqlClient (raw ADO.NET, stored procedures)
- Newtonsoft.Json
- Swagger (Swashbuckle)
- No ORM (no EF Core)

### Architecture Pattern: CQRS with MediatR
```
Controller → MediatR.Send(Command/Query)
    → Handler → IDataBaseHandler (repository) → SQL Stored Procedures
    → Handler → IConexionEstacionRemota (external SIGES API calls)
    → Handler → IFidelizacion (loyalty API calls)
```

### Database
- **Single connection string** `EstacionSiges` used for all operations
- `ConnectionString` in `BaseRepository` is `abstract` (returns `Settings.EstacionSiges`)
- All DB operations use **stored procedures** via `LoadDataTableFromStoredProcAsync`
- Results converted via static extension methods in `Convertidor.cs`

### Connection Strings (appsettings.json)
```json
"ConnectionStrings": {
  "Estacion": "...(estacionsiges DB)...",
  "Facturacion": "...(estacionsiges DB)...",
  "EstacionSiges": "...(estacionsiges DB)..."
}
```
All three point to the **same database** in this project.

### Key Stored Procedures Used
| SP Name | Purpose |
|---------|---------|
| `ObtenerTipoIdentificaciones` | List ID types |
| `BuscarFormasPagosSiges` | List payment methods |
| `ObtenerCaras` | List pump faces |
| `ObtenerFacturaPorVenta` | Get last invoice by pump face |
| `ObtenerTurnoIsla` | Get shift by island |
| `ObtenerTercero` | Search client by ID |
| `ConvertirAFactura` | Convert sale to invoice |
| `ConvertirAOrden` | Convert sale to dispatch order |
| `MandarImprimir` | Trigger print by sale ID |
| `GetFacturaPorIdVenta` | Get invoice by sale ID |
| `CambiarEstadoFactursEnviada` | Mark invoices as sent |
| `UpdateOrCreateCanastilla` | Sync canastilla items |
| `GetCanastilla` | Get canastilla items |
| `AbrirTurno` / `CerrarTurno` | Open/close shifts |

### Controllers (7 total, identical names in both projects)
| Controller | Routes | Purpose |
|------------|--------|---------|
| `FacturasController` | `/api/Facturas/*` | Invoice operations (print, convert, e-invoice, payment methods) |
| `EstacionController` | `/api/Estacion/*` | Station info (islands, faces, shifts, dispensers) |
| `TercerosController` | `/api/Terceros/*` | Client/third-party CRUD |
| `TurnosController` | `/api/Turnos/*` | Shift open/close/reprint |
| `CanastillaController` | `/api/Canastilla/*` | Convenience store items |
| `FidelizacionController` | `/api/Fidelizacion/*` | Loyalty point operations |
| `ReportesController` | `/api/Reportes/*` | Article and readings reports |

### Commands (Application/Commands/)
| Command | Purpose |
|---------|---------|
| `AbrirTurnoCommand` | Open shift on island |
| `CerrarTurnoCommand` | Close shift on island |
| `MandarImprimirCommand` | Print invoice (facturaPOSId, terceroId, formaPago, ventaId, placa, kilometraje) |
| `MandarImprimirConsecutivoCommand` | Print by consecutive number |
| `ConvertirAFacturaCommand` | Convert sale → invoice |
| `ConvertirAOrdenCommand` | Convert sale → dispatch order |
| `EnviarFacturaElectronicaCommand` | Send electronic invoice |
| `FidelizarVentaCommand` | Apply loyalty to sale |
| `CrearTerceroCommand` | Create new client |
| `AgregarFacturaCanastillaCommand` | Add canastilla invoice |
| `ProcesarYObtenerCanastillasCommand` | Sync & get canastilla items |
| `ReimprimirTurnoCommand` | Reprint shift report |

### Queries (Application/Queries/)
| Query | Purpose |
|-------|---------|
| `ListarFormasPagoSigesQuery` | Get payment methods |
| `ListarIslasSigesQuery` | Get islands |
| `ListarTiposIdentificacionQuery` | Get ID types |
| `ObtenerCarasPorIslaQuery` | Get faces per island |
| `ObtenerSurtidoresQuery` | Get dispensers |
| `ObtenerTerceroPorIDentificacionQuery` | Search client by ID |
| `ObtenerTurnoPorIslaQuery` | Get shift by island |
| `ObtenerUltimaFacturaPorCaraQuery` | Get last invoice on face |
| `ObtenerUltimaFacturaPorCaraTextoQuery` | Get last invoice text |
| `ReporteArticuloQuery` | Article report |
| `ReporteLecturasGeneralQuery` | General readings report |

### Key Models
| Model | Description |
|-------|-------------|
| `InfoEstacion` | Station config (name, NIT, address, flags, URLs, printer settings) |
| `FacturaSiges` | Invoice (ventaId, consecutivo, tercero, manguera, amounts, dates) |
| `Tercero` | Client (terceroId, identificacion, nombre, direccion, correo) |
| `Canastilla` | Store item (guid, descripcion, unidad, precio, iva) |
| `TurnoSiges` | Shift info |
| `CaraSiges` | Pump face |
| `IslaSiges` | Island |
| `SurtidorSiges` | Dispenser (contains list of caras) |
| `MangueraSiges` | Hose |
| `FormaPagoSiges` | Payment method |
| `Resolucion` | Invoice resolution |
| `Combustible` | Fuel type |
| `TurnoSurtidor` | Shift-dispenser relation |
| `TipoIdentificacion` | ID type |

### External Integrations
| Interface | Implementation | Purpose |
|-----------|---------------|---------|
| `IConexionEstacionRemota` | `ConexionEstacionRemota` | SIGES remote API (send invoices, get orders, get tokens) |
| `IFidelizacion` | `FidelizacionConexionApi` | Loyalty points API |

### DI Registration (Program.cs)
```csharp
services.AddMediatR(typeof(IApplicationAnchor));
services.Configure<InfoEstacion>(...)
services.Configure<ConnectionStringSettings>(...)
services.AddScoped<IConexionEstacionRemota, ConexionEstacionRemota>();
services.AddScoped<IFidelizacion, FidelizacionConexionApi>();
services.AddRepositories(); // → IDataBaseHandler → MsSqlDataBaseHandler
```

---

## 2. Backend: FacturadorApiSP (SP multi-DB scheme)

### Key Differences from FacturadorAPI

| Aspect | FacturadorAPI | FacturadorApiSP |
|--------|---------------|-----------------|
| **DB scheme** | Single DB (`EstacionSiges`) | Multiple DBs (`Estacion` = ZE900NG, `Facturacion` = Facturacion_Electronica) |
| **ConnectionString** | `abstract` property → `Settings.EstacionSiges` | `public set` property → assigned per-method (`_settings.Facturacion` or `_settings.Estacion`) |
| **Factura model** | `FacturaSiges` (flat) | `Factura` (has `Venta` object, `Ventas` collection, `enviadaFacturacion` flag) |
| **Extra models** | — | `Factura`, `Venta`, `Bolsa`, `Cara`, `Isla`, `Manguera`, `Surtidor`, `FormasPagos`, `FacturaCanastillaRequest` |
| **Extra commands** | — | `AgregarBolsaCommand`, `MandarImprimirTurnoCanastillaCommand` |
| **MandarImprimir** | 6 params | 8 params (adds `NumeroTransaccion`, `Impresiones`) |
| **IDataBaseHandler** | ~40 methods | ~55 methods (adds `getIslas`, `ListarFormasPagoSP`, `ListarCarasSp`, `ListarSurtidoresSP`, `getUltimasFacturas`, `ObtenerFacturaPorConsecutivo`, `getBolsa`, `getFacturaPorTurno`, `ObtenerTurnoIslaYFecha`, `MandarImprimirObjeto`) |
| **Platform** | AnyCPU | AnyCPU + x86 |
| **EnviarFacturaElectronica** | No `NumeroTransaccion` in command | Includes `NumeroTransaccion` |

### SP ConnectionString Strategy
Each method in `MsSqlDataBaseHandler` sets `ConnectionString` before calling the SP:
```csharp
ConnectionString = _settings.Facturacion;  // for invoice/client operations
ConnectionString = _settings.Estacion;     // for raw station data (caras, islas, ventas)
```

---

## 3. Frontend: facturador-web

### Tech Stack
- React 18.2 (CRA - Create React App)
- React Router v6
- React Bootstrap 2.9 + Bootstrap 5.3
- STOMP over WebSocket (RabbitMQ integration)
- No state management library (useState/props)

### Runtime Configuration
Config loaded via `public/config.js` (global `window` variables):
```javascript
var SERVER_URL = "https://localhost:7269";
var RabbitWebSocket = "ws://...";
var ConvertirAFactura = false;
var ConvertirAOrden = false;
var GenerarFacturaelectronica = true;
var DesabilitaFormasNoCredito = true;
var FormasPagos = [1,4,98];
var palabrasPermitidas = ["PIMPINAS","CANECAS",...];
```

### Routes
| Path | Component | Description |
|------|-----------|-------------|
| `/` | `Combustible` | Main fuel invoicing view (default) |
| `/canastilla` | `Canastilla` | Convenience store items |
| `/terceros` | `Terceros` | Client management |

### Components (src/components/)
| Component | Purpose |
|-----------|---------|
| `App.js` | Router + layout |
| `navbar.js` | Navigation (Combustible / Canastilla / Terceros) |
| `combustible.js` | **Main view** (~700 lines): island/face selection, invoice display, print, e-invoice, loyalty |
| `canastilla.js` | Convenience store management |
| `terceros.js` | Client search/create |
| `modalImprimir.js` | Print invoice modal |
| `modalImprimirPorConsecutivo.js` | Print by consecutive number |
| `modalFacturaElectronica.js` | Electronic invoice modal |
| `modalAbrirTurno.js` | Open shift modal |
| `modalCerrarTurno.js` | Close shift modal |
| `modalAddTercero.js` | Add new client modal |
| `modalAgregarBolsa.js` | Add bag/cash modal |
| `modalFidelizarVenta.js` | Loyalty modal |
| `modalReimprimirTurno.js` | Reprint shift modal |
| `alertaError.js` | Error alert |
| `alertaTercero.js` | Client alert |
| `alertTerceroAgregadoExitosamente.js` | Client added success |
| `alertTerceroNoExisteCanastilla.js` | Client not found (canastilla) |
| `AlertVentaExitosa.js` | Successful sale alert |
| `rabbitWebSocket.js` | RabbitMQ/STOMP WebSocket integration |

### API Service Layer (src/Services/getServices/)
All services use `fetch()` with `window.SERVER_URL` as base:
| Service File | API Endpoint | Method |
|-------------|-------------|--------|
| `GetIslas.js` | `/api/Estacion/Islas` | GET |
| `GetCarasPorIsla.js` | `/api/Estacion/CarasPorIsla` | GET |
| `GetTurnoIsla.js` | `/api/Estacion/TurnoPorIsla` | GET |
| `GetFormasDePago.js` | `/api/Facturas/FormasDePago` | GET |
| `GetTiposDeIdentificacion.js` | `/api/Terceros/TiposIdentificacion` | GET |
| `GetUltimaFacturaporCara.js` | `/api/Facturas/UltimaFacturaPorCara/{id}` | GET |
| `GetUltimaFacturaPorCaraTexto.js` | `/api/Facturas/UltimaFacturaPorCara/{id}/Texto` | GET |
| `GetTercero.js` | `/api/Terceros/{id}` | GET |
| `GetCanastilla.js` | `/api/Canastilla` | GET |
| `ConvertiraFactura.js` | `/api/Facturas/ConvertirAFactura/{id}` | GET |
| `ConvertirAOrden.js` | `/api/Facturas/ConvertirAOrden/{id}` | GET |
| `ImprimirFactura.js` | `/api/Facturas/Imprimir/{...}` | POST |
| `ImprimirPorConsecutivo.js` | `/api/Facturas/ImprimirPorConsecutivo/{c}` | POST |
| `ImprimirNativo.js` | Direct print (no API call?) | — |
| `EnviarFacturaElectronica.js` | `/api/Facturas/EnviarFacturaElectronica/{...}` | POST |
| `PostTercero.js` | `/api/Terceros` | POST |
| `PostCanastilla.js` | `/api/Canastilla` | POST |
| `AbrirTurno.js` | `/api/Turnos/AbrirTurno/{isla}/{codigo}` | POST |
| `CerrarTurno.js` | `/api/Turnos/CerrarTurno/{isla}/{codigo}` | POST |
| `ReimprimirTurno.js` | `/api/Turnos/reimprimirTurno/{...}` | POST |
| `FidelizarVenta.js` | `/api/Fidelizacion/FidelizarVenta/{...}` | POST |
| `AgregarBolsa.js` | (SP-specific) | POST |
| `PostImprimirTurnoCanastilla.js` | (SP-specific) | POST |

---

## 4. Frontend: VisualizadorEstatico

Simplified read-only React app. Same stack but fewer dependencies (no STOMP/WebSocket, no react-to-print). Contains only:
- `App.js`, `navbar.js`, `combustible.js`, `canastilla.js`, `terceros.js`
- No modals, no service layer
- Likely for display/monitoring purposes only

---

## 5. SQL Scripts

| File | Purpose |
|------|---------|
| `StoreProceduresFacturas.sql` | Stored procedures for invoice operations |
| `StoreProceduresVentas.sql` | Stored procedures for sales |
| `EstacionSIGES.sql` | Station schema / SIGES-related tables |
| `Canastilla.sql` | Canastilla tables/procedures |
| `ObjetosImprimir.sql` | Print-related objects |
| `infocupos.sql` | Quota info |
| `scriptpasoinfosiges.sql` | SIGES data migration script |

---

## 6. Key Configuration (appsettings.json)

### InfoEstacion (station-level settings)
| Property | Type | Purpose |
|----------|------|---------|
| `Surtidores` | int[] | Dispenser IDs |
| `RabbitHost` | string | RabbitMQ hostname |
| `Isla` | string | Island identifier |
| `UrlLocalService` | string | Local print service URL |
| `Url` | string | SIGES remote API URL |
| `UrlFidelizacion` | string | Loyalty API URL |
| `User/Password` | string | SIGES credentials |
| `UserFidelizacion/PasswordFidelizacion` | string | Loyalty credentials |
| `CentroVenta` | string | Sale center ID |
| `NitCentroVenta` | string | Sale center NIT |
| `ConvertirAFactura` | bool | Auto-convert to invoice |
| `ConvertirAOrden` | bool | Auto-convert to dispatch order |
| `CreaMovimientoContable` | bool | Create accounting entry |
| `ImpresionAutomatica` | bool | Auto-print |
| `GeneraFacturaElectronica` | bool | Electronic invoice enabled |
| `ImpresionPDA` | bool | PDA print mode |
| `vecesPermitidasImpresion` | int | Max reprint count |
| `CaracteresPorPagina` | int | Print chars per page |
| `ip/puerto` | string | Printer IP/port |

### CarasImpresoras (face → printer mapping)
```json
"CarasImpresoras": [
  { "Cara": "Cara 1", "Impresora": "IMP 1" },
  ...
]
```

### Sicom (government reporting integration)
```json
"Sicom": {
  "Url": "https://eds.sicom.gov.co/eds/api/v1",
  "Usuario": "...",
  "Contrasena": "...",
  "PuertoBoton": "COM8",
  "APIKey": "..."
}
```

---

## 7. Namespaces Map

Both API projects share similar namespaces (some legacy naming):

| Namespace | Used In | Contains |
|-----------|---------|----------|
| `FacturadorAPI.Controllers` | Both | All controllers |
| `FacturadorAPI.Application.Commands` | Both | Command + Handler classes |
| `FacturadorAPI.Application.Queries` | Both | Query + Handler classes |
| `FacturadorAPI.Models` | Both | Shared models (Tercero, FacturaSiges, Canastilla, etc.) |
| `FacturadorAPI.Models.Externos` | Both | External API models (Estacion, FacturaExterna, etc.) |
| `FacturadorApiSP.Models` | SP only | SP-specific models (Bolsa, Factura, Venta, etc.) |
| `FactoradorEstacionesModelo.Objetos` | SP only | Factura, Venta models |
| `MachineUtilizationApi.Repository` | Both | BaseRepository, ConnectionStringSettings, IDataBaseHandler, MsSqlDataBaseHandler |
| `MachineUtilizationApi.Extensions` | Both | RepositoryExtensions |
| `MachineUtilizationApi.Config` | Both | RepositoriesConfig |
| `FacturadorAPI.Repository` | Both | Convertidor (DataTable → Model mappers) |
| `FacturadorAPI.Repository.Repo` | Both | IConexionEstacionRemota, ConexionEstacionRemota |
| `FacturadorEstacionesRepositorio` | Both | IFidelizacion, FidelizacionConexionApi |
| `MachineUtilizationApi` | Both | ApiException, ApiExceptionFilter |

---

## 8. Data Flow Summary

### Fuel Invoice Flow
```
User selects Island → Face → sees last invoice
  → Enters Placa, Kilometraje, Tercero, FormaPago
  → Clicks Print → POST /api/Facturas/Imprimir/{...}
    → MandarImprimirCommandHandler:
      1. GetToken from SIGES remote
      2. GetFacturaPorIdVenta (local DB)
      3. ActualizarFactura (update client, payment, plate)
      4. EnviarFacturas to SIGES remote (sync)
      5. MandarImprimir (trigger local print via SP)
```

### Canastilla Flow
```
GET /api/Canastilla → ProcesarYObtenerCanastillasCommand
  → Sync items from remote SIGES → Update local DB → Return items
POST /api/Canastilla → AgregarFacturaCanastillaCommand
  → GenerarFacturaCanastilla in local DB
```

### Shift Flow
```
POST /api/Turnos/AbrirTurno/{isla}/{codigo}
POST /api/Turnos/CerrarTurno/{isla}/{codigo}
POST /api/Turnos/reimprimirTurno/{fecha}/{isla}/{posicion}
```

---

## 9. Important Notes

1. **No authentication** currently active (JWT middleware commented out in Program.cs)
2. **CORS**: open to all origins (`*`)
3. **No EF Core**: all data access is raw ADO.NET + stored procedures
4. **Both API projects are independent** but share the same controller names and similar code
5. **FacturadorApiSP switches connection strings per-method** while FacturadorAPI uses a single one
6. **Frontend uses global window vars** for config (set in public/config.js, not environment variables)
7. **Legacy namespace naming**: `MachineUtilizationApi` is used throughout despite being a billing app
8. **Convertidor.cs** pattern: DataTable extension methods that manually map columns to models
