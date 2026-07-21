# Plan: Soporte Dual de Fidelización (Servipunto + SIGES)

## Contexto

### Sistema actual (FacturadorApiSP + ImpresionService)
- `FidelizarVentaCommand` recibe **`IdCara`** (número de cara/dispensador)
- El handler resuelve el ventaId llamando a `GetFacturaPorIdVenta(request.IdCara)`
- Llama a `IFidelizacion.SubirPuntos()` via HTTP a `fidelizacion.api.sigessoluciones.com`
- Al finalizar llama a **`MandarImprimir(ventaId, 1)`** para forzar reimpresión del ticket
- Tiene código Servipunto (TCP socket) comentado que nunca se activa
- `ImpresionService.getLineasImprimir()` tiene el método `getPuntos()` definido pero **nunca lo llama** — los puntos no aparecen en el ticket impreso

### Sistema de referencia (FacturadorAPI + WorkerImpresion + EstacionSIGES.sql)
- `FidelizarVentaCommand` recibe **`IdVenta`** directamente
- Llama a `IFidelizacion.SubirPuntos()` (mismo API HTTP)
- **No** llama a `MandarImprimir` — la impresión la maneja `WorkerImpresion` por cola
- `WorkerImpresion.getLineasImprimir()` llama a `getPuntos(ventaId)` cuando forma de pago es efectivo y hay nombre de tercero → los puntos **sí aparecen** en el ticket
- SP `GetVentaFidelizarAutomaticaPorVenta` existe en `EstacionSIGES.sql` pero **no en `StoreProceduresFacturas.sql`**

---

## Objetivo

Permitir configurar en `FacturadorApiSP` (y su `ImpresionService`) cuál modo de fidelización usar:

| Modo | Cómo identifica la venta | Motor de fidelización | Impresión de puntos |
|------|--------------------------|----------------------|---------------------|
| **`Siges`** | Por `IdVenta` (directo) | `IFidelizacion` HTTP API + persistencia local SQL | Sí — aparecen en ticket |
| **`Servipunto`** | Por `IdCara` → resuelve ventaId | Protocolo TCP socket (`send_cmd`) hacia IP:puerto | No aplica (Servipunto maneja su propio display) |

---

## Archivos a modificar

1. `FacturadorAPI/FacturadorApiSP/appsettings.json`
2. `FacturadorAPI/FacturadorApiSP/Models/InfoEstacion.cs` (o donde viva el modelo de config)
3. `FacturadorAPI/FacturadorApiSP/Application/Commands/FidelizarVentaCommand.cs`
4. `FacturadorAPI/FacturadorApiSP/Application/Commands/FidelizarVentaCommandHandler.cs`
5. `EnviadorInformacionService/ImpresionService.cs`
6. `SP/StoreProceduresFacturas.sql`

---

## Cambios detallados

### 1. `appsettings.json` — agregar campo de configuración

Agregar en la sección `InfoEstacion`:

```json
"TipoFidelizacion": "Servipunto"
```

Valores válidos: `"Servipunto"` | `"Siges"`

---

### 2. `InfoEstacion.cs` — agregar propiedad

```csharp
public string TipoFidelizacion { get; set; } = "Servipunto";
```

---

### 3. `FidelizarVentaCommand.cs` — agregar campo `IdVenta`

Actualmente solo tiene `IdCara`. Para el modo Siges se necesita `IdVenta`:

```csharp
public class FidelizarVentaCommand : IRequest
{
    public FidelizarVentaCommand(string identificacion, int idCara, int idVenta = 0)
    {
        Identificacion = identificacion;
        IdCara = idCara;
        IdVenta = idVenta;
    }

    public string Identificacion { get; }
    public int IdCara { get; }
    public int IdVenta { get; }   // ← nuevo campo (0 = no aplica / modo Servipunto)
}
```

> **Verificar**: ¿Dónde se construye `FidelizarVentaCommand`? (controller/endpoint)  
> Si el caller ya envía `IdVenta` en el request HTTP, simplemente mapearlo al command.  
> Si no, ver si se puede resolver con el `IdCara` como hoy y dejar `IdVenta` opcional.

---

### 4. `FidelizarVentaCommandHandler.cs` — lógica dual

Reemplazar el método `Handle` por una lógica que bifurca según `TipoFidelizacion`:

```csharp
public async Task<Unit> Handle(FidelizarVentaCommand request, CancellationToken cancellationToken)
{
    if (_infoEstacion.TipoFidelizacion == "Siges")
    {
        return await HandleSiges(request, cancellationToken);
    }
    else
    {
        return await HandleServipunto(request, cancellationToken);
    }
}
```

**Modo Siges** (`HandleSiges`) — idéntico a `FacturadorAPI.FidelizarVentaCommandHandler`:
```
1. GetVentaFidelizarAutomaticaPorVenta(request.IdVenta)
2. GetFacturaPorIdVenta(request.IdVenta)
3. _fidelizacion.SubirPuntos(...)
4. Si ok: ActualizarFacturaFidelizada + AddFidelizado por cada fidelizado
5. MandarImprimir(ventaId, 1)   ← necesario porque ImpresionService necesita el trigger
```

