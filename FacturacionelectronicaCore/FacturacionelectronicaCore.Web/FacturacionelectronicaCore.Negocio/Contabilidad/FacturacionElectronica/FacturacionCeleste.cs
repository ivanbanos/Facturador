using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    /// <summary>
    /// Celeste electronic invoicing provider.
    /// Base URL: https://celwebhook.celestefacturacion.com
    /// Flow: POST /webhook/api/auth → Bearer token (300 s) → POST /webhook/api/factura
    /// </summary>
    public class FacturacionCeleste : IFacturacionElectronicaFacade
    {
        private readonly Alegra _options;
        private readonly IResolucionRepositorio _resolucionRepositorio;
        private static readonly SemaphoreSlim _semaphore = new(1, 1);

        // Cached token and its expiry moment.
        private static string _cachedToken = null;
        private static DateTime _tokenExpiry = DateTime.MinValue;
        private static readonly object _tokenLock = new object();

        // Rate limiting: 1 req/sec, max 30 req/min (API limit is 50; stay under to avoid rejections).
        private static DateTime _lastApiCall = DateTime.MinValue;
        private static readonly Queue<DateTime> _callWindow = new Queue<DateTime>();
        private const int MaxPerMinute = 30;
        private const int MinMsBetweenCalls = 1100;

        public FacturacionCeleste(IOptions<Alegra> options, IResolucionRepositorio resolucionRepositorio)
        {
            _options = options.Value;
            _resolucionRepositorio = resolucionRepositorio;
        }

        // ─── IFacturacionElectronicaFacade (not used by this provider) ──────────

        public Task ActualizarTercero(Modelo.Tercero tercero, string idFacturacion) =>
            Task.CompletedTask;

        public Task<int> GenerarTercero(Modelo.Tercero tercero) =>
            Task.FromResult(0);

        public Task<ResponseInvoice> GetFacturaElectronica(string id) =>
            Task.FromResult<ResponseInvoice>(null);

        public Task<string> GetFacturaElectronica(string id, Guid estacionGuid) =>
            Task.FromResult<string>(null);

        public Task<ResolucionElectronica> GetResolucionElectronica(string estacion) =>
            Task.FromResult<ResolucionElectronica>(null);

        public Task<Item> GetItem(string name, Alegra options) =>
            Task.FromResult<Item>(null);

        public Task<IEnumerable<TerceroResponse>> GetTerceros(int start) =>
            Task.FromResult(Enumerable.Empty<TerceroResponse>());

        public Task<string> GenerarFacturaElectronica(List<Modelo.OrdenDeDespacho> ordenes, Modelo.Tercero tercero, IEnumerable<Item> items) =>
            Task.FromResult("not_implemented");

        public Task<string> GenerarFacturaElectronica(List<Modelo.Factura> facturas, Modelo.Tercero tercero, IEnumerable<Item> items) =>
            Task.FromResult("not_implemented");

        public Task<string> GenerarFacturaElectronica(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, Guid estacionGuid) =>
            Task.FromResult("not_implemented");

        public Task<string> getJson(Modelo.OrdenDeDespacho ordenDeDespachoEntity, Guid estacio) =>
            Task.FromResult("not_implemented");

        public Task<string> getJsonCanastilla(Modelo.FacturaCanastilla facturaCanastilla, Guid estacio) =>
            Task.FromResult("not_implemented");

        public Task<string> ReenviarFactura(FacturacionelectronicaCore.Repositorio.Entities.OrdenDeDespacho orden, Guid estacion) =>
            Task.FromResult("not_implemented");

        // ─── Main entry points ────────────────────────────────────────────────

        public async Task<string> GenerarFacturaElectronica(Modelo.Factura factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            await _semaphore.WaitAsync();
            object payload = null;
            try
            {
                payload = BuildPayload(factura, tercero);
                return await EnviarFacturaCeleste(payload, estacionGuid.ToString());
            }
            catch (Exception ex)
            {
                var payloadStr = payload != null ? JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) : "(no payload)";
                var msg = ex.Message?.Length > 200 ? ex.Message.Substring(0, 200) : ex.Message;
                return $"error:{msg} | payload:{payloadStr}";
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, Guid estacionGuid)
        {
            await _semaphore.WaitAsync();
            object payload = null;
            try
            {
                payload = BuildPayload(orden, tercero);
                return await EnviarFacturaCeleste(payload, estacionGuid.ToString());
            }
            catch (Exception ex)
            {
                var payloadStr = payload != null ? JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }) : "(no payload)";
                var msg = ex.Message?.Length > 200 ? ex.Message.Substring(0, 200) : ex.Message;
                return $"error:{msg} : payload:{payloadStr}";
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // ─── Token management ─────────────────────────────────────────────────

        private async Task<string> ObtenerToken()
        {
            lock (_tokenLock)
            {
                if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
                    return _cachedToken;
            }

            var baseUrl = _options.Url.TrimEnd('/');
            var authBody = new Dictionary<string, string>
            {
                ["client-id"] = _options.Usuario,
                ["user-token"] = _options.Contrasena,
                ["nit"] = _options.Nit
            };

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var content = new StringContent(JsonConvert.SerializeObject(authBody), null, "application/json");
            var response = await client.PostAsync($"{baseUrl}/webhook/api/auth", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var tokenResponse = JsonConvert.DeserializeObject<RespuestaCelesteToken>(responseBody);

            lock (_tokenLock)
            {
                _cachedToken = tokenResponse.data.token;
                // Token valid 24 h; refresh 5 min before expiry.
                _tokenExpiry = DateTime.UtcNow.AddSeconds(86400 - 300);
            }

            return _cachedToken;
        }

        // ─── Rate limiting ────────────────────────────────────────────────────

        private static async Task WaitForRateLimit()
        {
            while (true)
            {
                int delayMs;
                lock (_tokenLock)
                {
                    var now = DateTime.UtcNow;

                    // Remove entries outside the 60-second window.
                    while (_callWindow.Count > 0 && (now - _callWindow.Peek()).TotalMilliseconds >= 60_000)
                        _callWindow.Dequeue();

                    // Per-minute limit.
                    if (_callWindow.Count >= MaxPerMinute)
                    {
                        var waitUntil = _callWindow.Peek().AddMilliseconds(60_000);
                        delayMs = Math.Max(100, (int)(waitUntil - now).TotalMilliseconds);
                    }
                    // Per-second limit.
                    else if ((now - _lastApiCall).TotalMilliseconds < MinMsBetweenCalls)
                    {
                        delayMs = (int)(MinMsBetweenCalls - (now - _lastApiCall).TotalMilliseconds);
                    }
                    else
                    {
                        _lastApiCall = now;
                        _callWindow.Enqueue(now);
                        return;
                    }
                }

                await Task.Delay(delayMs);
            }
        }

        // ─── HTTP call ────────────────────────────────────────────────────────

        private async Task<string> EnviarFacturaCeleste(object payload, string estacionGuid)
        {
            var baseUrl = _options.Url.TrimEnd('/');
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid);
            var prefijo = resolucion?.prefijo ?? "";
            var contentString = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            // Retry once if Celeste returns a session/token error (code 48).
            for (int attempt = 0; attempt < 2; attempt++)
            {
                var token = await ObtenerToken();

                await WaitForRateLimit();

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("X-Celeste-Nit", _options.Nit);
                client.DefaultRequestHeaders.Add("X-Celeste-Client-Id", _options.Usuario);

                var content = new StringContent(contentString, null, "application/json");
                var response = await client.PostAsync($"{baseUrl}/webhook/api/factura_venta", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[Celeste] OUT {contentString}");
                Console.WriteLine($"[Celeste] IN  {responseBody}");

                // Detect Celeste session error (code 48) regardless of HTTP status.
                if (IsCelesteSessionError(responseBody) || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    lock (_tokenLock) { _cachedToken = null; }
                    if (attempt == 0)
                    {
                        Console.WriteLine("[Celeste] Session/token error detected, invalidating cache and retrying...");
                        await Task.Delay(1000);
                        continue;
                    }
                    return "error:" + responseBody + contentString;
                }

                if (response.IsSuccessStatusCode)
                {
                    var respuesta = JsonConvert.DeserializeObject<RespuestaCelesteFactura>(responseBody);

                    var numeroPost = respuesta?.data?.numero_factura ?? "";
                    var uuid = respuesta?.data?.gen_uuid ?? "";
                    var fechaFactura = respuesta?.data?.fecha_factura ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

                    // Update resolution counter from POST response (best-effort).
                    if (int.TryParse(numeroPost, out var numFactura))
                    {
                        await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(
                            estacionGuid,
                            numFactura + 1);
                    }

                    // Celeste processes invoices asynchronously — poll the consultation endpoint
                    // until the invoice is approved (estado = "aprobada" or "0") or we exhaust retries.
                    var (consecutivo, cufe, consultaBody) = await ConsultarFacturaCeleste(uuid, fechaFactura, token);

                    // Prefer consultation's numero_factura when available (it's the authoritative DIAN consecutive).
                    var numero = !string.IsNullOrEmpty(consecutivo) ? consecutivo : numeroPost;

                    return "OK:" + prefijo + numero + ":" + cufe + ":" + uuid + ":" + consultaBody;
                }

                return "error:" + responseBody + contentString;
            }

            return "error:Max retries exceeded";
        }

        /// <summary>
        /// Polls the Celeste consultation endpoint after a successful POST until the invoice is
        /// approved or retries are exhausted. Returns (numero_factura, cufe, rawResponseBody).
        /// </summary>
        private async Task<(string numero, string cufe, string rawBody)> ConsultarFacturaCeleste(string uuid, string fechaFactura, string token)
        {
            if (string.IsNullOrWhiteSpace(uuid))
                return ("", "", "");

            var baseUrl = _options.Url.TrimEnd('/');
            // fecha must be yyyymmdd
            if (!DateTime.TryParse(fechaFactura, out var dt))
                dt = DateTime.UtcNow.AddHours(_options.ServerTimeOffsetHoursSearch ?? 0);
            var fecha = dt.ToString("yyyyMMdd");

            const int maxRetries = 6;
            const int delayMs = 5000; // 5 s between polls

            for (int i = 0; i < maxRetries; i++)
            {
                if (i > 0)
                    await Task.Delay(delayMs);

                try
                {
                    await WaitForRateLimit();

                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    client.DefaultRequestHeaders.Add("X-Celeste-Nit", _options.Nit);
                    client.DefaultRequestHeaders.Add("X-Celeste-Client-Id", _options.Usuario);

                    var url = $"{baseUrl}/webhook/api/factura_venta?uuid={uuid}&date={fecha}";
                    var response = await client.GetAsync(url);
                    var body = await response.Content.ReadAsStringAsync();

                    Console.WriteLine($"[Celeste Consulta] intento {i + 1} uuid={uuid} → {response.StatusCode} {body}");

                    if (!response.IsSuccessStatusCode)
                        continue;

                    var consulta = JsonConvert.DeserializeObject<RespuestaCelesteConsulta>(body);
                    var estado = consulta?.data?.estado ?? "";

                    // estado "0" or "aprobada" = approved
                    var aprobada = estado == "0"
                        || estado.Equals("aprobada", StringComparison.OrdinalIgnoreCase);

                    // estado "2" or "rechazada" = rejected — stop polling immediately
                    var rechazada = estado == "2"
                        || estado.Equals("rechazada", StringComparison.OrdinalIgnoreCase);

                    if (aprobada || rechazada)
                    {
                        var numero = consulta?.data?.numero_factura?.ToString() ?? "";
                        var cufe = consulta?.data?.cufe ?? "";
                        Console.WriteLine($"[Celeste Consulta] uuid={uuid} estado={estado} numero={numero} cufe={cufe}");
                        return (numero, cufe, body);
                    }

                    // estado "1" / "pendiente" → keep polling
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Celeste Consulta] Error intento {i + 1} uuid={uuid}: {ex.Message}");
                }
            }

            Console.WriteLine($"[Celeste Consulta] uuid={uuid} no aprobada tras {maxRetries} intentos, se continúa con datos del POST.");
            return ("", "", "");
        }

        private static bool IsCelesteSessionError(string responseBody)
        {
            if (string.IsNullOrEmpty(responseBody)) return false;
            try
            {
                var obj = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(responseBody);
                var code = obj?["code"]?.ToObject<int?>();
                return code == 48;
            }
            catch
            {
                return responseBody.Contains("Problemas iniciando sesión", StringComparison.OrdinalIgnoreCase)
                    || responseBody.Contains("vuelva a solicitar token", StringComparison.OrdinalIgnoreCase);
            }
        }

        // ─── Payload builders ─────────────────────────────────────────────────

        private object BuildPayload(Modelo.Factura factura, Modelo.Tercero tercero)
        {
            var cantidadRedondeada = Math.Round((double)factura.Cantidad, 2);
            var descuento = Math.Round(factura.Descuento, 2);
            var precioUnitario = cantidadRedondeada > 0
                ? Math.Round(((double)factura.Total + (double)descuento) / cantidadRedondeada, 2)
                : 0.0;
            var subtotal = Math.Round(cantidadRedondeada * precioUnitario - (double)descuento, 2);

            var (medioCodigo, medioNombre) = GetMedioPago(factura.FormaDePago);
            var (nombre, apellido) = SplitNombre(tercero);
            var tipoId = GetTipoIdentificacionDian(tercero.DescripcionTipoIdentificacion);
            var fechaLocal = DateTime.UtcNow.AddHours(_options.ServerTimeOffsetHoursSearch ?? 0);

            return new
            {
                documento = new
                {
                    datos_generales_documento = new
                    {
                        nit_tercero_emisor = _options.Nit,
                        tipo_moneda = "COP",
                        hora_generacion = fechaLocal.ToString("HH:mm:ss"),
                        fecha_generacion = fechaLocal.ToString("yyyy-MM-dd"),
                        fecha_vencimiento = fechaLocal.ToString("yyyy-MM-dd"),
                        gen_uuid = Guid.NewGuid().ToString(),
                        observaciones = $"Placa: {factura.Placa}, Kilometraje: {factura.Kilometraje}",
                        valor_total = ((double)factura.Total).ToString("0.##", CultureInfo.InvariantCulture),
                        contingencia = "false"
                    },
                    tercero_receptor = BuildTerceroReceptor(tercero, nombre, apellido, tipoId),
                    descripcion_productos = new[]
                    {
                        BuildProducto(
                            factura.Combustible,
                            cantidadRedondeada,
                            precioUnitario,
                            subtotal,
                            descuento,
                            descuento > 0 ? factura.Total + descuento : 0m)
                    },
                    formas_pago = new[]
                    {
                        new
                        {
                            forma_pago_dian = "1",
                            medio_pago_dian_codigo = medioCodigo,
                            valor = ((double)factura.Total).ToString("0.##", CultureInfo.InvariantCulture)
                        }
                    }
                }
            };
        }

        private object BuildPayload(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero)
        {
            var cantidadRedondeada = Math.Round((double)orden.Cantidad, 2);
            var descuento = Math.Round(orden.Descuento, 2);
            var precioUnitario = cantidadRedondeada > 0
                ? Math.Round(((double)orden.Total + (double)descuento) / cantidadRedondeada, 2)
                : 0.0;
            var subtotal = Math.Round(cantidadRedondeada * precioUnitario - (double)descuento, 2);

            var (nombre, apellido) = SplitNombre(tercero);
            var tipoId = GetTipoIdentificacionDian(tercero.DescripcionTipoIdentificacion);
            var formasPago = BuildFormasPago(orden);
            var fechaLocal = DateTime.UtcNow.AddHours(_options.ServerTimeOffsetHoursSearch ?? 0);

            return new
            {
                documento = new
                {
                    datos_generales_documento = new
                    {
                        nit_tercero_emisor = _options.Nit,
                        tipo_moneda = "COP",
                        hora_generacion = fechaLocal.ToString("HH:mm:ss"),
                        fecha_generacion = fechaLocal.ToString("yyyy-MM-dd"),
                        fecha_vencimiento = fechaLocal.ToString("yyyy-MM-dd"),
                        gen_uuid = Guid.NewGuid().ToString(),
                        observaciones = $"Placa: {orden.Placa}, Kilometraje: {orden.Kilometraje}, Nro Transaccion: {orden.numeroTransaccion}",
                        valor_total = orden.Total.ToString("0.##", CultureInfo.InvariantCulture),
                        contingencia = "false"
                    },
                    tercero_receptor = BuildTerceroReceptor(tercero, nombre, apellido, tipoId),
                    descripcion_productos = new[]
                    {
                        BuildProducto(
                            orden.Combustible,
                            cantidadRedondeada,
                            precioUnitario,
                            subtotal,
                            descuento,
                            descuento > 0 ? (decimal)orden.Total + descuento : 0m)
                    },
                    formas_pago = formasPago
                }
            };
        }

        // ─── Helpers ──────────────────────────────────────────────────────────

        private object[] BuildFormasPago(Modelo.OrdenDeDespacho orden)
        {
            if (orden.Total2.HasValue && orden.Total2.Value > 0 &&
                !string.IsNullOrWhiteSpace(orden.FormaDePago2))
            {
                var (codigo1, _) = GetMedioPago(orden.FormaDePago);
                var (codigo2, _) = GetMedioPago(orden.FormaDePago2);
                return new object[]
                {
                    new { forma_pago_dian = "1", medio_pago_dian_codigo = codigo1,
                          valor = ((double)(orden.Total1 ?? (decimal)orden.Total)).ToString("0.##", CultureInfo.InvariantCulture) },
                    new { forma_pago_dian = "1", medio_pago_dian_codigo = codigo2,
                          valor = ((double)orden.Total2.Value).ToString("0.##", CultureInfo.InvariantCulture) }
                };
            }

            var (mc, _) = GetMedioPago(orden.FormaDePago);
            return new object[]
            {
                new { forma_pago_dian = "1", medio_pago_dian_codigo = mc,
                      valor = orden.Total.ToString("0.##", CultureInfo.InvariantCulture) }
            };
        }

        private object BuildProducto(string combustible, double cantidad, double precioUnitario,
            double subtotal, decimal descuento, decimal totalConDescuento)
        {
            var codigo = GetCodigoCombustible(combustible);

            // Only include the descuento block when the rounded discount is meaningful (>= 0.01).
            // Sending a descuento block with valor_descuento = "0" causes a Celeste validation error.
            var descuentoRedondeado = Math.Round(descuento, 2);
            if (descuentoRedondeado > 0 && totalConDescuento > 0)
            {
                var porcentajeDescuento = Math.Round((double)(descuentoRedondeado / totalConDescuento * 100), 2).ToString("0.##", CultureInfo.InvariantCulture);
                return new
                {
                    producto_codigo = codigo,
                    unidad_medida = "GL",
                    cantidad = cantidad.ToString("0.##", CultureInfo.InvariantCulture),
                    valor_unitario_final = precioUnitario.ToString("0.##", CultureInfo.InvariantCulture),
                    subtotal = subtotal.ToString("0.##", CultureInfo.InvariantCulture),
                    descripcion = combustible,
                    producto_gratuito = "false",
                    descuento = new
                    {
                        porcentaje_descuento = porcentajeDescuento,
                        valor_descuento = ((double)descuentoRedondeado).ToString("0.##", CultureInfo.InvariantCulture),
                        valor_base_descuento = ((double)totalConDescuento).ToString("0.##", CultureInfo.InvariantCulture)
                    }
                };
            }

            return new
            {
                producto_codigo = codigo,
                unidad_medida = "GL",
                cantidad = cantidad.ToString("0.##", CultureInfo.InvariantCulture),
                valor_unitario_final = precioUnitario.ToString("0.##", CultureInfo.InvariantCulture),
                subtotal = subtotal.ToString("0.##", CultureInfo.InvariantCulture),
                descripcion = combustible,
                producto_gratuito = "false"
            };
        }

        private object BuildTerceroReceptor(Modelo.Tercero tercero, string nombre, string apellido, int tipoId)
        {
            return new
            {
                nit = tercero.Identificacion,
                juridico = (tipoId == 31) ? "true" : "false",
                tipo_identificacion_dian = tipoId.ToString(),
                nombre,
                nombre2 = "",
                apellido,
                apellido2 = "",
                razon_social = tercero.Nombre,
                nombre_empresa = "",
                email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.ToLower().Contains("no informado")
                    ? _options.Correo
                    : tercero.Correo,
                telefono = string.IsNullOrEmpty(tercero.Telefono) ? "" : tercero.Telefono,
                celular = string.IsNullOrEmpty(tercero.Celular) ? "0" : tercero.Celular,
                direccion = string.IsNullOrEmpty(tercero.Direccion) ? "No informado" : tercero.Direccion,
                codigo_pais = "CO",
                codigo_departamento = ResolveCodigo(_options.Department, _departamentoCodes, "11"),
                codigo_municipio = ResolveCodigo(_options.City, _municipioCodes, "11001")
            };
        }

        /// <summary>
        /// Returns the value if already numeric; otherwise looks it up in the provided map;
        /// falls back to <paramref name="defaultCode"/> if not found.
        /// </summary>
        private static string ResolveCodigo(string value, Dictionary<string, string> map, string defaultCode)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultCode;

            // Already numeric → use as-is
            if (value.Trim().All(char.IsDigit))
                return value.Trim();

            var key = value.Trim().ToUpperInvariant()
                .Replace("Á", "A").Replace("É", "E").Replace("Í", "I")
                .Replace("Ó", "O").Replace("Ú", "U").Replace("Ü", "U");

            return map.TryGetValue(key, out var code) ? code : defaultCode;
        }

        // DIAN DIVIPOLA: department name (uppercase, no accents) → 2-digit code
        private static readonly Dictionary<string, string> _departamentoCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["BOGOTA"] = "11", ["BOGOTÁ"] = "11", ["BOGOTA DC"] = "11", ["BOGOTÁ D.C."] = "11",
            ["ANTIOQUIA"] = "05",
            ["ATLANTICO"] = "08", ["ATLÁNTICO"] = "08",
            ["BOLIVAR"] = "13", ["BOLÍVAR"] = "13",
            ["BOYACA"] = "15", ["BOYACÁ"] = "15",
            ["CALDAS"] = "17",
            ["CAQUETA"] = "18", ["CAQUETÁ"] = "18",
            ["CAUCA"] = "19",
            ["CESAR"] = "20",
            ["CHOCO"] = "27", ["CHOCÓ"] = "27",
            ["CORDOBA"] = "23", ["CÓRDOBA"] = "23",
            ["CUNDINAMARCA"] = "25",
            ["GUAJIRA"] = "44", ["LA GUAJIRA"] = "44",
            ["HUILA"] = "41",
            ["MAGDALENA"] = "47",
            ["META"] = "50",
            ["NARINO"] = "52", ["NARIÑO"] = "52",
            ["NORTE DE SANTANDER"] = "54",
            ["PUTUMAYO"] = "86",
            ["QUINDIO"] = "63", ["QUINDÍO"] = "63",
            ["RISARALDA"] = "66",
            ["SAN ANDRES"] = "88", ["SAN ANDRÉS"] = "88",
            ["SANTANDER"] = "68",
            ["SUCRE"] = "70",
            ["TOLIMA"] = "73",
            ["VALLE DEL CAUCA"] = "76", ["VALLE"] = "76",
            ["VAUPES"] = "97", ["VAUPÉS"] = "97",
            ["VICHADA"] = "99",
            ["ARAUCA"] = "81",
            ["CASANARE"] = "85",
            ["AMAZONAS"] = "91",
            ["GUAINIA"] = "94", ["GUAINÍA"] = "94",
            ["GUAVIARE"] = "95",
        };

        // DIAN DIVIPOLA: municipality name (uppercase, no accents) → 5-digit code
        private static readonly Dictionary<string, string> _municipioCodes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["BOGOTA"] = "11001", ["BOGOTÁ"] = "11001", ["BOGOTA DC"] = "11001", ["BOGOTÁ D.C."] = "11001",
            ["MEDELLIN"] = "05001", ["MEDELLÍN"] = "05001",
            ["CALI"] = "76001",
            ["BARRANQUILLA"] = "08001",
            ["CARTAGENA"] = "13001",
            ["CUCUTA"] = "54001", ["CÚCUTA"] = "54001",
            ["BUCARAMANGA"] = "68001",
            ["PEREIRA"] = "66001",
            ["MANIZALES"] = "17001",
            ["IBAGUE"] = "73001", ["IBAGUÉ"] = "73001",
            ["SANTA MARTA"] = "47001",
            ["VILLAVICENCIO"] = "50001",
            ["ARMENIA"] = "63001",
            ["NEIVA"] = "41001",
            ["PASTO"] = "52001",
            ["MONTERIA"] = "23001", ["MONTERÍA"] = "23001",
            ["SINCELEJO"] = "70001",
            ["VALLEDUPAR"] = "20001",
            ["POPAYAN"] = "19001", ["POPAYÁN"] = "19001",
            ["TUNJA"] = "15001",
            ["FLORENCIA"] = "18001",
            ["QUIBDO"] = "27001", ["QUIBDÓ"] = "27001",
            ["RIOHACHA"] = "44001",
            ["SAN ANDRES"] = "88001", ["SAN ANDRÉS"] = "88001",
            ["YOPAL"] = "85001",
            ["MOCOA"] = "86001",
            ["LETICIA"] = "91001",
            ["INIRIDA"] = "94001", ["INÍRIDA"] = "94001",
            ["SAN JOSE DEL GUAVIARE"] = "95001",
            ["MITU"] = "97001", ["MITÚ"] = "97001",
            ["PUERTO CARRENO"] = "99001", ["PUERTO CARREÑO"] = "99001",
            ["ARAUCA"] = "81001",
        };

        private static (string nombre, string apellido) SplitNombre(Modelo.Tercero tercero)
        {
            if (!string.IsNullOrEmpty(tercero.Apellidos) && !tercero.Apellidos.ToLower().Contains("no informado"))
                return (tercero.Nombre.Trim(), tercero.Apellidos.Trim());

            var nombreCompleto = tercero.Nombre.Trim();
            if (nombreCompleto.Contains(' '))
            {
                var ultimo = nombreCompleto.LastIndexOf(' ');
                return (nombreCompleto.Substring(0, ultimo), nombreCompleto.Substring(ultimo + 1));
            }
            return (nombreCompleto, "no informado");
        }

        private string GetCodigoCombustible(string combustible)
        {
            var lower = combustible.ToLower();
            if (lower.Contains("acpm") || lower.Contains("die")) return _options.Acpm ?? "ACPM";
            if (lower.Contains("corri")) return _options.Corriente ?? "CORRIENTE";
            return _options.Gas ?? "GAS";
        }

        /// <summary>
        /// Maps FormaDePago description to Celeste DIAN payment-means codes.
        /// Returns (medio_pago_dian_codigo, medio_pago_dian_nombre).
        /// </summary>
        private static (string codigo, string nombre) GetMedioPago(string formaDePago)
        {
            if (string.IsNullOrWhiteSpace(formaDePago)) return ("11", "Efectivo");
            var lower = formaDePago.ToLower().Trim();

            if (lower.Contains("tarjeta") && lower.Contains("dé")) return ("49", "Tarjeta Debito");
            if (lower.Contains("tarjeta") && lower.Contains("cré")) return ("48", "Tarjeta Credito");
            if (lower.Contains("transferencia") || lower.Contains("nequi") || lower.Contains("datafono") || lower.Contains("datafóno"))
                return ("47", "Transferencia");
            if (lower.Contains("cheque")) return ("20", "Cheque");
            if (lower.Contains("convenio") || lower.Contains("credito")) return ("ZZZ", "Otro");
            return ("11", "Efectivo");
        }

        private static int GetTipoIdentificacionDian(string descripcionTipoIdentificacion)
        {
            return descripcionTipoIdentificacion switch
            {
                "Nit" => 31,
                "Pasaporte" => 41,
                _ => 13  // Cédula de ciudadanía
            };
        }
    }
}
