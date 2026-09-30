using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using FacturacionelectronicaCore.Repositorio.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class FacturacionSiigo : IFacturacionElectronicaFacade
    {
        private readonly Alegra alegraOptions;
        private readonly ContactsHandler contactsHandler;
        private readonly InvoiceHandler invoiceHandler;
        private readonly ItemHandler itemHandler;
        private readonly ResolucionesHandler resolucionesHandler;
        private readonly ResolucionNumber _resolucionNumber;
        private readonly IResolucionRepositorio _resolucionRepositorio;
        private readonly IEmpleadoRepositorio _empleadoRepositorio;
        private static readonly Semaphore _semaphore = new(initialCount: 1, maximumCount: 1);

        public FacturacionSiigo(IOptions<Alegra> alegra, ResolucionNumber resolucionNumber, IResolucionRepositorio resolucionRepositorio, IEmpleadoRepositorio empleadoRepositorio)
        {
            alegraOptions = alegra.Value;

            _resolucionNumber = resolucionNumber;

            _resolucionRepositorio = resolucionRepositorio;
            _empleadoRepositorio = empleadoRepositorio;
        }

        public async Task ActualizarTercero(Modelo.Tercero tercero, string idFacturacion)
        {
            await contactsHandler.ActualizarCliente(idFacturacion, tercero.ConvertirAContact(), alegraOptions);
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.Factura factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            var invoice = await GetFacturaSiigo(factura, tercero, estacionGuid);
            try
            {

                Console.WriteLine(JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }));
                using (var client = new HttpClient())
                {
                    try
                    {
                        var token = await GetToken(null);
                        client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Add("Partner-Id", "SIGES");
                        var content = new StringContent(JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        Console.WriteLine($"{alegraOptions.Url}v1/invoices");
                        var response = await client.PostAsync($"{alegraOptions.Url}v1/invoices", content);
                        string responseBody = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(responseBody);
                        response.EnsureSuccessStatusCode();

                        var respuestaSiigo = JsonConvert.DeserializeObject<FacturaSiigoResponse>(responseBody);
                        if (responseBody.ToLower().Contains("error"))
                        {

                            return "error:" + responseBody + ":" + JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
                        }
                        else
                        {
                            Console.WriteLine("Ok:" + factura.IdVentaLocal + ":" + respuestaSiigo.stamp.cufe);
                            return "Ok:" + respuestaSiigo.prefix + respuestaSiigo.number + ":" + (respuestaSiigo.stamp.cufe ?? respuestaSiigo.prefix + respuestaSiigo.number);

                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        return "error:" + ex.Message +  ":" + JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);

                return "error:" + ex.Message + ":" + JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            }
        }
        private string GetTipoIdentificacion(string descripcionTipoIdentificacion)
        {
            if (descripcionTipoIdentificacion == "Nit")
            {
                return "Company";
            }
            else
            {
                return "Person";
            }
        }
        public async Task<FacturaSiigo> GetFacturaSiigo(Modelo.Factura x, Modelo.Tercero tercero, Guid estacionGuid)
        {
            var numero = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());

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
            return new FacturaSiigo()
            {

                stamp = new StampSiigoRequest { send = true },
                document = new DocumentSiigoRequest { id = alegraOptions.Documento },
                date = x.Fecha.ToString("yyyy-MM-dd"),
                customer = new CustomerSiigoRequest
                {
                    person_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                    name = new List<string>() { nombre, apellido },
                    id_type = GetTipoIdentificacionNumero(tercero.DescripcionTipoIdentificacion),
                    identification = tercero.Identificacion,
                    branch_office = 0,
                    address = new AddressSiigoRequest
                    {
                        address = "Cra. 18 #79A - 42",
                        city = new CitySiigoRequest
                        {
                            country_code = "Co",
                            state_code = "11",
                            city_code = "11001"
                        }
                    },
                    phones = new List<PhoneSiigoRequest> { new PhoneSiigoRequest { number = string.IsNullOrEmpty(tercero.Telefono) || isAlphaNumeric(tercero.Telefono) ? "3000000000" : tercero.Telefono, } },
                    contacts = new List<ContactSiigoRequest> { new ContactSiigoRequest {
                        first_name = nombre,
                        last_name = apellido, email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.ToLower() == "no informado" ? alegraOptions.Correo : tercero.Correo, } }
                },
                seller = alegraOptions.Seller,
                payments = BuildPayments(x.FormaDePago, null, null, null, Math.Round((decimal)x.Precio * (decimal)x.Cantidad, 2)),
                items = new List<ItemSiigoRequest>()
                {
                    new ItemSiigoRequest
                    {
                        code = GetCodeCombustible(x.Combustible),
                        description = x.Combustible,
                        discount = Convert.ToInt32(x.Descuento),
                        price =  Math.Round(Convert.ToDouble(x.Precio), 2),
                        quantity = Math.Round(Convert.ToDouble(x.Cantidad),2),

                    }
                },
                cost_center = 85
            };
        }
        public bool isAlphaNumeric(string strToCheck)
        {
            Regex rg = new Regex(@"^[a-zA-Z \s,]*$");
            return rg.IsMatch(strToCheck);
        }
        private string GetCodeCombustible(string combustible)
        {
            if (combustible.ToLower().Contains("acpm") || combustible.ToLower().Contains("die"))
            {
                return  alegraOptions.Acpm;
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

        private int GetPaymentType(string formaDePago)
        {
            var forma = (formaDePago ?? string.Empty).ToLower();

            if (forma.Contains("de"))
            {
                return alegraOptions.Debito;
            }
            else if (forma.Contains("cre"))
            {
                return alegraOptions.Credito;
            }
            else 
            {
                return alegraOptions.Efectivo;
            }
        }

        private string GetTipoIdentificacionNumero(string descripcionTipoIdentificacion)
        {
            if (descripcionTipoIdentificacion == "Nit")
            {
                return "31";
            }
            else
            {
                return "13";
            }
        }

        public async Task<FacturaSiigo> GetFacturaSiigo(Modelo.OrdenDeDespacho x, Modelo.Tercero tercero, Guid estacionGuid, ResolucionFacturaElectronica numero)
        {

            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero.Nombre.Trim();
            if (string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.Contains("no informado"))
            {
                if(tercero.DescripcionTipoIdentificacion == "Nit")
                {

                    nombre = nombreCompleto;
                    apellido = "no informado";
                }
                else {
                    if (nombreCompleto.Split(' ').Count() > 1)
                    {
                        nombre = nombreCompleto.Substring(0, nombreCompleto.LastIndexOf(" "));
                        apellido = int.TryParse(nombreCompleto.Split(' ').Last(), out _) ? "no informado" : nombreCompleto.Split(' ').Last();
                    }
                    else
                    {
                        nombre = nombreCompleto;
                        apellido = "no informado";
                    }
                }
                
            }
            else
            {
                nombre = nombreCompleto;
                apellido = tercero.Apellidos;
            }
            return new FacturaSiigo()
            {
                mail = new MailSiigoRequest { send = true },
                stamp = new StampSiigoRequest { send = true },
                document = new DocumentSiigoRequest { id = int.Parse(numero.idNumeracion) },
                date = x.Fecha.ToString("yyyy-MM-dd"),
                
                customer = new CustomerSiigoRequest
                {
                    person_type = GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion),
                    name = new List<string>() { nombre, apellido },
                    id_type = GetTipoIdentificacionNumero(tercero.DescripcionTipoIdentificacion),
                    identification = tercero.Identificacion,
                    branch_office = 0,
                    address = new AddressSiigoRequest
                    {
                        address = "Cra. 18 #79A - 42",
                        city = new CitySiigoRequest
                        {
                            country_code = "Co",
                            state_code = "11",
                            city_code = "11001"
                        }
                    },
                    phones = new List<PhoneSiigoRequest> { new PhoneSiigoRequest { number = string.IsNullOrEmpty(tercero.Telefono) || tercero.Telefono.ToLower() == "no informado" || isAlphaNumeric(tercero.Telefono) ? "3000000000" : tercero.Telefono, } },
                    contacts = new List<ContactSiigoRequest> { new ContactSiigoRequest {
                        first_name = nombre,
                        last_name = apellido,
                        email = string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.ToLower() == "no informado" ? alegraOptions.Correo : tercero.Correo, } }
                },
                seller = alegraOptions.Seller,

                payments = BuildPayments(x.FormaDePago, x.FormaDePago2, x.Total1, x.Total2, Math.Round((decimal)x.Total, 2)),
                items = new List<ItemSiigoRequest>()
                {
                    new ItemSiigoRequest
                    {
                        code = GetCodeCombustible(x.Combustible),
                        description = x.Combustible,
                        discount = Convert.ToInt32(x.Descuento),
                        price =  Math.Round(Convert.ToDouble(x.Precio), 2),
                        quantity = Math.Round(Convert.ToDouble(x.Cantidad),2),

                    }
                },
            };
        }

        private List<PaymentSiigoRequest> BuildPayments(string formaDePago, string formaDePago2, decimal? total1, decimal? total2, decimal totalFactura)
        {
            var pagos = new List<PaymentSiigoRequest>();

            var forma1 = string.IsNullOrWhiteSpace(formaDePago) ? "Efectivo" : formaDePago.Trim();
            var forma2 = string.IsNullOrWhiteSpace(formaDePago2) ? null : formaDePago2.Trim();

            if (string.IsNullOrWhiteSpace(forma2))
            {
                pagos.Add(new PaymentSiigoRequest
                {
                    id = GetPaymentType(forma1),
                    value = Math.Round(Convert.ToDouble(totalFactura), 2)
                });
                return pagos;
            }

            var valor1 = Math.Round(total1 ?? 0m, 2);
            var valor2 = Math.Round(total2 ?? (totalFactura - valor1), 2);

            if (valor1 <= 0m && valor2 <= 0m)
            {
                valor1 = totalFactura;
                valor2 = 0m;
            }

            pagos.Add(new PaymentSiigoRequest
            {
                id = GetPaymentType(forma1),
                value = Math.Round(Convert.ToDouble(valor1), 2)
            });

            if (valor2 > 0m)
            {
                pagos.Add(new PaymentSiigoRequest
                {
                    id = GetPaymentType(forma2),
                    value = Math.Round(Convert.ToDouble(valor2), 2)
                });
            }

            return pagos;
        }

        public async Task<string> GetToken(ResolucionFacturaElectronica numero)
        {

            try
            {
                var tokenrequets = new SiigoTokenRequest
                {
                    access_key = alegraOptions.Token,
                    username = numero.correo

                };
                Console.WriteLine(JsonConvert.SerializeObject(tokenrequets));

                using (var client = new HttpClient())
                {
                    try
                    {

                        var content = new StringContent(JsonConvert.SerializeObject(tokenrequets));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        Console.WriteLine($"{alegraOptions.Url}auth");
                        var response = await client.PostAsync($"{alegraOptions.Url}auth", content);
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync();
                        var respuestaSiigo = JsonConvert.DeserializeObject<SiigoTokenResponse>(responseBody);

                        return respuestaSiigo.access_token;
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        throw;
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


        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, Guid estacionGuid)
        {


            _semaphore.WaitOne();

            var numero = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
            var invoice = await GetFacturaSiigo(orden, tercero, estacionGuid, numero);
            try
            {

                Console.WriteLine(JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }));
                using (var client = new HttpClient())
                {
                    try
                    {
                        var token = await GetToken(numero);
                        client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Add("Partner-Id", "SIGES");
                        var content = new StringContent(JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        Console.WriteLine($"{alegraOptions.Url}v1/invoices");
                        var response = await client.PostAsync($"{alegraOptions.Url}v1/invoices", content);
                        string responseBody = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(responseBody);
                        response.EnsureSuccessStatusCode();

                        var respuestaSiigo = JsonConvert.DeserializeObject<FacturaSiigoResponse>(responseBody);
                        if (responseBody.ToLower().Contains("error"))
                        {

                            return "error:" + responseBody + ":" + JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
                        }
                        else
                        {
                            Console.WriteLine("Ok:" + orden.IdVentaLocal + ":" + respuestaSiigo.stamp?.cufe);
                            //await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), invoice.document.id + 1);
                            return "Ok:" + respuestaSiigo.prefix + respuestaSiigo.number + ":" + (respuestaSiigo.stamp?.cufe ?? respuestaSiigo.prefix + respuestaSiigo.number + ":" + respuestaSiigo.id);

                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        return "error:" + ex.Message + ":" + JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);

                return "error:" + ex.Message + ":" + JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });
            }
            finally
            {
                _semaphore.Release();
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
        public async Task<string> getJson(Modelo.OrdenDeDespacho ordenDeDespachoEntity, Guid estacion)
        {
            throw new NotImplementedException();
        }


        public Task<string> GenerarFacturaElectronica(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetFacturaElectronica(string id, Guid estacionGuid)
        {
            var numero = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
            try
            {

                using (var client = new HttpClient())
                {
                    try
                    {
                        var token = await GetToken(numero);
                        client.Timeout = new TimeSpan(0, 0, 5, 0, 0);
                        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
                        client.DefaultRequestHeaders.Add("Partner-Id", "SIGES");
                        var response = await client.GetAsync($"{alegraOptions.Url}v1/invoices/{id.Split(":").Last()}");
                        string responseBody = await response.Content.ReadAsStringAsync();

                        Console.WriteLine(responseBody);
                        response.EnsureSuccessStatusCode();

                        var respuestaSiigo = JsonConvert.DeserializeObject<FacturaSiigoResponse>(responseBody);
                        if (responseBody.ToLower().Contains("error"))
                        {

                            return null;
                        }
                        else
                        {
                            return $"Factura electrónica\n\r{respuestaSiigo.prefix + respuestaSiigo.number}\n\rCUFE:\n\r{respuestaSiigo?.stamp?.cufe}";
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        return null;

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
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