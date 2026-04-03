# Refactoring Plan: Separate Protocol from Operations

## Current Architecture Problem

`OperadorCara.cs` is tightly coupled to ASPRO serial protocol:
- Serial port management mixed into operational logic
- Frame parsing (GetTRamaVenta, GetTRamaTotalizador, etc.) embedded in business logic
- ASPRO-specific details (headers, fallback codes, frame structure) hardcoded
- Impossible to swap in Silog, Prosoft, or other protocols

## Proposed Solution: Strategy Pattern with IPumpProtocol

### Phase 1: Create Protocol Interface
✅ Already created: `IPumpProtocol.cs`

### Phase 2: Create ASPRO Implementation
Create `AsproProtocol.cs` that implementations:
- Serial port management (currently in OperadorCara `serialPort1`)
- Frame parsing (move `GetTRamaVenta`, `GetTRamaTotalizador`, `GetTRamaAutorizar`, `GetTRamaDesautorizar`)
- Status code interpretation (move `VerificarEstado` parsing logic)
- Response synchronization (move `respondio`, `finalizo`, `DataReceiverHandler`)

### Phase 3: Refactor OperadorCara
Remove all protocol-specific code and depend on `IPumpProtocol`:
- Remove `SerialPort serialPort1`
- Remove `respondio`, `finalizo`, `count` globals
- Remove all `GetTRama*` methods
- Remove frame parsing logic
- Replace direct calls with protocol interface calls

### Phase 4: Create Other Protocol Implementations
- `SilogProtocol.cs`
- `ProsoftProtocol.cs`
- Any other protocol...

---

## Mapping: Current Code → IPumpProtocol

| Current Method in OperadorCara | Maps to IPumpProtocol | Notes |
|---|---|---|
| `autorizarManguera()` | `AuthorizeHoseAsync()` | Wraps EnviarTramaAsync + frame building |
| `desautorizarManguera()` | `DeauthorizeHoseAsync()` | Same as above |
| `venta()` (reads last sale) | `ReadLastSaleAsync()` | Handles frame parsing, returns double |
| `totalizadorManguera()` (reads totalizer) | `ReadTotalizerAsync()` | Handles frame parsing, returns double |
| `estado()` and `VerificarEstado()` | `GetStatusAsync()` | Parses B2/80/00/20 codes, returns per-hose status |
| `DataReceiverHandler()` | Protocol owns; fires `DataReceived` event | OperadorCara listens for unsolicited events |
| `EnviarTramaAsync()` | Protocol internal | Protocol manages request/response correlation |
| `GetTRamaVenta()` | Protocol internal | ASPRO-specific parsing |
| `GetTRamaTotalizador()` | Protocol internal | ASPRO-specific parsing |
| `GetTRamaAutorizar()` | Protocol internal | ASPRO-specific parsing |
| `GetTRamaDesautorizar()` | Protocol internal | ASPRO-specific parsing |
| `FromHex()` | Protocol internal or shared utility | Move to protocol if ASPRO-specific |

---

## Refactoring Steps

### Step 1: Wrap Current ASPRO Logic in AsproProtocol.cs
```csharp
public class AsproProtocol : IPumpProtocol
{
    private SerialPort serialPort1 = null!;
    private bool respondio;
    private bool finalizo;
    private int count = 0;
    // ... all current OperadorCara protocol code moves here
}
```

### Step 2: Move Methods to AsproProtocol
Move these wholesale from OperadorCara:
- `EnviarTramaAsync()` - becomes internal to AsproProtocol
- `GetTRamaVenta()` - becomes internal
- `GetTRamaTotalizador()` - becomes internal
- `GetTRamaAutorizar()` - becomes internal
- `GetTRamaDesautorizar()` - becomes internal
- `VerificarEstado()` - becomes internal, fires status via `DataReceived` event
- `DataReceiverHandler()` - becomes internal
- `DrenarRespuestaCorta()` - becomes internal
- `FromHex()` - becomes internal or shared

### Step 3: Implement Protocol Methods in AsproProtocol
```csharp
public async Task AuthorizeHoseAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
{
    // Call internal EnviarTramaAsync with ASPRO frame format
    // Use GetTRamaAutorizar to parse response, handle DrenarRespuestaCorta
    // Return when complete
}

public async Task<double> ReadLastSaleAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
{
    // Call internal EnviarTramaAsync with ASPRO "request last sale" frame
    // Call GetTRamaVenta to parse and return double
}

public async Task<double> ReadTotalizerAsync(SurtidorSiges surtidor, MangueraSiges manguera, CancellationToken stoppingToken)
{
    // Similar to ReadLastSaleAsync but for totalizer
}

public async Task<Dictionary<string, string>> GetStatusAsync(SurtidorSiges surtidor, CancellationToken stoppingToken)
{
    // Call internal VerificarEstado
    // Parse B2/80/00/20 codes into standard statuses
    // Return as Dictionary<HoseLocation, StatusCode>
}
```

