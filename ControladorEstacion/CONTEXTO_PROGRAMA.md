# Contexto del programa ControladorEstacion

## 1) Que es este programa
`ControladorEstacion` es una aplicacion WinForms (.NET 8, x86) orientada a operacion de estacion de servicio.
Tiene dos funciones principales:

- Monitorear en pantalla el estado de surtidores en tiempo real (turno, islero y estado par/impar).
- Generar reportes PDF de lecturas y ventas por rango de fechas.

Tambien actua como capa de orquestacion entre UI, repositorio de datos y mensajeria RabbitMQ.

## 2) Stack y dependencias
- Plataforma: WinForms, `net8.0-windows7.0`, `PlatformTarget=x86`.
- DI/Host: `Microsoft.Extensions.Hosting` + `Microsoft.Extensions.DependencyInjection`.
- Datos: `IEstacionesRepositorio` (implementacion `EstacionesRepositorioSqlServer`).
- Mensajeria: `RabbitMQ.Client` (consumo de cola por isla).
- Reportes: `QuestPDF`.
- Logging: `NLog`.
- JSON: `Newtonsoft.Json`.

Referencias de proyecto:
- `FactoradorEstacionesModelo`
- `FacturadorEstacionesRepositorio`

## 3) Flujo de arranque (startup)
Archivo clave: `Program.cs`

1. Configura NLog (archivo diario en `logs/<fecha>.log` y consola).
2. Inicializa WinForms (`SetHighDpiMode`, `EnableVisualStyles`).
3. Crea `HostBuilder` con:
   - `appsettings.json`
   - DI para repositorio, conexion remota, formulario principal
   - binding de `ConnectionStrings` e `InfoEstacion`
4. Resuelve `Form1` desde DI.
5. Ejecuta `Application.Run(mainForm)`.

## 4) Pantalla principal y comportamiento
Archivo clave: `Form1.cs`

En el constructor de `Form1`:
1. Guarda repositorio e informacion de estacion (`InfoEstacion`).
2. Crea receptor de mensajes RabbitMQ: `RabbitMQMessagesReceiver`.
3. Consulta surtidores: `_estacionesRepositorio.GetSurtidoresSiges()`.
4. Por cada surtidor:
   - Crea control visual `Surtidor`.
   - Lo posiciona verticalmente en el formulario.
   - Se suscribe al stream de mensajes (`IObservable<string>`).

Botones principales:
- `Reporte de lecturas` -> abre `FormReporte("lecturas", ...)`
- `Reporte de ventas` -> abre `FormReporte("ventas", ...)`

## 5) Monitoreo en tiempo real de surtidores
Archivo clave: `Surtidor.cs`

`Surtidor` implementa `IObserver<string>`.

- `OnNext(string value)` deserializa JSON a `Mensaje`.
- Si el `SurtidorId` coincide con el control actual:
  - Actualiza Turno
  - Actualiza Islero
  - Actualiza estado de ubicacion `Par` o `Impar`

Como los mensajes llegan en hilo no UI, usa `InvokeRequired` + `Invoke` para actualizar labels de forma segura.

Modelo esperado del mensaje (`Mensaje`):
- `SurtidorId`
- `Estado`
- `Ubicacion`
- `Turno`
- `Empleado`

## 6) Receptor de RabbitMQ
Archivo clave: `FacturadorEstacionesRepositorio/Messages/RabbitMQMessagesReceiver.cs`

Responsabilidades:
- Inicializa conexion/canal RabbitMQ perezosamente (`EnsureInitializedAsync`).
- Declara cola con nombre `_infoEstacion.Isla`.
- Hace `BasicConsume` con `autoAck=true`.
- Reenvia cada mensaje recibido a todos los observers registrados.

Datos de conexion:
- Host: `_infoEstacion.RabbitHost`
- Puerto: `5672`
- Usuario/clave: `siges/siges`
- Queue: `_infoEstacion.Isla`

## 7) Generacion de reportes PDF
Archivo clave: `FormReporte.cs`

Entrada:
- Rango de fechas (`dateTimePicker1`, `dateTimePicker2`).

Consulta base:
- `GetFacturasPorFechas(fechaInicio, fechaFin + 1 dia)`
- `GetTurnosByFechas(fechaInicio, fechaFin + 1 dia)`

Salida:
- PDF en: `{InfoEstacion.Reportes}/reporte-{tipo}-{desde}-{hasta}.pdf`

### 7.1 Reporte de lecturas (`tipo = lecturas`)
- Recorre turnos y detalle por surtidor (`ObtenerTurnoInfo`).
- Omite filas sin cierre (`Cierre == null`).
- Calcula por fila:
  - Diferencia = `Cierre - Apertura`
  - Ventas = `Diferencia * Precio`
- Acumula totales de cantidad y venta.
- Genera tabla PDF con QuestPDF.

### 7.2 Reporte de ventas (`tipo = ventas`)
Construye dos bloques:

1. Resumen por articulo
- Agrupa facturas por combustible (`Combustible`).
- Calcula ventas, cantidad, valor neto, descuento, recaudo, total.

2. Resumen por forma de pago
- Obtiene catalogo `BuscarFormasPagosSiges()`.
- Maneja una o dos formas de pago por factura (`codigoFormaPago`, `codigoFormaPago2`).
- Prorratea cantidad cuando hay dos medios de pago segun participacion del total.
- Agrupa por codigo y genera tabla de ventas/cantidad/total.

## 8) Configuracion relevante (appsettings.json)
Secciones importantes:
- `InfoEstacion`
  - `RabbitHost`, `Isla`, `Reportes`
  - Datos de empresa para encabezado de reportes (`Razon`, `NIT`)
- `ConnectionStrings`
  - Conexion a SQL Server (`EstacionSiges`)
- `CarasImpresoras` y opciones operativas de impresion/facturacion.

## 9) Mapa rapido de archivos
- `Program.cs`: bootstrap, logging, host DI
- `Form1.cs` + `Form1.Designer.cs`: pantalla principal
- `Surtidor.cs`: widget por surtidor (observer de mensajes)
- `FormReporte.cs` + `FormReporte.Designer.cs`: filtros y construccion PDF
- `appsettings.json`: parametros de estacion, rutas y conexiones
- `FacturadorEstacionesRepositorio/Messages/RabbitMQMessagesReceiver.cs`: consumo de cola
- `FacturadorEstacionesRepositorio/IEstacionesRepositorio.cs`: contrato de acceso a datos

## 10) Puntos tecnicos a tener presentes
1. `OnCompleted()` y `OnError()` en `Surtidor` lanzan `NotImplementedException`.
   - Si se invocan, pueden romper la UI.
2. `ReceiveMessages` en RabbitMQ receiver es `async void`.
   - Dificulta manejo de errores y pruebas.
3. Hay credenciales/configuracion sensible en `appsettings.json`.
   - Conviene migrarlas a secretos por entorno.
4. En `Program.cs` se usa logging manual y ademas existe `nlog.config`.
   - Revisar si hay configuracion duplicada.

## 11) Flujo mental en 20 segundos
1. Inicia app -> crea host + carga config.
2. Abre `Form1` -> crea controles de surtidor.
3. Se conecta a RabbitMQ -> cada mensaje actualiza un control `Surtidor`.
4. Usuario pide reporte -> `FormReporte` consulta repositorio.
5. QuestPDF genera el PDF en carpeta de reportes.

---

Si necesitas, el siguiente paso natural es crear un segundo md llamado `GUIA_DEBUG_CONTROLADORESTACION.md` con checklist de fallas tipicas (Rabbit no conecta, PDF bloqueado, data vacia, etc.).
