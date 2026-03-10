# Analisis OperadorCara y registro de ventas

Documento de contexto tecnico para futuras sesiones de trabajo (GPT-5.3-Codex) sobre `ManejadorSurtidor/OperadorCara.cs`.

## 1) Algoritmo actual paso a paso

### 1.1 Inicializacion (`OperarCara`)
1. Construye la lista de mangueras por surtidor usando `caras -> GetMangueras(x.Id).First()`.
2. Abre puerto serial configurado (`serialPort1`) y deja `DataReceived` suscrito.
3. Para cada manguera, hace ciclo de arranque:
   - `desautorizarManguera(...)`
   - `Estado = "BuscandoUltimaVenta"` y lee `venta(...)`
   - `Estado = "Totalizadores"` y lee `totalizadorManguera(...)`
   - Guarda baseline en memoria:
     - `NuevoTotalizador = totalizador`
     - `NuevaVenta = ultimaVenta`
   - `Estado = "Lista"`

### 1.2 Loop principal (`OperarCara` -> `ProcesarSurtidorAsync`)
1. Consulta turno activo del surtidor (`ObtenerTurnoSurtidor`).
2. Segun estado de turno:
   - `IdEstado == 1`: apertura (`ProcesarAperturaTurnoAsync`)
   - `IdEstado == 3 || 4`: cierre (`ProcesarCierreTurnoAsync`)
   - `IdEstado == 2`: operacion normal (`ProcesarTurnoOperativoAsync`)
3. Hace `Task.Delay(1000)` por surtidor.

### 1.3 Apertura y cierre de turno
- En cada manguera se leen totalizadores y se envian al repositorio:
  - apertura: `EnviarTotalizadorApertura`
  - cierre: `EnviarTotalizadorCierre`
- Publica estados por cola con `sendEstado`.

### 1.4 Operacion normal (`ProcesarTurnoOperativoAsync`)
1. Si puerto serial esta cerrado, intenta reabrir.
2. Si alguna manguera esta en estados activos (`Desautorizar`, `BuscarBoton`, o `Colgada` vendiendo), procesa estado por manguera.
3. Si no, procesa reposo de mangueras y luego pregunta estado global del surtidor (`estado(...)`).

### 1.5 Maquina de estados de manguera
- `Desautorizar` -> `ManejarEstadoDesautorizarAsync`
  - Desautoriza, limpia `Vendiendo`, limpia `Vehiculo`.
- `BuscarBoton` -> `ManejarEstadoBuscarBotonAsync`
  - Desautoriza
  - Valida iButton (`ValidarBoton`)
  - Si vehiculo valido: autoriza y pasa a `Vendiendo`
  - Si no: queda desautorizada
- `Colgada` -> `ManejarEstadoColgadaAsync`
  - Si `Vendiendo == true`: entra a `ManejarFinVentaMangueraAsync`
  - Si no vendia: desautoriza en reposo

### 1.6 Fin de venta (`ManejarFinVentaMangueraAsync`)
1. Publica "Fin venta".
2. Desautoriza la manguera.
3. Lee ultima venta del dispensador:
   - `Estado = "BuscandoUltimaVenta"`
   - `venta(...)`
4. Si hay vehiculo y `ultimaVenta > 0`, decide si guardar con `DeterminarSiDebeVenderAsync`.
5. Si decide guardar:
   - `AgregarVenta(manguera.Id, ultimaVenta, vehiculo.idrom)`
   - Fidelizacion
   - `NuevaVenta = ultimaVenta`
6. Cierra estado local:
   - `Estado = "FinVenta"`
   - `Vendiendo = false`
   - `Vehiculo = null`

### 1.7 Regla actual para guardar (`DeterminarSiDebeVenderAsync`)
1. Toma baseline en memoria:
   - `totalizadorAnterior = NuevoTotalizador`
   - `ultimaVentaAnterior = NuevaVenta`
2. Pide totalizador antes de registrar venta.
3. Si totalizador NO cambia contra memoria (`CambioTotalizador`): bloquea venta.
4. Si cambia, espera estabilidad (`EsperarEstabilidadTotalizadorAsync`) y vuelve a validar.
5. Si al estabilizar vuelve al mismo valor anterior: bloquea venta.
6. Si se mantiene distinto: actualiza `NuevoTotalizador` y permite guardar.

### 1.8 Capa serial
- `EnviarTramaAsync` marca surtidor/manguera en espera y espera respuesta por flags globales:
  - `respondio`
  - `finalizo`
- `DataReceiverHandler` toma el objeto en espera y parsea segun estado:
  - `BuscandoUltimaVenta` -> `GetTRamaVenta`
  - `Totalizadores` -> `GetTRamaTotalizador`
  - sin manguera -> `VerificarEstado` (B2/80/00/20)

## 2) Puntos de inflexion (conflictos reales)

## 2.1 Conflicto A: filtro por totalizador puede perder ventas validas
Riesgo: ALTO (ya observado).

Detalle:
- Ahora una venta depende 100% de que el totalizador cambie.
- Si la lectura de totalizador llega tarde, incompleta, o repetida, la venta se bloquea aunque `ultimaVenta` si cambio.
- Este caso explica "ventas validas perdidas" despues del ajuste.

Escenario tipico:
1. Manguera termina venta real.
2. `GetTRamaVenta` trae valor nuevo correcto.
3. `GetTRamaTotalizador` devuelve valor viejo por latencia/ruido serial.
4. `DeterminarSiDebeVenderAsync` bloquea.

## 2.2 Conflicto B: sincronizacion global de respuesta serial
Riesgo: ALTO.

