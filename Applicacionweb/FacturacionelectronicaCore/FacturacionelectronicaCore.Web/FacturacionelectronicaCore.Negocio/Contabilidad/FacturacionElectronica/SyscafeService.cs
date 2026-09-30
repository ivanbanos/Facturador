using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class SyscafeService : ISyscafeService
    {
        private readonly SyscafeOptions _options;
        private readonly ILogger<SyscafeService> _logger;
        private readonly HttpClient _httpClient;

        public SyscafeService(IOptions<SyscafeOptions> options, ILogger<SyscafeService> logger)
        {
            _options = options.Value;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task<bool> EnviarFacturaCanastilla(Modelo.FacturaCanastilla factura)
        {
            if (!_options.Habilitado || string.IsNullOrEmpty(_options.Url))
                return false;

            try
            {
                var documento = MapearDocumento(factura);
                var documentos = new List<SyscafeDocument> { documento };
                var json = JsonConvert.SerializeObject(documentos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.Token);
                var response = await _httpClient.PostAsync(_options.Url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Syscafe EnviarFacturaCanastilla error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> EnviarFacturaCombustible(Modelo.OrdenDeDespacho orden)
        {
            if (!_options.Habilitado || string.IsNullOrEmpty(_options.Url))
                return false;

            try
            {
                var documento = MapearDocumentoCombustible(orden);
                var documentos = new List<SyscafeDocument> { documento };
                var json = JsonConvert.SerializeObject(documentos);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.Token);
                var response = await _httpClient.PostAsync(_options.Url, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Syscafe EnviarFacturaCombustible error: {ex.Message}");
                return false;
            }
        }

        private SyscafeDocument MapearDocumentoCombustible(Modelo.OrdenDeDespacho orden)
{
    var cliente = MapearCliente(orden.Tercero);
    var items = new List<SyscafeItem>();

    // Para combustible, creamos un item con los datos del combustible
    if (!string.IsNullOrEmpty(orden.Combustible) && orden.Precio > 0)
    {
        items.Add(new SyscafeItem
        {
            referencia = orden.Combustible ?? "COMB",
                    cant = (decimal)(orden.Cantidad > 0 ? orden.Cantidad : 1),
                    precio = (decimal)orden.Precio,
                    vrunit = (decimal)orden.Precio,
                    vrtotal = (decimal)(orden.Total > 0 ? orden.Total : orden.Precio * (orden.Cantidad > 0 ? orden.Cantidad : 1)),
                    piva = 0,
                    vriva = 0
                });
            }

            var total = items.Sum(i => i.vrtotal);
            return new SyscafeDocument
            {
                tipo = _options.TipoComprobante,
                numero = "",
                noext = orden.IdVentaLocal.ToString(),
                fecha = orden.Fecha.ToString("yyyy-MM-dd"),
                fechaven = orden.Fecha.ToString("yyyy-MM-dd"),
                fpago = orden.FormaDePago,
                nit = orden.Identificacion,
                items = items,
                cliente = cliente,
                fpagos = new List<SyscafeFpago>
                {
                    new SyscafeFpago { vrfpago = total }
                }
            };
        }

        private SyscafeDocument MapearDocumento(Modelo.FacturaCanastilla factura)
        {
            var tercero = factura.terceroId;
            var descripcionFormaPago = factura.codigoFormaPago?.Descripcion ?? string.Empty;

            var items = (factura.canastillas ?? Enumerable.Empty<Modelo.CanastillaFactura>())
                .Select(c => new SyscafeItem
                {
                    referencia = c.Canastilla?.guid.ToString() ?? string.Empty,
                    servicio = string.Empty,
                    cant = (decimal)c.cantidad,
                    precio = (decimal)c.precio,
                    vrunit = (decimal)c.precio,
                    vrtotal = (decimal)c.total,
                    piva = c.Canastilla != null ? c.Canastilla.iva : 0,
                    vriva = (decimal)c.iva,
                    vrico = 0
                })
                .ToList();

            return new SyscafeDocument
            {
                tipo = _options.TipoComprobante,
                numero = string.Empty,
                noext = factura.FacturasCanastillaId.ToString(),
                fecha = factura.fecha.ToString("yyyy-MM-dd"),
                fechaven = factura.fecha.ToString("yyyy-MM-dd"),
                fpago = factura.codigoFormaPago?.Id.ToString() ?? string.Empty,
                nit = tercero?.Identificacion?.Trim() ?? string.Empty,
                detalle = $"Factura canastilla {factura.FacturasCanastillaId}",
                obs = descripcionFormaPago,
                cliente = MapearCliente(tercero),
                items = items,
                fpagos = new List<SyscafeFpago>
                {
                    new SyscafeFpago
                    {
                        fpago = factura.codigoFormaPago?.Id.ToString() ?? string.Empty,
                        vrfpago = (decimal)factura.total,
                        ctafpago = string.Empty
                    }
                }
            };
        }

        private static SyscafeCliente MapearCliente(Modelo.Tercero tercero)
        {
            if (tercero == null) return new SyscafeCliente();

            var nombreCompleto = (tercero.Nombre ?? string.Empty).Trim();
            var partes = nombreCompleto.Split(' ');
            var nom1 = partes.Length > 0 ? partes[0] : nombreCompleto;
            var nom2 = partes.Length > 1 ? partes[1] : string.Empty;

            var apellidos = (tercero.Apellidos ?? string.Empty).Trim();
            var partesApe = apellidos.Split(' ');
            var ape1 = partesApe.Length > 0 ? partesApe[0] : apellidos;
            var ape2 = partesApe.Length > 1 ? partesApe[1] : string.Empty;

            var claseid = ObtenerClaseId(tercero.DescripcionTipoIdentificacion);

            return new SyscafeCliente
            {
                nit = tercero.Identificacion?.Trim() ?? string.Empty,
                dv = "0",
                claseid = claseid,
                nom1 = nom1,
                nom2 = nom2,
                ape1 = ape1,
                ape2 = ape2,
                nombrec = nombreCompleto,
                dir = tercero.Direccion ?? string.Empty,
                tel = tercero.Telefono ?? tercero.Celular ?? string.Empty,
                email = tercero.Correo ?? string.Empty
            };
        }

        private static string ObtenerClaseId(string descripcionTipo)
        {
            if (string.IsNullOrWhiteSpace(descripcionTipo)) return "C";
            var tipo = descripcionTipo.ToLower();
            if (tipo.Contains("nit")) return "N";
            if (tipo.Contains("pasaporte") || tipo.Contains("pasap")) return "P";
            if (tipo.Contains("extranjera") || tipo.Contains("extranjero")) return "E";
            if (tipo.Contains("tarjeta") && tipo.Contains("identidad")) return "T";
            return "C"; // Cédula de ciudadanía por defecto
        }
    }
}