### Step 4: Update OperadorCara Constructor and Dependencies
```csharp
public OperadorCara(
    Logger logger, 
    IEnumerable<SurtidorSiges> surtidores, 
    IEstacionesRepositorio estacionesRepositorio, 
    IOptions<Sicom> options, 
    ISicomConection sicomConection, 
    IMessageProducer messageProducer, 
    IFidelizacion fidelizacion, 
    Islas islas,
    IPumpProtocol pumpProtocol)  // NEW DEPENDENCY
{
    _pumpProtocol = pumpProtocol;
    // Remove: serialPort1 init, DataReceived handler subscription
}
```

### Step 5: Update All OperadorCara Method Calls
Replace all ASPRO calls with protocol calls:

**Before:**
```csharp
await desautorizarManguera(surtidor, manguera, stoppingToken);
manguera.Estado = "BuscandoUltimaVenta";
await venta(surtidor, manguera, stoppingToken);
await EsperarCambioVentaAsync(manguera, stoppingToken);
```

**After:**
```csharp
await _pumpProtocol.DeauthorizeHoseAsync(surtidor, manguera, stoppingToken);
manguera.Estado = "BuscandoUltimaVenta";
manguera.ultimaVenta = await _pumpProtocol.ReadLastSaleAsync(surtidor, manguera, stoppingToken);
// AutoResetEvent or similar signal mechanism (moved from venta() method)
manguera.CambioVenta = true;
```

### Step 6: Handle Status Updates via Event
Currently `VerificarEstado()` directly modifies manguera states. Move to event:

```csharp
// In OperadorCara constructor:
_pumpProtocol.DataReceived += OnProtocolDataReceived;

private void OnProtocolDataReceived(object? sender, ProtocolDataReceivedEventArgs e)
{
    if (e.ParsedData.TryGetValue("HoseStatuses", out var statuses))
    {
        var dict = (Dictionary<string, string>)statuses;
        foreach (var kvp in dict)
        {
            var manguera = surtidor.mangueras.FirstOrDefault(m => m.Ubicacion == kvp.Key);
            if (manguera != null)
            {
                ProcessHoseStatusChange(manguera, kvp.Value);
            }
        }
    }
}

private void ProcessHoseStatusChange(MangueraSiges manguera, string statusCode)
{
    // Move logic from VerificarEstado here
    // Transition states based on B2/80/00/20 codes
}
```

---

## Benefits

1. **Swappable Protocols**: Add Silog, Prosoft, etc. by implementing IPumpProtocol
2. **Testable**: Mock IPumpProtocol for unit tests
3. **Maintainable**: Operational logic separated from protocol details
4. **Clearer**: OperadorCara focuses on business logic (sales validation, vehicle auth, etc.)
5. **Robust**: Each protocol owns its own request/response correlation and frame parsing

---

## Implementation Order

1. ✅ Create `IPumpProtocol.cs` 
2. Create `AsproProtocol.cs` (move and wrap all current protocol code)
3. Refactor `OperadorCara.cs` to use IPumpProtocol
4. Update dependency injection to register `AsproProtocol` as `IPumpProtocol`
5. Add `SilogProtocol.cs` (new implementation for Silog protocol)
6. Add `ProsoftProtocol.cs` (new implementation for Prosoft protocol)
7. Add protocol selection logic in configuration/startup

---

## Notes for Implementation

- **Request/Response Correlation**: Each protocol is responsible for matching requests to responses. Use unique operation IDs or sequence numbers internally.
- **Timeout Handling**: Each protocol should implement its own timeouts (not infinite waits like current `EsperarEstabilidadTotalizadorAsync`).
- **Error Handling**: Protocol should throw exceptions on timeouts/failures; OperadorCara handles them at business logic level.
- **Status Reporting**: Currently `sendEstado()` is called directly. Move this to firing `DataReceived` events so other protocols can also report state changes.
- **Logging**: Keep detailed operational logs in OperadorCara; move protocol-level debug logs to AsproProtocol.
