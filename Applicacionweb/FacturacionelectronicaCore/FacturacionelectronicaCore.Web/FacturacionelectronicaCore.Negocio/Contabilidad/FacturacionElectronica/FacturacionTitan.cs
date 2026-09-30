using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class FacturacionTitan : IFacturacionElectronicaFacade
    {
        private readonly Alegra alegraOptions;
        private readonly ContactsHandler contactsHandler;
        private readonly InvoiceHandler invoiceHandler;
        private readonly ItemHandler itemHandler;
        private readonly ResolucionesHandler resolucionesHandler;
        private readonly ResolucionNumber _resolucionNumber;
        private readonly IResolucionRepositorio _resolucionRepositorio;

        public FacturacionTitan(IOptions<Alegra> alegra, ResolucionNumber resolucionNumber, IResolucionRepositorio resolucionRepositorio)
        {
            alegraOptions = alegra.Value;

            _resolucionNumber = resolucionNumber;
            
           _resolucionRepositorio = resolucionRepositorio;
        }

        public async Task ActualizarTercero(Modelo.Tercero tercero, string idFacturacion)
        {
            await contactsHandler.ActualizarCliente(idFacturacion, tercero.ConvertirAContact(), alegraOptions);
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.Factura factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            try
            {

                var invoice = await GetFacturaTitan(factura, tercero);
                Console.WriteLine(JsonConvert.SerializeObject(invoice));
                while (true)
                {

                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                        var path = $"{alegraOptions.Url}";
                        var form = new MultipartFormDataContent
                        {
                            { new StringContent(JsonConvert.SerializeObject(invoice)), "json" },
                            { new StringContent(alegraOptions.Token), "token" }
                        };
                        var response = client.PostAsync(path, form).Result;
                        string responseBody = await response.Content.ReadAsStringAsync();
                        try
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        catch (Exception)
                        {
                            try
                            {
                                var respuestaError = JsonConvert.DeserializeObject<ResponseTitan>(responseBody);
                                if (respuestaError.Mensaje.Any(x=>x.msg.ToLower().Contains("no")))
                                {

                                    throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                }
                                else
                                {
                                    throw new AlegraException(responseBody+ JsonConvert.SerializeObject(invoice));

                                }
                            }
                            catch (Exception ex)
                            {
                                throw new AlegraException(responseBody+ex.Message+ JsonConvert.SerializeObject(invoice));
                            }
                        }
                            Console.WriteLine(responseBody);
                            var respuesta = JsonConvert.DeserializeObject<ResponseTitan>(responseBody);
                            Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                            Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                            await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), Int32.Parse(invoice.Comprobante.Numero) + 1);
                            return "Enviada" + ":" + invoice.Comprobante.Numero + ":" + invoice.Comprobante.Numero;
                        
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        public async Task<FacturaTitan> GetFacturaTitan(Modelo.Factura factura, Modelo.Tercero tercero)
        {
            var numero = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(factura.IdEstacion.ToString());

            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero.Nombre.Trim();
            if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.Contains("no informado"))
            {
                if (nombreCompleto.Split(' ').Count() > 1)
                {
                    nombre = nombreCompleto.Substring(0, nombreCompleto.LastIndexOf(" "));
                    apellido = nombreCompleto.Split(' ').Last();
                }
                else
                {
                    nombre = nombreCompleto;
                    apellido = "no informado";
                }
            }
            else
            {
                nombre = nombreCompleto;
                apellido = tercero.Apellidos;
            }
            return new FacturaTitan()
            {
                Comprobante = new ComprobanteTitan()
                {
                    TipoComprobante = "01",
                    Fecha = factura.Fecha.ToString("yyyy-MM-dd"),
                    Prefijo = alegraOptions.Prefix,
                    Numero = numero.ToString(),
                    Observaciones = "Facturación electrónica",
                    MetodoPago = new List<MetodoPagoTitan>()
                    {
                        new MetodoPagoTitan()
                        {
                            FormaPago = getFormaPago(factura.FormaDePago),
                            MedioPago = "ZZZ",
                            Fechavence = factura.Fecha.ToString("yyyy-MM-dd"),
                            Diasplazo = "0"
                        }
                    }
                },
                Emisor = new EmisorTitan()
                {
                    Identificacion = alegraOptions.DataicoAccountId
                },
                Receptor = new ReceptorTitan()
                {
                    Identificacion = factura.Identificacion.ToString(),
                    Dv = "0",
                    Apellido1 = tercero.DescripcionTipoIdentificacion == "Nit" ? "" : apellido,
                    Apellido2 = "",
                    Nombres = tercero.DescripcionTipoIdentificacion == "Nit" ? "" : nombre,
                    Razonsocial = tercero.DescripcionTipoIdentificacion == "Nit" ? nombreCompleto : "",
                    Direccion = string.IsNullOrEmpty(tercero.Direccion) ? "0" : tercero.Direccion,
                    CodCiudad = "50001",
                    Telefono = string.IsNullOrEmpty(tercero.Celular) ? "0" : tercero.Celular,
                    Correo = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.Contains("no informado") ? alegraOptions.Correo : tercero.Correo,

                },
                Detalles = new List<DetalleTitan>()
                {
                    new DetalleTitan()
                    {
                        Item = "0001",
                        Nrodoc = factura.Consecutivo.ToString(),
                        codigo = "0001",
                        Nombre = factura.Combustible,
                        Cantidad = factura.Cantidad.ToString(),
                        ValorUnitario = factura.Precio.ToString(),
                        Descuento = factura.Descuento.ToString(),
                        SubTotal = factura.SubTotal.ToString(),
                        Total = factura.Total.ToString(),
                        Impuestos = new List<ImpuestoTitan>(){ new ImpuestoTitan() { Base = "0", Impuesto = "0", Nombre = "No" , Porcentaje = "0" } }



                    }
                }



            };
        }

        private string getFormaPago(string formaDePago)
        {
            switch ((formaDePago ?? string.Empty).ToLower())
            {
                case "efectivo":
                    return "1";

                default:
                    return "2";
            }
        }

        private string GetPrimaryPaymentForm(string formaDePago, string formaDePago2, decimal? total1, decimal? total2)
        {
            var forma1 = string.IsNullOrWhiteSpace(formaDePago) ? "Efectivo" : formaDePago.Trim();
            var forma2 = string.IsNullOrWhiteSpace(formaDePago2) ? null : formaDePago2.Trim();

            if (string.IsNullOrWhiteSpace(forma2))
            {
                return forma1;
            }

            var valor1 = total1 ?? 0m;
            var valor2 = total2 ?? 0m;
            return valor2 > valor1 ? forma2 : forma1;
        }

        private string BuildPaymentDetail(string formaDePago, string formaDePago2, decimal? total1, decimal? total2)
        {
            var forma1 = string.IsNullOrWhiteSpace(formaDePago) ? "Efectivo" : formaDePago.Trim();
            var forma2 = string.IsNullOrWhiteSpace(formaDePago2) ? null : formaDePago2.Trim();

            if (string.IsNullOrWhiteSpace(forma2))
            {
                return $"Pago: {forma1}";
            }

            return $"Pagos: {forma1}={(total1 ?? 0m):0.##}, {forma2}={(total2 ?? 0m):0.##}";
        }

        private List<MetodoPagoTitan> BuildPaymentMethods(string formaPagoPrincipal, DateTime fecha)
        {
            return new List<MetodoPagoTitan>
            {
                new MetodoPagoTitan()
                {
                    FormaPago = getFormaPago(formaPagoPrincipal),
                    MedioPago = "ZZZ",
                    Fechavence = fecha.ToString("yyyy-MM-dd"),
                    Diasplazo = "0"
                }
            };
        }
    


        private static string GetRegime(int responsabilidadTributaria)
        {
            switch (responsabilidadTributaria)
            {
                case 1:
                    return "AGENTE_RETENCION_IVA";
                case 2:
                    return "SIMPLE";
                case 3:
                    return "AUTORRETENEDOR";
                case 4:
                    return "AGENTE_RETENCION_IVA";
                case 5:
                    return "GRAN_CONTRIBUYENTE";
                default:
                    return "ORDINARIO";
            }
        }

        private static string GetNivelTributario(int responsabilidadTributaria)
        {
            switch (responsabilidadTributaria)
            {
                case 1:
                    return "COMUN";
                case 2:
                    return "SIMPLIFICADO";
                case 3:
                    return "NO_RESPONSABLE_DE_IVA";
                case 4:
                    return "COMUN";
                case 5:
                    return "RESPONSABLE_DE_IVA";
                default:
                    return "COMUN";
            }
        }

        private string GetTipoIdentificacion(string descripcionTipoIdentificacion)
        {
            if (descripcionTipoIdentificacion == "Nit")
            {
                return "NIT";
            }
            else
            {
                return "CC";
            }
        }
        private string GetKindOfPErson(string descripcionTipoIdentificacion)
        {
            switch (descripcionTipoIdentificacion)
            {
                case "Nit":
                    return "PERSONA_JURIDICA";
                default:
                    return "PERSONA_NATURAL";
            }
        }
        public async Task<FacturaTitan> GetFacturaTitan(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero)
        {
            var numero = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(orden.IdEstacion.ToString());
            var formaPagoPrincipal = GetPrimaryPaymentForm(orden.FormaDePago, orden.FormaDePago2, orden.Total1, orden.Total2);
            var detallePago = BuildPaymentDetail(orden.FormaDePago, orden.FormaDePago2, orden.Total1, orden.Total2);
            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero.Nombre.Trim();
            if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.Contains("no informado"))
            {
                if (nombreCompleto.Split(' ').Count() > 1)
                {
                    nombre = nombreCompleto.Substring(0, nombreCompleto.LastIndexOf(" "));
                    apellido = nombreCompleto.Split(' ').Last();
                }
                else
                {
                    nombre = nombreCompleto;
                    apellido = "no informado";
                }
            }
            else
            {
                nombre = nombreCompleto;
                apellido = tercero.Apellidos;
            }
            return new FacturaTitan()
            {
                Comprobante = new ComprobanteTitan()
                {
                    TipoComprobante = "01",
                    Fecha = orden.Fecha.ToString("yyyy-MM-dd"),
                    Prefijo = alegraOptions.Prefix,
                    Numero = numero.ToString(),
                    Observaciones = $"Facturación electrónica. {detallePago}",
                    MetodoPago = BuildPaymentMethods(formaPagoPrincipal, orden.Fecha)
                },
                Emisor = new EmisorTitan()
                {
                    Identificacion = alegraOptions.DataicoAccountId
                },
                Receptor = new ReceptorTitan()
                {
                    Identificacion = orden.Identificacion.ToString(),
                    Dv = "0",
                    Apellido1 = tercero.DescripcionTipoIdentificacion == "Nit" ? "" : apellido,
                    Apellido2 = "",
                    Nombres = tercero.DescripcionTipoIdentificacion == "Nit" ? "" : nombre,
                    Razonsocial = tercero.DescripcionTipoIdentificacion == "Nit" ? nombreCompleto : "",
                    Direccion = string.IsNullOrEmpty(tercero.Direccion) ? "0" : tercero.Direccion,
                    CodCiudad = "50001",
                    Telefono = string.IsNullOrEmpty(tercero.Celular) ? "0" : tercero.Celular,
                    Correo = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.Contains("no informado") ? alegraOptions.Correo : tercero.Correo,

                },
                Detalles = new List<DetalleTitan>()
                {
                    new DetalleTitan()
                    {
                        Item = "0001",
                        Nrodoc = orden.IdVentaLocal.ToString(),
                        codigo = "0001",
                        Nombre = orden.Combustible,
                        Cantidad = orden.Cantidad.ToString(),
                        ValorUnitario = orden.Precio.ToString(),
                        Descuento = orden.Descuento.ToString(),
                        SubTotal = orden.SubTotal.ToString(),
                        Total = orden.Total.ToString(),
                        Impuestos = new List<ImpuestoTitan>(){ new ImpuestoTitan() { Base = "0", Impuesto = "0", Nombre = "No", Porcentaje = "0" } }



                    }
                }



            };
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, Guid estacionGuid)
        {

            try
            {

                var invoice = await GetFacturaTitan(orden, tercero);
                Console.WriteLine(JsonConvert.SerializeObject(invoice));
                while (true)
                {

                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 0, 5, 0, 0);

                        var path = $"{alegraOptions.Url}";
                        var form = new MultipartFormDataContent
                        {
                            { new StringContent(JsonConvert.SerializeObject(invoice)), "json" },
                            { new StringContent(alegraOptions.Token), "token" }
                        };
                        var response = client.PostAsync(path, form).Result;
                        string responseBody = await response.Content.ReadAsStringAsync();
                        try
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        catch (Exception)
                        {
                            try
                            {
                                var respuestaError = JsonConvert.DeserializeObject<ErrorDataico>(responseBody);
                                if (respuestaError.errors.Any(x => x.path.Any(y => y.Contains("invoice"))))
                                {

                                    throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                }
                                else
                                {
                                    throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                }
                            }
                            catch (Exception ex)
                            {
                                throw new AlegraException(responseBody + ex.Message + JsonConvert.SerializeObject(invoice));
                            }
                        }
                        Console.WriteLine(responseBody);
                        var respuesta = JsonConvert.DeserializeObject<ResponseTitan>(responseBody);
                        Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                        Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                        await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), Int32.Parse(invoice.Comprobante.Numero) + 1);
                        return "Enviada" + ":" + invoice.Comprobante.Numero + ":" + invoice.Comprobante.Numero;

                    }
                }
            }
            catch (Exception ex)
            {
                throw new AlegraException(ex.Message);
            }
        }

        public async Task<string> GenerarFacturaElectronica(List<Modelo.OrdenDeDespacho> ordenes, Modelo.Tercero tercero, IEnumerable<Item> items)
        {
            var responseBody = await invoiceHandler.CrearFatura(ordenes.ConvertirAInvoice(tercero, items), alegraOptions);
            var invoice = JsonConvert.DeserializeObject<ResponseInvoice>(responseBody);
            return invoice.numberTemplate.prefix + invoice.numberTemplate.number + ":" + invoice.id;
        }
        public async Task<string> GenerarFacturaElectronica(List<Modelo.Factura> facturas, Modelo.Tercero tercero, IEnumerable<Item> items)
        {
            var responseBody = await invoiceHandler.CrearFatura(facturas.ConvertirAInvoice(tercero, items), alegraOptions);
            var invoice = JsonConvert.DeserializeObject<ResponseInvoice>(responseBody);
            return invoice.numberTemplate.prefix + invoice.numberTemplate.number + ":" + invoice.id;
        }

        public async Task<int> GenerarTercero(Modelo.Tercero tercero)
        {
            return (await contactsHandler.CrearCliente(tercero.ConvertirAContact(), alegraOptions));
        }

        public async Task<ResponseInvoice> GetFacturaElectronica(string id)
        {
            return await invoiceHandler.GetFatura(id, alegraOptions);
        }

        public async Task<Item> GetItem(string name)
        {
            return await itemHandler.GetItem(name, alegraOptions);
        }

        public async Task<ResolucionElectronica> GetResolucionElectronica(string estacion)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                client.DefaultRequestHeaders.Add("auth-token", alegraOptions.Token);
                var path = $"{alegraOptions.Url}numberings/invoice";
                var response = client.GetAsync(path).Result;
                string responseBody = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception)
                {
                   
                }

                var respuesta = JsonConvert.DeserializeObject<ResolucionesDataico>(responseBody);
                Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                return new ResolucionElectronica(respuesta.numberings.First());
            }
        }

        public async Task<string> getJson(Modelo.OrdenDeDespacho ordenDeDespachoEntity, Guid estacion)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<TerceroResponse>> GetTerceros(int start)
        {

            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Basic", alegraOptions.Auth);
                var path = $"{alegraOptions.Url}contacts/?start={start}";
                var response = client.GetAsync(path).Result;
                string responseBody = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception)
                {
                    throw new AlegraException(responseBody);
                }

                return JsonConvert.DeserializeObject<IEnumerable<TerceroResponse>>(responseBody);
            }
        }

        public Task<string> GenerarFacturaElectronica(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetFacturaElectronica(string id, Guid estacionGuid)
        {
            throw new NotImplementedException();
        }

        public Task<Item> GetItem(string name, Alegra options)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReenviarFactura(Repositorio.Entities.OrdenDeDespacho orden, Guid estacion)
        {
            throw new NotImplementedException();
        }

        public Task<string> getJsonCanastilla(Modelo.FacturaCanastilla facturaCanastilla, Guid estacio)
        {
            throw new NotImplementedException();
        }
    }
}
