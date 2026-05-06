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

        // ─── HTTP call ────────────────────────────────────────────────────────

        private async Task<string> EnviarFacturaCeleste(object payload, string estacionGuid)
        {
            var baseUrl = _options.Url.TrimEnd('/');
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid);
            var prefijo = resolucion?.prefijo ?? "";
            var token = await ObtenerToken();
            var contentString = JsonConvert.SerializeObject(payload, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("X-Celeste-Nit", _options.Nit);
            client.DefaultRequestHeaders.Add("X-Celeste-Client-Id", _options.Usuario);

            var content = new StringContent(contentString, null, "application/json");
            var response = await client.PostAsync($"{baseUrl}/webhook/api/factura_venta", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[Celeste] OUT {contentString}");
            Console.WriteLine($"[Celeste] IN  {responseBody}");

            if (response.IsSuccessStatusCode)
            {
                var respuesta = JsonConvert.DeserializeObject<RespuestaCelesteFactura>(responseBody);

                // On any 200 OK, update the resolution counter if we can parse the invoice number.
                if (respuesta?.data != null &&
                    int.TryParse(respuesta.data.numero_factura, out var numFactura))
                {
                    await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(
                        estacionGuid,
                        numFactura + 1);
                }

                var numero = respuesta?.data?.numero_factura ?? "";
                var uuid = respuesta?.data?.gen_uuid ?? "";
                return "OK:" + prefijo + numero + ":" + uuid;
            }

            // If 401 the token may have just expired — invalidate cache so next call re-authenticates.
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                lock (_tokenLock) { _cachedToken = null; }
            }

            return "error:" + responseBody + contentString;
        }

        // ─── Payload builders ─────────────────────────────────────────────────

        private object BuildPayload(Modelo.Factura factura, Modelo.Tercero tercero)
        {
            var cantidadRedondeada = Math.Round((double)factura.Cantidad, 2);
            var precioUnitario = cantidadRedondeada > 0
                ? Math.Round(((double)factura.Total + (double)factura.Descuento) / cantidadRedondeada, 2)
                : 0.0;
            var subtotal = Math.Round(cantidadRedondeada * precioUnitario - (double)factura.Descuento, 2);

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
                            factura.Descuento,
                            factura.Total + factura.Descuento)
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
            var precioUnitario = cantidadRedondeada > 0
                ? Math.Round(((double)orden.Total + (double)orden.Descuento) / cantidadRedondeada, 2)
                : 0.0;
            var subtotal = Math.Round(cantidadRedondeada * precioUnitario - (double)orden.Descuento, 2);

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
                            orden.Descuento,
                            (decimal)orden.Total + orden.Descuento)
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

            if (descuento > 0 && totalConDescuento > 0)
            {
                var porcentajeDescuento = Math.Round((double)(descuento / totalConDescuento * 100), 2).ToString("0.##", CultureInfo.InvariantCulture);
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
                        valor_descuento = ((double)descuento).ToString("0.##", CultureInfo.InvariantCulture),
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
                codigo_departamento = _options.Department ?? "11",
                codigo_municipio = _options.City ?? "001"
            };
        }

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
            if (string.IsNullOrWhiteSpace(formaDePago)) return ("10", "Efectivo");
            var lower = formaDePago.ToLower().Trim();

            if (lower.Contains("tarjeta") && lower.Contains("dé")) return ("49", "Tarjeta Debito");
            if (lower.Contains("tarjeta") && lower.Contains("cré")) return ("48", "Tarjeta Credito");
            if (lower.Contains("transferencia") || lower.Contains("nequi") || lower.Contains("datafono") || lower.Contains("datafóno"))
                return ("47", "Transferencia");
            if (lower.Contains("cheque")) return ("20", "Cheque");
            if (lower.Contains("convenio") || lower.Contains("credito")) return ("ZZZ", "Otro");
            return ("10", "Efectivo");
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