**Modo Servipunto** (`HandleServipunto`) — reactiva el código comentado:
```
1. Construir trama TCP "FIDELI" con IdCara + Identificacion
2. send_cmd(trama) hacia ip:puerto
3. Si respuesta contiene "FIDELIA": éxito
4. NO llama MandarImprimir (Servipunto maneja su propio flujo)
```

> **Nota importante**: Eliminar el código muerto `// 000170FIDELI019099599` y el bloque 
> completo comentado; moverlo limpiamente a `HandleServipunto`.

---

### 5. `ImpresionService.cs` — activar `getPuntos()` en el ticket

**El problema**: En `getLineasImprimir()`, la sección de pago en efectivo (`COD_FOR_PAG == 4`) muestra el nombre del tercero pero **no llama `getPuntos()`**.

WorkerImpresion lo hace así (referencia, línea ~490):
```csharp
// Dentro del bloque codigoFormaPago == 1 (efectivo), si nombreCompletoTercero no está vacío:
lineasImprimir.AddRange(getPuntos(_factura.ventaId));
```

**El cambio en ImpresionService**: En `getLineasImprimir()`, dentro del bloque `else` que corresponde a `COD_FOR_PAG == 4` (efectivo), después de agregar el nombre del tercero, agregar:

```csharp
// línea ~1170 en ImpresionService.cs
lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a : ", nombreTercero) + "", false));
lineasImprimir.AddRange(getPuntos(_factura.ventaId));   // ← agregar esta línea
```

> **Verificar**: `COD_FOR_PAG == 4` en ImpresionService = efectivo (equivale a `codigoFormaPago == 1` en WorkerImpresion).  
> Confirmar antes de ejecutar que el código de forma de pago para efectivo es 4 en el sistema Servipunto/SP.

---

### 6. `StoreProceduresFacturas.sql` — agregar SP faltante

`GetVentaFidelizarAutomaticaPorVenta` existe en `EstacionSIGES.sql` (línea ~3933) pero no en `StoreProceduresFacturas.sql`.

Copiar el SP desde `EstacionSIGES.sql` y agregarlo al final de `StoreProceduresFacturas.sql`:

```sql
IF EXISTS(SELECT * FROM sys.procedures WHERE Name = 'GetVentaFidelizarAutomaticaPorVenta')
    DROP PROCEDURE [dbo].[GetVentaFidelizarAutomaticaPorVenta]
GO
CREATE procedure [dbo].GetVentaFidelizarAutomaticaPorVenta
(@idVenta int)
as
begin try
    set nocount on;
    select TOP (1) Venta.total as ValorVenta, '' as DocumentoFidelizado,
        case when FacturasPOS.ventaId is null
             then convert(varchar, OrdenesDeDespacho.facturaPOSId)
             else Resoluciones.descripcion + '-' + convert(varchar, FacturasPOS.consecutivo)
        end as Factura
    from Venta
    left join FacturasPOS on FacturasPOS.ventaId = Venta.Id
    left join OrdenesDeDespacho on OrdenesDeDespacho.ventaId = Venta.Id
    inner join Resoluciones on
        (FacturasPOS.resolucionId is not null and FacturasPOS.resolucionId = Resoluciones.ResolucionId)
        or (OrdenesDeDespacho.resolucionId is not null and OrdenesDeDespacho.resolucionId = Resoluciones.ResolucionId)
    where venta.Id = @idVenta
end try
begin catch
    declare
        @errorMessage varchar(2000),
        @errorSeverity int,
        @errorState int,
        @errorProcedure nvarchar(128),
        @errorLine int;
    select
        @errorMessage = error_message(),
        @errorSeverity = error_severity(),
        @errorState = error_state(),
        @errorProcedure = error_procedure(),
        @errorLine = error_line();
    raiserror(N'<message>Error occurred in %s :: %s :: Line number: %d</message>',
        16, 1, @errorProcedure, @errorMessage, @errorLine);
end catch;
GO
```

---

## Lo que NO cambia

- La tabla `VentaFidelizada`, `Fidelizado` — ya existen en ambos SQLs
- `ActualizarFacturaFidelizada`, `AddFidelizado` SPs — ya existen en `StoreProceduresFacturas.sql`
- `FidelizacionConexionApi` — ya funciona correctamente para modo Siges
- El registro de `IFidelizacion` en `Program.cs` — se mantiene igual
- `ImpresionService.getPuntos()` — ya existe, solo necesita ser invocado

---

## Preguntas a confirmar antes de ejecutar

1. **`COD_FOR_PAG == 4` = efectivo** en el sistema Servipunto/SP? (En WorkerImpresion el efectivo es `codigoFormaPago == 1`)
2. **¿El endpoint que llama `FidelizarVentaCommand` envía `IdVenta`?** Si no, ¿se puede agregar ese campo al request HTTP del controller?
3. **Modo por defecto**: ¿`"Servipunto"` o `"Siges"`? El plan asume `"Servipunto"` para no romper instalaciones existentes.
4. **¿`MandarImprimir` en modo Siges?** En `FacturadorAPI` el handler NO llama MandarImprimir, pero en `FacturadorApiSP` se necesita porque `ImpresionService` no tiene otro trigger. El plan incluye llamar MandarImprimir en modo Siges también.
