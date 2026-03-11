using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class FacturacionDataico : IFacturacionElectronicaFacade
    {
        private readonly Alegra alegraOptions;
        private readonly ContactsHandler contactsHandler;
        private readonly InvoiceHandler invoiceHandler;
        private readonly ItemHandler itemHandler;
        private readonly ResolucionNumber _resolucionNumber;
        private readonly IResolucionRepositorio _resolucionRepositorio;
        private static readonly SemaphoreSlim _globalSemaphore = new(1, 1);
        private readonly IOrdenDeDespachoRepositorio _ordenDeDespachoRepositorio;

        public FacturacionDataico(IOptions<Alegra> alegra,
        ResolucionNumber resolucionNumber,
        IResolucionRepositorio resolucionRepositorio,
        IOrdenDeDespachoRepositorio ordenDeDespachoRepositorio)
        {
            alegraOptions = alegra.Value;

            _resolucionNumber = resolucionNumber;

            _resolucionRepositorio = resolucionRepositorio;

            _ordenDeDespachoRepositorio = ordenDeDespachoRepositorio;
        }

        public async Task ActualizarTercero(Modelo.Tercero tercero, string idFacturacion)
        {
            await contactsHandler.ActualizarCliente(idFacturacion, tercero.ConvertirAContact(), alegraOptions);
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.Factura factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            await _globalSemaphore.WaitAsync();
            try
            {
                var invoice = await GetFacturaDataico(factura, tercero, estacionGuid.ToString());
                //Console.WriteLine(JsonConvert.SerializeObject(invoice));
                var triedAgain = 0;
                while (triedAgain++ < 1)
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                        client.DefaultRequestHeaders.Add("auth-token", alegraOptions.Token);
                        var path = $"{alegraOptions.Url}invoices";
                        var content = new StringContent(JsonConvert.SerializeObject(invoice, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = client.PostAsync(path, content).Result;
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
                                    var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                    if (error.error.Contains("Tiene que ser el siguiente"))
                                    {
                                        var numberpos = error.error.IndexOf('\'');
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        var fin = error.error.IndexOf('\'', numberpos + 1);
                                        var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                        _resolucionNumber.number = Int32.Parse(number);

                                    }
                                    else
                                    {
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                    }
                                    invoice.invoice.number = _resolucionNumber.number.ToString();
                                }
                                else
                                {
                                    throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                }
                            }
                            catch (Exception)
                            {
                                throw new AlegraException(responseBody + ":" + JsonConvert.SerializeObject(invoice));
                            }
                        }
                        if (!responseBody.Contains("cufe"))
                        {

                            try
                            {
                                var respuestaError = JsonConvert.DeserializeObject<ErrorDataico>(responseBody);
                                if (respuestaError.errors.Any(x => x.path.Any(y => y.Contains("invoice"))))
                                {
                                    var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                    if (error.error.Contains("Tiene que ser el siguiente"))
                                    {
                                        var numberpos = error.error.IndexOf('\'');
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        var fin = error.error.IndexOf('\'', numberpos + 1);
                                        var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                        _resolucionNumber.number = Int32.Parse(number);
                                    }
                                    else
                                    {
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                    }
                                    invoice.invoice.number = _resolucionNumber.number.ToString();

                                }
                                else
                                {
                                    throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                }
                            }
                            catch (Exception)
                            {
                                return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                            }
                        }
                        else
                        {
                            //Console.WriteLine(responseBody);
                            var respuesta = JsonConvert.DeserializeObject<RespuestaDataico>(responseBody);
                            //Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                            //Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                            await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), int.Parse(invoice.invoice.number) + 1);
                            return respuesta.dian_status + ":" + respuesta.number + ":" + respuesta.cufe;
                        }
                    }
                }
                return "error:" + JsonConvert.SerializeObject(invoice);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
            finally
            {
                _globalSemaphore.Release();
            }
        }

        public async Task<FacturaDataico> GetFacturaDataico(Modelo.Factura factura, Modelo.Tercero tercero, string estacion)
        {
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion);
            var numero = resolucion.numeroActual;

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
            var cantidadRedondeada = Math.Round((double)factura.Cantidad, 2);
            var precioCalculado = (cantidadRedondeada > 0)
                ? Math.Round(((double)factura.Total + (double)factura.Descuento) / cantidadRedondeada, 2)
                : 0.0;
            var items = new List<ItemDataico>();
            items.Add(new ItemDataico()
            {
                sku = GetCodeCombustible(factura.Combustible),
                description = factura.Combustible,
                quantity = cantidadRedondeada,
                taxes = new List<TaxDataico>() { },
                measuring_unit = "GL",
                retentions = new List<RetentionDataico>() { },
                original_price = (factura.Descuento > 0 && cantidadRedondeada > 0) ? (double?)(precioCalculado + ((double)factura.Descuento / cantidadRedondeada)) : null,
                discount_rate = (decimal?)((factura.Descuento > 0 && factura.SubTotal > 0) ? Math.Round(factura.Descuento / (factura.SubTotal + factura.Descuento) * 100, 2) : (decimal?)null),
                price = precioCalculado
            });
            return new FacturaDataico()
            {
                actions = new ActionsDataico() { send_dian = true, send_email = true },
                invoice = new InvoiceDataico()
                {
                    notes = new List<string>() { $"Placa: {factura.Placa}, Kilometraje : {factura.Kilometraje}, Nro Transaccion : NA" },
                    env = "PRODUCCION",
                    dataico_account_id = alegraOptions.DataicoAccountId,
                    issue_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    payment_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    order_reference = factura.DescripcionResolucion + factura.Consecutivo,
                    invoice_type_code = "FACTURA_VENTA",
                    payment_means = GetPaymentType(factura.FormaDePago),
                    payment_means_type = GetPaymentMeansType(factura.FormaDePago),
                    number = numero.ToString(),
                    numbering = new NumberingDataico()
                    {
                        resolution_number = resolucion.resolucion,
                        flexible = true,
                        prefix = resolucion.prefijo
                    },
                    customer = new CustomerDataico()
                    {
                        email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.Contains("no informado") ? alegraOptions.Correo : tercero.Correo,
                        phone = string.IsNullOrEmpty(tercero.Celular) ? "0" : tercero.Celular,
                        party_identification_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                        party_identification = tercero.Identificacion,
                        party_type = GetKindOfPErson(tercero.DescripcionTipoIdentificacion),
                        tax_level_code = GetNivelTributario(tercero.ResponsabilidadTributaria),
                        regimen = GetRegime(tercero.ResponsabilidadTributaria),
                        address_line = string.IsNullOrEmpty(tercero.Direccion) ? "0" : tercero.Direccion,
                        country_code = "CO",
                        first_name = nombre,
                        family_name = apellido,
                        company_name = tercero.Nombre,
                        department = alegraOptions.Department ?? "73",
                        city = alegraOptions.City ?? "001",
                    },
                    items = items,
                    currency_exchange_rate = 1
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
        public async Task<FacturaDataico> GetFacturaDataico(Modelo.OrdenDeDespacho factura, Modelo.Tercero tercero, string estacion, ResolucionFacturaElectronica resolucion)
        {
            var numero = resolucion.numeroActual;

            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero.Nombre.Trim();
            if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.ToLower().Contains("no informado"))
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


            var cantidadRedondeada = Math.Round((double)factura.Cantidad, 2);
            var precioCalculado = (cantidadRedondeada > 0)
                ? Math.Round(((double)factura.Total + (double)factura.Descuento) / cantidadRedondeada, 2)
                : 0.0;
            var items = new List<ItemDataico>();
            items.Add(new ItemDataico()
            {
                sku = GetCodeCombustible(factura.Combustible),
                description = factura.Combustible,
                quantity = cantidadRedondeada,
                taxes = new List<TaxDataico>() { },
                measuring_unit = "GL",
                retentions = new List<RetentionDataico>() { },
                original_price = (double?)((factura.Descuento > 0 && cantidadRedondeada > 0) ? precioCalculado + ((double)factura.Descuento / cantidadRedondeada) : (double?)null),
                discount_rate = (factura.Descuento > 0 && factura.Total > 0) ? Math.Round(factura.Descuento / ((decimal)factura.Total + factura.Descuento) * 100, 2) : null,
                price = (double)precioCalculado,
            });

            var formaPagoPrincipal = GetPrimaryPaymentForm(factura.FormaDePago, factura.FormaDePago2, factura.Total1, factura.Total2);
            var detallePago = BuildPaymentDetailNote(factura.FormaDePago, factura.FormaDePago2, factura.Total1, factura.Total2);

            return new FacturaDataico()
            {
                actions = new ActionsDataico() { send_dian = true, send_email = true },
                invoice = new InvoiceDataico()
                {
                    notes = new List<string>() { $"Placa: {factura.Placa}, Kilometraje : {factura.Kilometraje}, Nro Transaccion : {factura.numeroTransaccion}. {detallePago}" },
                    env = "PRODUCCION",
                    dataico_account_id = resolucion.idNumeracion,
                    issue_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    payment_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    order_reference = factura.IdVentaLocal.ToString(),
                    invoice_type_code = "FACTURA_VENTA",
                    payment_means = GetPaymentType(formaPagoPrincipal),
                    payment_means_type = GetPaymentMeansType(formaPagoPrincipal),
                    number = numero.ToString(),
                    numbering = new NumberingDataico()
                    {
                        resolution_number = resolucion.resolucion,
                        flexible = true,
                        prefix = resolucion.prefijo
                    },
                    customer = new CustomerDataico()
                    {
                        email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.ToLower().Contains("no informado") ? alegraOptions.Correo : tercero.Correo,
                        phone = string.IsNullOrEmpty(tercero.Celular) ? "0" : tercero.Celular,
                        party_identification_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                        party_identification = tercero.Identificacion,
                        party_type = GetKindOfPErson(tercero.DescripcionTipoIdentificacion),
                        tax_level_code = GetNivelTributario(tercero.ResponsabilidadTributaria),
                        regimen = GetRegime(tercero.ResponsabilidadTributaria),
                        address_line = string.IsNullOrEmpty(tercero.Direccion) ? "0" : tercero.Direccion,
                        country_code = "CO",
                        first_name = nombre,
                        family_name = apellido,
                        company_name = tercero.Nombre,
                        department = alegraOptions.Department ?? "73",
                        city = alegraOptions.City ?? "001",
                    },
                    items = items,
                    currency_exchange_rate = 1
                }
            };
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

        private string BuildPaymentDetailNote(string formaDePago, string formaDePago2, decimal? total1, decimal? total2)
        {
            var forma1 = string.IsNullOrWhiteSpace(formaDePago) ? "Efectivo" : formaDePago.Trim();
            var forma2 = string.IsNullOrWhiteSpace(formaDePago2) ? null : formaDePago2.Trim();

            if (string.IsNullOrWhiteSpace(forma2))
            {
                return $"FormaPago: {forma1}";
            }

            var valor1 = total1 ?? 0m;
            var valor2 = total2 ?? 0m;
            return $"FormasPago: {forma1}={valor1:0.##}, {forma2}={valor2:0.##}";
        }

        private string GetCodeCombustible(string combustible)
        {
            if (combustible.ToLower().Contains("acpm") || combustible.ToLower().Contains("die"))
            {
                return alegraOptions.Acpm;
            }
            else if (combustible.ToLower().Contains("corri"))
            {
                return alegraOptions.Corriente;
            }
            else
            {
                return alegraOptions.Gas;
            }
        }

        private string GetPaymentType(string formaDePago)
        {
            if (formaDePago.ToLower().Contains("dé") && formaDePago.ToLower().Contains("tran"))
            {
                return "DEBIT_TRANSFER";
            }
            else if (formaDePago.ToLower().Contains("dé") && formaDePago.ToLower().Contains("tar"))
            {
                return "DEBIT_CARD";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("ban") && formaDePago.ToLower().Contains("tran"))
            {
                return "BANK_TRANSFER";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("tran"))
            {
                return "CREDIT_TRANSFER";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("tar"))
            {
                return "CREDIT_CARD";
            }
            else if (formaDePago.ToLower().Contains("ban") && formaDePago.ToLower().Contains("cons"))
            {
                return "DEBIT_BANK_TRANSFER";
            }
            else
            {
                return "CASH";
            }
        }


        private string GetPaymentMeansType(string formaDePago)
        {
            if (formaDePago.ToLower().Contains("dé") && formaDePago.ToLower().Contains("tran"))
            {
                return "DEBITO";
            }
            else if (formaDePago.ToLower().Contains("dé") && formaDePago.ToLower().Contains("tar"))
            {
                return "DEBITO";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("ban") && formaDePago.ToLower().Contains("tran"))
            {
                return "DEBITO";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("tran"))
            {
                return "CREDITO";
            }
            else if (formaDePago.ToLower().Contains("cré") && formaDePago.ToLower().Contains("tar"))
            {
                return "CREDITO";
            }
            else if (formaDePago.ToLower().Contains("ban") && formaDePago.ToLower().Contains("cons"))
            {
                return "DEBITO";
            }
            else
            {
                return "DEBITO";
            }
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, Guid estacionGuid)
        {

            await _globalSemaphore.WaitAsync();
            var invoice = new FacturaDataico();
            var respuestaFactura = string.Empty;
            var wasParsed = false;
            try
            {
                var ordenDeDespachoEntity = (await _ordenDeDespachoRepositorio.ObtenerOrdenDespachoPorIdVentaLocal(orden.IdVentaLocal, estacionGuid)).FirstOrDefault();
                

                Console.WriteLine(estacionGuid.ToString());
                var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());

                // If the order has an error and idFacturaElectronica contains a previously built invoice JSON
                // try to extract a FacturaDataico object from it and reuse it instead of rebuilding.

                // if (ordenDeDespachoEntity != null && !string.IsNullOrEmpty(ordenDeDespachoEntity.idFacturaElectronica) &&
                //     !ordenDeDespachoEntity.idFacturaElectronica.Contains("order_reference_mismatch"))
                // {
                //     var parsed = TryParseFacturaDataicoFromLog(ordenDeDespachoEntity.idFacturaElectronica);
                //     if (parsed != null)
                //     {
                //         // If the parsed invoice contains an order_reference different from the current order IdVentaLocal,
                //         // rebuild the invoice using GetFacturaDataico and use that instead (the parsed one likely belongs to another order)
                //         var parsedOrderRef = parsed?.invoice?.order_reference;
                //         Console.WriteLine($"Parsed order_reference: {parsedOrderRef}, Current order IdVentaLocal: {orden.IdVentaLocal}");
                //         if (!string.IsNullOrEmpty(parsedOrderRef) && parsedOrderRef.Trim() != orden.IdVentaLocal.ToString())
                //         {
                //             invoice = await GetFacturaDataico(orden, tercero, estacionGuid.ToString(), resolucion);
                //         }
                //         else
                //         {
                //             invoice = parsed;
                //             wasParsed = true;
                //         }
                //     }
                //     else
                //     {
                //         invoice = await GetFacturaDataico(orden, tercero, estacionGuid.ToString(), resolucion);
                //     }
                // }
                // else
                // {
                //     invoice = await GetFacturaDataico(orden, tercero, estacionGuid.ToString(), resolucion);
                // }
                 invoice = await GetFacturaDataico(orden, tercero, estacionGuid.ToString(), resolucion);
                Console.WriteLine($"Factura: {invoice.invoice.order_reference}, {invoice.invoice.number} enviandose");

                var shouldSend = true;
                while (shouldSend)
                {
                    shouldSend = false;
                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                        client.DefaultRequestHeaders.Add("auth-token", resolucion.token);
                        var path = $"{alegraOptions.Url}invoices";
                        var content = new StringContent(JsonConvert.SerializeObject(invoice, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = client.PostAsync(path, content).Result;
                        string responseBody = await response.Content.ReadAsStringAsync();

                        respuestaFactura += responseBody;
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
                                    var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                    if (error.error.Contains("Tiene que ser el siguiente"))
                                    {
                                        var numberpos = error.error.IndexOf('\'');
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        var fin = error.error.IndexOf('\'', numberpos + 1);
                                        var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                        resolucion.numeroActual = Int32.Parse(number);
                                        invoice.invoice.number = resolucion.numeroActual.ToString();
                                        shouldSend = true;

                                    }
                                    else if (error.error.Contains("modificar"))
                                    {

                                        resolucion.numeroActual++;
                                        invoice.invoice.number = resolucion.numeroActual.ToString();
                                        shouldSend = true;

                                    }
                                    else
                                    {
                                        return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                                    }
                                }
                                else
                                {
                                    return "error:" + responseBody + JsonConvert.SerializeObject(invoice);

                                }
                            }
                            catch (Exception)
                            {

                                return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                            }
                        }
                        if (shouldSend)
                        {
                            continue;
                        }
                        var respuesta = JsonConvert.DeserializeObject<RespuestaDataico>(responseBody);
                        // Console.WriteLine(JsonConvert.SerializeObject(respuesta));

                        // Validate that the provider's order_reference matches our local order IdVentaLocal.
                        try
                        {
                            var j = JObject.Parse(responseBody);
                            var orderRef = j["order_reference"]?.ToString() ?? j["invoice"]?["order_reference"]?.ToString();
                            if (!string.IsNullOrEmpty(orderRef) && orderRef != orden.IdVentaLocal.ToString())
                            {
                                if (resolucion.numeroActual <= int.Parse(invoice.invoice.number))
                                {
                                    await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), int.Parse(invoice.invoice.number) + 1);

                                }
                                Console.WriteLine($"Order reference mismatch: expected {orden.IdVentaLocal}, got {orderRef}");
                                // Do not persist the resolution number if the response refers to a different order.
                                return "error:order_reference_mismatch:" + responseBody + ":" + JsonConvert.SerializeObject(invoice);
                            }

                        }
                        catch (Exception)
                        {
                            // If parsing fails, continue with the normal flow (we still have the typed respuesta object).
                        }
                        if (!wasParsed)
                        {
                            if (resolucion.numeroActual <= int.Parse(invoice.invoice.number))
                            {
                                await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), int.Parse(invoice.invoice.number) + 1);

                            }

                        }