Detalle:
- `respondio`, `finalizo` y `count` son globales a toda la clase, no por request.
- El handler serial tambien es global y asincrono.
- Un evento de lectura puede destrabar una espera que no le corresponde (especialmente si hay ruido o respuestas atrasadas).

Efecto:
- Lectura asociada a comando equivocado.
- `ultimaVenta` o `totalizador` de una manguera pueden quedar con dato no correlacionado.
- Resultado posible: ventas duplicadas o ventas perdidas.

## 2.3 Conflicto C: parseo con headers de fallback
Riesgo: ALTO.

Detalle:
- `GetTRamaTotalizador` acepta fallback header `163030`.
- `VerificarEstado` acepta fallback `023030`.
- `GetTRamaVenta` usa fallback `0130303`.
- Estos prefijos son mas cortos y pueden coincidir con fragmentos parciales del buffer.

Efecto:
- Parse de frame incompleto/parcial.
- Substring en posiciones fijas puede tomar digitos no validos pero "parseables".
- Totalizador/venta pueden quedar igual (falso no-cambio) o cambiar falso.

## 2.4 Conflicto D: estabilizacion sin timeout
Riesgo: MEDIO-ALTO.

Detalle:
- `EsperarEstabilidadTotalizadorAsync` hace loop infinito hasta 2 lecturas consecutivas iguales.
- No hay timeout maximo ni "circuit breaker".

Efecto:
- Si el dispensador oscila o el parseo varia por ruido, puede tardar mucho.
- Se retrasa el ciclo y se abren ventanas de estados inconsistentes.

## 2.5 Conflicto E: comparacion exacta en estabilizacion
Riesgo: MEDIO.

Detalle:
- En estabilizacion se usa `if (totalizador == manguera.totalizador)` exacto.
- En otras partes ya se usa tolerancia (`CambioTotalizador`).

Efecto:
- Puede tardar de mas en estabilizar o salir en momentos no consistentes.

## 2.6 Conflicto F: transicion de estados `Colgada` / `Vendiendo`
Riesgo: MEDIO.

Detalle:
- `VerificarEstado` cambia estados por bytes B2/80/00/20.
- Si hay secuencia rapida de cambios fisicos y retardo serial, se puede entrar/salir de `Colgada` con `Vendiendo` desalineado.
- El guardado ocurre solo en ruta `Colgada && Vendiendo`.

Efecto:
- Venta real no pasa por `ManejarFinVentaMangueraAsync` (perdida).
- O se procesa dos veces en bordes de transicion (duplicado).

## 2.7 Conflicto G: baseline en memoria se actualiza solo al guardar
Riesgo: MEDIO.

Detalle:
- `NuevoTotalizador` se actualiza solo si se decide guardar.
- Si se bloquea una venta valida por lectura mala, el baseline queda viejo.
- La siguiente venta puede evaluarse contra una referencia incorrecta.

Efecto:
- Cascada de falsos bloqueos o de decisiones erraticas.

## 2.8 Conflicto H: desautorizacion y venta final muy juntas
Riesgo: MEDIO.

Detalle:
- En fin de venta se desautoriza y casi enseguida se consulta ultima venta y totalizador.
- Algunos controladores no consolidan totalizador inmediatamente tras colgado.

Efecto:
- Ventana corta donde `ultimaVenta` ya cambio pero totalizador aun no.
- Es exactamente el patron de "venta valida bloqueada".

## 3) Causa raiz mas probable de las 2 ventas perdidas

Hipotesis principal (alta confianza):
- El nuevo criterio estricto (no guardar si totalizador no cambia) bloqueo ventas reales durante una ventana de no-consolidacion del totalizador, combinada con latencia/parseo serial.

Hipotesis secundaria:
- Asignacion de respuesta serial a solicitud incorrecta por flags globales y fallback header corto.

## 4) Instrumentacion recomendada para confirmar causa real

Agregar logs correlables por intento de fin de venta con un `operationId` por manguera:
1. `operationId`, `surtidor`, `manguera`, timestamp.
2. `ultimaVenta` leida y `NuevaVenta` previa.
3. Cada lectura de totalizador con raw frame normalizado y valor parseado.
4. Resultado de `CambioTotalizador` antes y despues de estabilizar.
5. Decision final: `GUARDAR` o `BLOQUEAR` y motivo.
6. Tiempo entre `Colgada` detectada y lecturas (venta/totalizador).

Con eso se separan 3 casos:
- Totalizador realmente no cambio.
- Totalizador cambio pero lectura llego tarde.
- Parseo incorrecto/mezclado.

## 5) Reglas de negocio que conviene explicitar

Para evitar cambios pendulares en codigo:
1. Definir fuente de verdad primaria: `ultimaVenta`, `totalizador` o combinada.
2. Definir ventana maxima de espera de consolidacion de totalizador (ej: 2-3 s).
3. Definir fallback seguro cuando `ultimaVenta` cambia y totalizador no confirma a tiempo.
4. Definir estrategia anti-duplicado en persistencia (idempotencia por manguera + turno + lectura).

## 6) Conclusiones practicas

1. El ajuste reciente redujo duplicados, pero hizo mas probable perder ventas validas en escenarios de latencia serial.
2. El problema no es solo "if al reves"; hay un problema estructural de correlacion de respuestas y robustez de parseo.
3. Para resolver de fondo se necesita:
   - trazabilidad por intento
   - correlacion robusta de request/respuesta serial
   - regla de negocio explicita para casos donde venta y totalizador no llegan sincronizados.