Console.WriteLine($"Factura creada, {respuesta.order_reference}, {respuesta.dian_status}, {respuesta.number}");
                        
                        return respuesta.dian_status + ":" + respuesta.number + ":" + respuesta.cufe + ":" + respuestaFactura + ":" + JsonConvert.SerializeObject(invoice);

                    }
                }

                return "error:" + JsonConvert.SerializeObject("No se pudo procesar la factura") + ":" + respuestaFactura;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return "error:" + ex.StackTrace + ":" + ex.Message + ":" + JsonConvert.SerializeObject(invoice) + ":" + respuestaFactura;
            }
            finally
            {
                _globalSemaphore.Release();
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
            string lastResult = string.Empty;
            foreach (var factura in facturas)
            {
                // var invoice = await invoiceHandler.CrearFatura(factura.ConvertirAInvoice(items), alegraOptions);
                // lastResult = invoice.numberTemplate.prefix + invoice.numberTemplate.number + ":" + invoice.id;
                // await Task.Delay(2000); // Wait 2 seconds before sending the next factura
            }
            return lastResult;
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
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion);
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                client.DefaultRequestHeaders.Add("auth-token", resolucion.token);
                var path = $"{alegraOptions.Url}numberings/invoice";
                var response = client.GetAsync(path).Result;
                string responseBody = await response.Content.ReadAsStringAsync();
                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception)
                {
                    return new ResolucionElectronica(new Numbering() { prefix = resolucion.prefijo, dian_resolutions = new List<DianResolution>() { new DianResolution { number = resolucion.resolucion } } });
                }

                var respuesta = JsonConvert.DeserializeObject<ResolucionesDataico>(responseBody);
                // Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                // Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                return new ResolucionElectronica(respuesta.numberings.First(x => x.prefix == resolucion.prefijo));
            }
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

        /// <summary>
        /// Try to extract a FacturaDataico JSON object from an error log string.
        /// The log sometimes contains two JSON blobs; we look for the second one which starts with '{"actions":'.
        /// </summary>
        private FacturaDataico TryParseFacturaDataicoFromLog(string log)
        {
            if (string.IsNullOrEmpty(log)) return null;
            try
            {
                var marker = "{\"actions\":";
                var idx = log.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (idx < 0)
                {
                    // fallback: if the log contains '{"invoice":' it may be the invoice object
                    idx = log.IndexOf("{\"invoice\":", StringComparison.OrdinalIgnoreCase);
                }
                if (idx >= 0)
                {
                    var json = log.Substring(idx);
                    // If there are two JSON objects concatenated, try to trim to the first complete object
                    // A simple approach: try to deserialize; if it fails, attempt to find matching braces
                    try
                    {
                        return JsonConvert.DeserializeObject<FacturaDataico>(json);
                    }
                    catch
                    {
                        // Try to find the end by counting braces
                        int depth = 0;
                        int end = -1;
                        for (int i = 0; i < json.Length; i++)
                        {
                            if (json[i] == '{') depth++;
                            else if (json[i] == '}') depth--;
                            if (depth == 0)
                            {
                                end = i;
                                break;
                            }
                        }
                        if (end > 0)
                        {
                            var candidate = json.Substring(0, end + 1);
                            try { return JsonConvert.DeserializeObject<FacturaDataico>(candidate); } catch { return null; }
                        }
                    }
                }
            }
            catch (Exception) { }
            return null;
        }

        public async Task<string> getJson(Modelo.OrdenDeDespacho orden, Guid estacion)
        {
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion.ToString());

            var factura = await GetFacturaDataico(orden, orden.Tercero, estacion.ToString(), resolucion);

            // Console.WriteLine(JsonConvert.SerializeObject(factura));
            return JsonConvert.SerializeObject(factura);

        }
        public async Task<string> getJsonCanastilla(Modelo.FacturaCanastilla factura, Guid estacion)
        {
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion.ToString());

            var facturaDataico = await GetFacturaDataico(factura, factura.terceroId, estacion.ToString(), resolucion);

            //Console.WriteLine(JsonConvert.SerializeObject(facturaDataico));
            return JsonConvert.SerializeObject(facturaDataico);

        }

        public async Task<string> GenerarFacturaElectronica(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            await _globalSemaphore.WaitAsync();
            try
            {
                Console.WriteLine(estacionGuid.ToString());
                var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
                var invoice = await GetFacturaDataico(factura, tercero, estacionGuid.ToString(), resolucion);
                if (invoice == null)
                {
                    return "error:Factura canastilla sin articulos (items vacíos)";
                }
                //Console.WriteLine(JsonConvert.SerializeObject(invoice));
                var triedAgain = 0;
                while (triedAgain++ < 1)
                {

                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 0, 1, 0, 0);
                        client.DefaultRequestHeaders.Add("auth-token", resolucion.token);
                        var path = $"{alegraOptions.Url}invoices";
                        var content = new StringContent(JsonConvert.SerializeObject(invoice, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = client.PostAsync(path, content).Result;
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
                                    var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                    if (error.error.Contains("Tiene que ser el siguiente"))
                                    {
                                        var numberpos = error.error.IndexOf('\'');
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        var fin = error.error.IndexOf('\'', numberpos + 1);
                                        var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                        resolucion.numeroActual = Int32.Parse(number);
                                        invoice.invoice.number = resolucion.numeroActual.ToString();

                                    }
                                    else if (error.error.Contains("modificar"))
                                    {

                                        resolucion.numeroActual++;
                                        invoice.invoice.number = resolucion.numeroActual.ToString();

                                    }
                                    else if (error.error.ToLower().Contains("ciudad"))
                                    {
                                        var numberpos = error.error.IndexOf('\'');
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        var fin = error.error.IndexOf('\'', numberpos + 1);
                                        var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                        resolucion.numeroActual = Int32.Parse(number);
                                        invoice.invoice.number = resolucion.numeroActual.ToString();

                                    }
                                    else
                                    {
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                    }
                                }
                                else
                                {
                                    throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                }
                            }
                            catch (Exception)
                            {
                                return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                            }
                        }
                        if (!responseBody.Contains("cufe"))
                        {

                            try
                            {
                                var respuestaError = JsonConvert.DeserializeObject<ErrorDataico>(responseBody);
                                if (respuestaError.errors.Any(x => x.path.Any(y => y.Contains("invoice"))))
                                {
                                    var error = respuestaError.errors.First(x => x.path.Any(y => y.Contains("invoice")));
                                    if (error.error.Contains("Tiene que ser el siguiente"))
                                    {
                                        var numberpos = error.error.IndexOf('\'');
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        numberpos = error.error.IndexOf('\'', numberpos + 1);
                                        var fin = error.error.IndexOf('\'', numberpos + 1);
                                        var number = error.error.Substring(numberpos + 1, fin - numberpos - 1);
                                        resolucion.numeroActual = Int32.Parse(number);
                                    }
                                    else if (error.error.Contains("modificar"))
                                    {

                                        resolucion.numeroActual++;
                                    }
                                    else
                                    {
                                        throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));
                                    }
                                    invoice.invoice.number = resolucion.numeroActual.ToString();
                                }
                                else
                                {
                                    //Console.WriteLine(responseBody + JsonConvert.SerializeObject(invoice));
                                    throw new AlegraException(responseBody + JsonConvert.SerializeObject(invoice));

                                }
                            }
                            catch (Exception)
                            {
                                return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                            }
                        }
                        else
                        {
                            var respuesta = JsonConvert.DeserializeObject<RespuestaDataico>(responseBody);
                            //Console.WriteLine(JsonConvert.SerializeObject(respuesta));
                            //Console.WriteLine(JsonConvert.SerializeObject(responseBody));
                            await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), int.Parse(invoice.invoice.number) + 1);
                            return respuesta.dian_status + ":" + respuesta.number + ":" + respuesta.cufe;
                        }
                    }
                }
                return "error:" + JsonConvert.SerializeObject(invoice);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw new AlegraException(ex.Message);
            }
            finally
            {
                _globalSemaphore.Release();
            }
        }




        private async Task<FacturaDataico> GetFacturaDataico(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, string v, ResolucionFacturaElectronica resolucion)
        {
            var numero = resolucion.numeroActual;

            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero.Nombre.Trim();
            if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.ToLower().Contains("no informado"))
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
            var items = new List<ItemDataico>();
            if (factura.canastillas == null || !factura.canastillas.Any())
            {
                return null;
            }
            foreach (var articulo in factura.canastillas)
            {
                var taxes = new List<TaxDataico>();
                if (articulo.iva > 0)
                {
                    taxes.Add(new TaxDataico
                    {
                        tax_category = "IVA",
                        tax_rate = 19,
                        tax_amount = (double)articulo.iva,
                        tax_description = "IVA",
                        tax_base = (double)articulo.subtotal,
                        base_amount = (double)articulo.subtotal
                    });
                }
                var item = new ItemDataico()
                {
                    sku = "C" + articulo.Canastilla.CanastillaId.ToString(),
                    price = (double)articulo.precio,
                    original_price = (factura.descuento > 0) ? (double?)articulo.precio : null,
                    description = articulo.Canastilla.descripcion,
                    quantity = (double)articulo.cantidad,
                    taxes = taxes,
                    measuring_unit = "GL",
                    retentions = new List<RetentionDataico>() { },
                    discount_rate = (decimal?)((factura.descuento > 0 && factura.subtotal > 0) ? Math.Round((decimal)factura.descuento / (decimal)factura.subtotal * 100, 2) : (decimal?)null)
                };
                items.Add(item);
            }
            return new FacturaDataico()
            {
                actions = new ActionsDataico() { send_dian = true, send_email = true },
                invoice = new InvoiceDataico()
                {
                    notes = new List<string>() { $"Placa: {(string.IsNullOrWhiteSpace(factura.Placa) ? "N/A" : factura.Placa.Trim())}, Kilometraje : , Nro Transaccion : " },
                    env = "PRODUCCION",
                    dataico_account_id = resolucion.idNumeracion,
                    issue_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    payment_date = DateTime.Now.ToString("dd/MM/yyyy"),
                    order_reference = factura.FacturasCanastillaId.ToString(),
                    invoice_type_code = "FACTURA_VENTA",
                    payment_means = GetPaymentType(factura.codigoFormaPago.Descripcion),
                    payment_means_type = GetPaymentMeansType(factura.codigoFormaPago.Descripcion),
                    number = numero.ToString(),
                    numbering = new NumberingDataico()
                    {
                        resolution_number = resolucion.resolucion,
                        flexible = true,
                        prefix = resolucion.prefijo
                    },
                    customer = new CustomerDataico()
                    {
                        email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.ToLower().Contains("no informado") ? alegraOptions.Correo : tercero.Correo,
                        phone = string.IsNullOrEmpty(tercero.Celular) ? "0" : tercero.Celular,
                        party_identification_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                        party_identification = tercero.Identificacion,
                        party_type = GetKindOfPErson(tercero.DescripcionTipoIdentificacion),
                        tax_level_code = GetNivelTributario(tercero.ResponsabilidadTributaria),
                        regimen = GetRegime(tercero.ResponsabilidadTributaria),
                        first_name = nombre,
                        family_name = apellido,
                        company_name = tercero.Nombre,
                    },
                    items = items,
                    currency_exchange_rate = 1
                }
            };
        }

        private float GetTasa(Modelo.CanastillaFactura articulo)
        {
            return articulo.iva > 0 ? 19 : 0;
        }



        public Task<Item> GetItem(string name, Alegra options)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetFacturaElectronica(string id, Guid estacionGuid)
        {
            throw new NotImplementedException();
        }

        public Task<string> ReenviarFactura(Repositorio.Entities.OrdenDeDespacho orden, Guid estacion)
        {
            throw new NotImplementedException();
        }
    }
}
