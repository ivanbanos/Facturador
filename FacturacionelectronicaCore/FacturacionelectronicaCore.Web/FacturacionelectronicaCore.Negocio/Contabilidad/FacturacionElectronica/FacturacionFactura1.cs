using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;
using MongoDB.Driver;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class FacturacionFactura1 : IFacturacionElectronicaFacade
    {
        private string facturaString = "<Factura xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\n         xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n\t<Encabezado>\r\n\t\t<ordenCompra/>\r\n\t\t<nitemisor>{nit}</nitemisor>\r\n\t\t<tipoOpera>10</tipoOpera>\r\n\t\t<tipocomprobante>01</tipocomprobante>\r\n\t\t<noresolucion>{resolucion}</noresolucion>\r\n\t\t<prefijo>{prefijo}</prefijo>\r\n\t\t<folio>{numero}</folio>\r\n\t\t<fecha>{fecha}</fecha>\r\n\t\t<hora>{hora}</hora>\r\n\t\t<fechavencimiento>{fecha}</fechavencimiento>\r\n\t\t<moneda>COP</moneda>\r\n\t\t<xslt>1</xslt>\r\n\t\t<montoletra>{montoletra}</montoletra>\r\n\t\t<terminospago>30</terminospago>\r\n\t\t<mediospago>\r\n\t\t\t<Mp>\r\n\t\t\t\t<mediopago>{formapago}</mediopago>\r\n\t\t\t\t<fechapago>{fecha}</fechapago>\r\n\t\t\t</Mp>\r\n\t\t</mediospago>\r\n\t\t<tipoDocRec>{tipoDocumento}</tipoDocRec>\r\n\t\t<tiporeceptor>{tiporeceptor}</tiporeceptor>\r\n\t\t<nitreceptor>{nitreceptor}</nitreceptor>\r\n\t\t{digitoverificacion}\r\n\t\t<nombrereceptor>{nombre}</nombrereceptor>\r\n\t\t<paisreceptor>CO</paisreceptor>\r\n\t\t<obligacionesfiscalesreceptor>{obligacionfiscal}</obligacionesfiscalesreceptor>\r\n\t\t<tributoreceptor>{tributoreceptor}</tributoreceptor>\r\n\t\t<regimenreceptor>{regimen}</regimenreceptor>\r\n\t\t<mailreceptor>{mail}</mailreceptor>\r\n\t\t<metodopago>{metodopago}</metodopago>\r\n\t\t<subtotal>{subtotal}</subtotal>\r\n\t\t<baseimpuesto>{subtotal}</baseimpuesto>\r\n\t\t<totalsindescuento>{totalmenosdescuento}</totalsindescuento>\r\n\t\t<totaldescuentos>{descuento}</totaldescuentos>\r\n\t\t<totalimpuestos>0.00</totalimpuestos>\r\n\t\t<totalimpuestosretenidos>0.00</totalimpuestosretenidos>\r\n\t\t<total>{total}</total>\r\n\t\t<codigodepartamento>11</codigodepartamento>\r\n\t\t<codigociudadreceptor>11001</codigociudadreceptor>\r\n\t\t<direccionreceptor>CALLE 1 # 1-2</direccionreceptor>\r\n\t\t<extra1>{vendedor}</extra1>\r\n\t\t<extra2>{venta}</extra2>\r\n\t\t<extra3>{placa}</extra3>\r\n\t\t<extra4>{kilometraje}</extra4>\r\n\t\t<extra5>{codigointerno}</extra5>\r\n\t\t<extra6>{surtidor}</extra6>\r\n\t\t<extra7>{cara}</extra7>\r\n\t\t<extra8>{manguera}</extra8>\r\n\t</Encabezado>\r\n\t<Detalle>\r\n\t\t<Det>\r\n\t\t\t<idConcepto>1</idConcepto>\r\n\t\t\t<cantidad>{cantidad}</cantidad>\r\n\t\t\t<unidadmedida>{unidadmedida}</unidadmedida>\r\n\t\t\t<importe>{subtotal}</importe>\r\n\t\t\t<descripcion>{combustible}</descripcion>\r\n\t\t\t<precioUnitario>{precio}</precioUnitario>\r\n\t\t\t<identificacionproductos>{producto}</identificacionproductos>\r\n\t\t\t<SubIdentiproductos>0</SubIdentiproductos>\r\n\t\t\t<extra1>Exento</extra1>\r\n\t\t\t<ImpRet>\r\n\t\t\t\t<ImpRetDet>\r\n\t\t\t\t\t<baseimpuestos>{subtotal}</baseimpuestos>\r\n\t\t\t\t\t<ImporteImpRet>0.00</ImporteImpRet>\r\n\t\t\t\t\t<tasaImpRet>0.00</tasaImpRet>\r\n\t\t\t\t\t<tipoImpRet>01</tipoImpRet>\r\n\t\t\t\t</ImpRetDet>\r\n\t\t\t</ImpRet>\r\n\t\t</Det>\r\n\t</Detalle>\r\n\t<Impuestos>\r\n\t\t<Imp>\r\n\t\t\t<idImpuesto>1</idImpuesto>\r\n\t\t\t<baseimpuestos>{subtotal}</baseimpuestos>\r\n\t\t\t<tasa>0.00</tasa>\r\n\t\t\t<tipoImpuesto>01</tipoImpuesto>\r\n\t\t\t<importe>0.00</importe>\r\n\t\t</Imp>\r\n\t</Impuestos>\r\n{descuentoCuerpo}</Factura>";
        private string facturaCanastillaString = "<Factura xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\n         xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n\t<Encabezado>\r\n\t\t<ordenCompra/>\r\n\t\t<nitemisor>{nit}</nitemisor>\r\n\t\t<tipoOpera>10</tipoOpera>\r\n\t\t<tipocomprobante>01</tipocomprobante>\r\n\t\t<noresolucion>{resolucion}</noresolucion>\r\n\t\t<prefijo>{prefijo}</prefijo>\r\n\t\t<folio>{numero}</folio>\r\n\t\t<fecha>{fecha}</fecha>\r\n\t\t<hora>{hora}</hora>\r\n\t\t<fechavencimiento>{fecha}</fechavencimiento>\r\n\t\t<moneda>COP</moneda>\r\n\t\t<xslt>1</xslt>\r\n\t\t<montoletra>{montoletra}</montoletra>\r\n\t\t<terminospago>30</terminospago>\r\n\t\t<mediospago>\r\n\t\t\t<Mp>\r\n\t\t\t\t<mediopago>{formapago}</mediopago>\r\n\t\t\t\t<fechapago>{fecha}</fechapago>\r\n\t\t\t</Mp>\r\n\t\t</mediospago>\r\n\t\t<tipoDocRec>{tipoDocumento}</tipoDocRec>\r\n\t\t<tiporeceptor>{tiporeceptor}</tiporeceptor>\r\n\t\t<nitreceptor>{nitreceptor}</nitreceptor>\r\n\t\t{digitoverificacion}\r\n\t\t<nombrereceptor>{nombre}</nombrereceptor>\r\n\t\t<paisreceptor>CO</paisreceptor>\r\n\t\t<obligacionesfiscalesreceptor>{obligacionfiscal}</obligacionesfiscalesreceptor>\r\n\t\t<tributoreceptor>{tributoreceptor}</tributoreceptor>\r\n\t\t<regimenreceptor>{regimen}</regimenreceptor>\r\n\t\t<mailreceptor>{mail}</mailreceptor>\r\n\t\t<metodopago>{metodopago}</metodopago>\r\n\t\t<subtotal>{subtotal}</subtotal>\r\n\t\t<baseimpuesto>{subtotal}</baseimpuesto>\r\n\t\t<totalsindescuento>{totalmenosdescuento}</totalsindescuento>\r\n\t\t<totaldescuentos>{descuento}</totaldescuentos>\r\n\t\t<totalimpuestos>{totalimpuesto}</totalimpuestos>\r\n\t\t<totalimpuestosretenidos>{totalimpuesto}</totalimpuestosretenidos>\r\n\t\t<total>{total}</total>\r\n\t\t<codigodepartamento>11</codigodepartamento>\r\n\t\t<codigociudadreceptor>11001</codigociudadreceptor>\r\n\t\t<direccionreceptor>CALLE 1 # 1-2</direccionreceptor>\r\n\t\t<extra1>{vendedor}</extra1>\r\n\t\t<extra2>{venta}</extra2>\r\n\t\t<extra3>{placa}</extra3>\r\n\t\t<extra4>{kilometraje}</extra4>\r\n\t\t<extra5>{codigointerno}</extra5>\r\n\t\t<extra6>{surtidor}</extra6>\r\n\t\t<extra7>{cara}</extra7>\r\n\t\t<extra8>{manguera}</extra8>\r\n\t</Encabezado>\r\n\t<Detalle>\r\n\t\t{detalle}\r\n\t</Detalle>\r\n\t<Impuestos>\r\n\t\t{impuestos}\r\n\t</Impuestos>\r\n{descuentoCuerpo}</Factura>";
        private string detalle = "<Det>\r\n\t\t\t<idConcepto>{idarticulo}</idConcepto>\r\n\t\t\t<cantidad>{cantidad}</cantidad>\r\n\t\t\t<unidadmedida>{unidadmedida}</unidadmedida>\r\n\t\t\t<importe>{subtotal}</importe>\r\n\t\t\t<descripcion>{articulo}</descripcion>\r\n\t\t\t<precioUnitario>{precio}</precioUnitario>\r\n\t\t\t<identificacionproductos>{producto}</identificacionproductos>\r\n\t\t\t<SubIdentiproductos>0</SubIdentiproductos>\r\n\t\t\t<extra1>{Exento}</extra1>\r\n\t\t\t<ImpRet>\r\n\t\t\t\t<ImpRetDet>\r\n\t\t\t\t\t<baseimpuestos>{subtotal}</baseimpuestos>\r\n\t\t\t\t\t<ImporteImpRet>{impuesto}</ImporteImpRet>\r\n\t\t\t\t\t<tasaImpRet>{tasa}</tasaImpRet>\r\n\t\t\t\t\t<tipoImpRet>01</tipoImpRet>\r\n\t\t\t\t</ImpRetDet>\r\n\t\t\t</ImpRet>\r\n\t\t</Det>";
        private string impuesto = "<Imp>\r\n\t\t\t<idImpuesto>1</idImpuesto>\r\n\t\t\t<baseimpuestos>{subtotal}</baseimpuestos>\r\n\t\t\t<tasa>{tasa}</tasa>\r\n\t\t\t<tipoImpuesto>01</tipoImpuesto>\r\n\t\t\t<importe>{totalimpuesto}</importe>\r\n\t\t</Imp>";
        private string descuentoCuerpo = "<DesCargFac>\r\n\t\t<DesCarg>\r\n\t\t\t<id>FV</id>\r\n\t\t\t<CodDesCar>01</CodDesCar>\r\n\t\t\t<indicador>2</indicador>\r\n\t\t\t<razon>Descuento cliente frecuente</razon>\r\n\t\t\t<porcentaje>{porcentaje}</porcentaje>\r\n\t\t\t<base>{subtotal}</base>\r\n\t\t\t<importe>{descuento}</importe>\r\n\t\t</DesCarg>\r\n\t</DesCargFac>";
        private readonly Alegra alegraOptions;
        private readonly ContactsHandler contactsHandler;
        private readonly InvoiceHandler invoiceHandler;
        private readonly ItemHandler itemHandler;
        private readonly ResolucionNumber _resolucionNumber;
        private readonly IResolucionRepositorio _resolucionRepositorio;
        private static readonly Semaphore _semaphore = new(initialCount: 1, maximumCount: 1);

        public FacturacionFactura1(IOptions<Alegra> alegra, ResolucionNumber resolucionNumber, IResolucionRepositorio resolucionRepositorio)
        {
            alegraOptions = alegra.Value;

            _resolucionNumber = resolucionNumber;

            _resolucionRepositorio = resolucionRepositorio;
        }
        public Task ActualizarTercero(Modelo.Tercero t, string idFacturacion)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerarFacturaElectronica(Modelo.Factura factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            throw new NotImplementedException();
        }

        public async Task<Factura1> GetFacturaDataico(Modelo.OrdenDeDespacho factura, Modelo.Tercero tercero, string estacion, ResolucionFacturaElectronica resolucion)
        {
            var numero = resolucion.numeroActual;
            var formaPagoPrincipal = GetPrimaryPaymentForm(factura.FormaDePago, factura.FormaDePago2, factura.Total1, factura.Total2);
            var detallePago = BuildPaymentDetail(factura.FormaDePago, factura.FormaDePago2, factura.Total1, factura.Total2);

            var nombre = "";
            var apellido = "";
            var nombreCompleto = tercero?.Nombre?.Trim();
            if (tercero == null || string.IsNullOrEmpty(tercero.Apellidos) || tercero.Apellidos.ToLower().Contains("no informado"))
            {
                if(string.IsNullOrEmpty(nombreCompleto))
                {

                    nombre = "no informado";
                    apellido = "no informado";
                }
                else
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
            }
            else
            {
                nombre = nombreCompleto;
                apellido = tercero.Apellidos;
            }
            var subtotal = factura.Cantidad * factura.Precio;
            var total = subtotal-double.Parse(factura.Descuento.ToString());

            var facturaParticular = facturaString.Replace("{nit}", alegraOptions.Nit).Replace("{nitreceptor}", factura.Identificacion)
                .Replace("{resolucion}", resolucion.resolucion)
                .Replace("{prefijo}", resolucion.prefijo)
                .Replace("{numero}", resolucion.numeroActual.ToString())
                .Replace("{fecha}", factura.Fecha.ToString("yyyy-MM-dd"))
                .Replace("{hora}", factura.Fecha.ToString("HH:mm:ss"));
            facturaParticular = facturaParticular.Replace("{mail}", string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.ToLower().Contains("no informado") ? alegraOptions.Correo : tercero.Correo);
            facturaParticular = facturaParticular.Replace("{subtotal}", subtotal.ToString("F2",
                  CultureInfo.InvariantCulture));
            facturaParticular = facturaParticular.Replace("{descuento}", factura.Descuento.ToString("F2",
                  CultureInfo.InvariantCulture));
            facturaParticular = facturaParticular.Replace("{total}", total.ToString("F2",
                  CultureInfo.InvariantCulture)); 
            facturaParticular = facturaParticular.Replace("{totalmenosdescuento}", subtotal.ToString("F2",
                  CultureInfo.InvariantCulture));
            facturaParticular = facturaParticular.Replace("{vendedor}", factura.Vendedor?.ToString());
            facturaParticular = facturaParticular.Replace("{venta}", $"{factura.IdVentaLocal} {detallePago}".Trim());
            facturaParticular = facturaParticular.Replace("{placa}", factura.Placa?.ToString());
            facturaParticular = facturaParticular.Replace("{kilometraje}", factura.Kilometraje?.ToString());
            facturaParticular = facturaParticular.Replace("{codigointerno}", factura.IdInterno?.ToString());
            facturaParticular = facturaParticular.Replace("{surtidor}", factura.Surtidor?.ToString());
            facturaParticular = facturaParticular.Replace("{cara}", factura.Cara?.ToString());
            facturaParticular = facturaParticular.Replace("{manguera}", factura.Manguera?.ToString());
            facturaParticular = facturaParticular.Replace("{cantidad}", factura.Cantidad.ToString("F3",
                  CultureInfo.InvariantCulture));
            facturaParticular = facturaParticular.Replace("{unidadmedida}", GetUnidadMedida(factura.Combustible));
            facturaParticular = facturaParticular.Replace("{precio}", factura.Precio.ToString("F2",
                  CultureInfo.InvariantCulture));
            facturaParticular = facturaParticular.Replace("{producto}", "7702345");
            facturaParticular = facturaParticular.Replace("{tipoDocumento}", GetTipoDocumento(tercero.DescripcionTipoIdentificacion));
            facturaParticular = facturaParticular.Replace("{tiporeceptor}", GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion));
            facturaParticular = facturaParticular.Replace("{nombre}", nombreCompleto.Replace("&", "&amp;"));
            facturaParticular = facturaParticular.Replace("{obligacionfiscal}", "R-99-PN");
            facturaParticular = facturaParticular.Replace("{tributoreceptor}", "ZZ");
            facturaParticular = facturaParticular.Replace("{regimen}", "49");
            facturaParticular = facturaParticular.Replace("{formapago}", GetPaymentType(formaPagoPrincipal));
            facturaParticular = facturaParticular.Replace("{metodopago}", "1");
            facturaParticular = facturaParticular.Replace("{montoletra}", total.ConvertNumeroALetras());
            facturaParticular = facturaParticular.Replace("{combustible}", factura.Combustible);
            facturaParticular = facturaParticular.Replace("{digitoverificacion}", GetDigitoVerificaicon(tercero, factura.Identificacion));
            if (factura.Descuento > 0)
            {
                var porcentajeDescuento = double.Parse(factura.Descuento.ToString()) * 100/subtotal;
                var descuentoAAgregar = descuentoCuerpo.Replace("{porcentaje}", porcentajeDescuento.ToString("F2",
                  CultureInfo.InvariantCulture))
                    .Replace("{descuento}", factura.Descuento.ToString("F2",
                  CultureInfo.InvariantCulture)).Replace("{subtotal}", subtotal.ToString("F2",
                  CultureInfo.InvariantCulture));
                facturaParticular = facturaParticular.Replace("{descuentoCuerpo}", descuentoAAgregar);
            }
            else
            {
                facturaParticular = facturaParticular.Replace("{descuentoCuerpo}", "");
            }
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(facturaParticular);
            var facturaBase64 = System.Convert.ToBase64String(plainTextBytes);
            return new Factura1()
            {
              contrasena = alegraOptions.Contrasena,
              usuario = alegraOptions.Usuario,
              sucursal = "",
              base64doc = facturaBase64
            };
        }

        private string GetDigitoVerificaicon(Modelo.Tercero tercero, string identificacion)
        {
            if (tercero.DescripcionTipoIdentificacion == "Nit")
            {
                return $"<digitoverificacion>{CalcularDigitoVerificacion(identificacion)}</digitoverificacion>";
            }
            else
            {
                return "<digitoverificacion/>";
            }
        }

        private string CalcularDigitoVerificacion(string identificacion)
        {
            var digito = 0;
            int[] nums = { 3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71 };
            for (int i = 0; i < identificacion.Length; i++)
            {
                digito += nums[i] * int.Parse(identificacion[identificacion.Length - 1 - i].ToString());
            }
            var value= (digito % 11);
            if(value > 1)
            {
                return (11 - value).ToString();
            } else
            {
                return value.ToString();
            }
        }

        private string GetPaymentType(string formaDePago)
        {
            var forma = (formaDePago ?? string.Empty).ToLower();

            if (forma.Contains("dé") && forma.Contains("tran"))
            {
                return "47";
            }
            else if(forma.Contains("dé") && forma.Contains("tar"))
            {
                return "49";
            }
            else if(forma.Contains("cré") && forma.Contains("ban") && forma.Contains("tran"))
            {
                return "45";
            }
            else if (forma.Contains("cré") && forma.Contains("tran"))
            {
                return "30";
            }
            else if (forma.Contains("cré") && forma.Contains("tar"))
            {
                return "48";
            }
            else if (forma.Contains("ban") && forma.Contains("cons"))
            {
                return "42";
            }
            else
            {
                return "10";
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
                return $"Pago:{forma1}";
            }

            return $"Pagos:{forma1}={(total1 ?? 0m):0.##},{forma2}={(total2 ?? 0m):0.##}";
        }

        private string GetTipoDocumento(string descripcionTipoIdentificacion)
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

        private string GetTipoIdentificacion(string descripcionTipoIdentificacion)
        {
            if (descripcionTipoIdentificacion == "Nit")
            {
                return "1";
            }
            else
            {
                return "2";
            }
        }
        private string GetUnidadMedida(string combustible)
        {
             if (combustible.ToLower().Contains("gas") || combustible.ToLower().Contains("gnvc"))
            {
                return "MTQ";
            }
            else
            {
                return "A76";
            }
        }

        public async Task<string> GenerarFacturaElectronica(Modelo.OrdenDeDespacho orden, Modelo.Tercero tercero, Guid estacionGuid)
        {
            string responseBody = "";
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
            var invoice = await GetFacturaDataico(orden, tercero, estacionGuid.ToString(), resolucion);
            _semaphore.WaitOne();
            try
            {


                var token = await GetToken(resolucion);

                using (var client = new HttpClient())
                {
                    try
                    {

                        client.Timeout = new TimeSpan(0, 0, 2, 0, 0);
                        client.DefaultRequestHeaders.Add("Authorization", token);
                        var content = new StringContent(JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await client.PostAsync($"{alegraOptions.Url}v3/factura", content);
                        responseBody = await response.Content.ReadAsStringAsync();


                        var respuesta = JsonConvert.DeserializeObject<RespuestaFactura1>(responseBody);
                        if (!string.IsNullOrWhiteSpace(respuesta.error))
                        {
                            if(respuesta.error.Contains("Factura existente con los criterios NoResolucion"))
                            {

                                resolucion.numeroActual += 1;
                                invoice = await GetFacturaDataico(orden, tercero, estacionGuid.ToString(), resolucion);
                                
                                content = new StringContent(JsonConvert.SerializeObject(invoice,
                                    Newtonsoft.Json.Formatting.None,
                                    new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore
                                    }));
                                response = await client.PostAsync($"{alegraOptions.Url}v3/factura", content);
                                responseBody = await response.Content.ReadAsStringAsync();


                                respuesta = JsonConvert.DeserializeObject<RespuestaFactura1>(responseBody);
                                if (string.IsNullOrWhiteSpace(respuesta.error))
                                {
                                    await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), resolucion.numeroActual + 1);

                                    return "Ok:" + resolucion.prefijo + resolucion.numeroActual + ":" + respuesta.cufe + ":" + responseBody + ":" + JsonConvert.SerializeObject(invoice);

                                }
                                else
                                {
                                    await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), resolucion.numeroActual);

                                }
                            }
                            return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                        }
                        else
                        {
                            await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), resolucion.numeroActual + 1);

                            return "Ok:" + resolucion.prefijo + resolucion.numeroActual + ":" + respuesta.cufe+":"+ responseBody+":"+ JsonConvert.SerializeObject(invoice);

                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        return "error:" + ex.Message + ex.StackTrace + responseBody + JsonConvert.SerializeObject(invoice);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                return "error:" + ex.Message + ex.StackTrace + responseBody + JsonConvert.SerializeObject(invoice);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<string> GetToken(ResolucionFacturaElectronica resolucion)
        {
            try
            {
                var tokenrequets = new Factura1TokenRequest
                {
                    username = alegraOptions.Usuario,
                    password = alegraOptions.Contrasena

                };
                Console.WriteLine(JsonConvert.SerializeObject(tokenrequets));

                using (var client = new HttpClient())
                {
                    try
                    {

                        var content = new StringContent(JsonConvert.SerializeObject(tokenrequets));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        Console.WriteLine($"{alegraOptions.Url}authV2");
                        var response = await client.PostAsync($"{alegraOptions.Url}authV2", content);
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync();
                        var respuestaSiigo = JsonConvert.DeserializeObject<Factura1TokenResponse>(responseBody);

                        return respuestaSiigo.Token;
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

        public async Task<string> GenerarFacturaElectronica(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, Guid estacionGuid)
        {
            if (factura.canastillas == null || !factura.canastillas.Any())
            {
                return "error:Factura canastilla sin artículos (items vacíos)";
            }
            string responseBody = "";
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacionGuid.ToString());
            var invoice = await GetFacturaDataico(factura, tercero, estacionGuid.ToString(), resolucion);
            _semaphore.WaitOne();
            try
            {


                var token = await GetToken(resolucion);

                using (var client = new HttpClient())
                {
                    try
                    {

                        client.Timeout = new TimeSpan(0, 0, 2, 0, 0);
                        client.DefaultRequestHeaders.Add("Authorization", token);
                        var content = new StringContent(JsonConvert.SerializeObject(invoice,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }));
                        content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        var response = await client.PostAsync($"{alegraOptions.Url}v3/factura", content);
                        responseBody = await response.Content.ReadAsStringAsync();


                        var respuesta = JsonConvert.DeserializeObject<RespuestaFactura1>(responseBody);
                        if (!string.IsNullOrWhiteSpace(respuesta.error))
                        {
                            if (respuesta.error.Contains("Factura existente con los criterios NoResolucion"))
                            {

                                resolucion.numeroActual += 1;
                                invoice = await GetFacturaDataico(factura, tercero, estacionGuid.ToString(), resolucion);

                                content = new StringContent(JsonConvert.SerializeObject(invoice,
                                    Newtonsoft.Json.Formatting.None,
                                    new JsonSerializerSettings
                                    {
                                        NullValueHandling = NullValueHandling.Ignore
                                    }));
                                response = await client.PostAsync($"{alegraOptions.Url}v3/factura", content);
                                responseBody = await response.Content.ReadAsStringAsync();


                                respuesta = JsonConvert.DeserializeObject<RespuestaFactura1>(responseBody);
                                if (string.IsNullOrWhiteSpace(respuesta.error))
                                {
                                    await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), resolucion.numeroActual + 1);

                                    return "Ok:" + resolucion.prefijo + resolucion.numeroActual + ":" + respuesta.cufe + ":" + responseBody + ":" + JsonConvert.SerializeObject(invoice);

                                }
                                else
                                {
                                    await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), resolucion.numeroActual);

                                }
                            }
                            return "error:" + responseBody + JsonConvert.SerializeObject(invoice);
                        }
                        else
                        {
                            await _resolucionRepositorio.SetFacturaelectronicaPorPRefijo(estacionGuid.ToString(), resolucion.numeroActual + 1);

                            return "Ok:" + resolucion.prefijo + resolucion.numeroActual + ":" + respuesta.cufe + ":" + responseBody + ":" + JsonConvert.SerializeObject(invoice);

                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine(ex);
                        Console.WriteLine(ex.StackTrace);
                        return "error:" + ex.Message + ex.StackTrace + responseBody + JsonConvert.SerializeObject(invoice);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);
                return "error:" + ex.Message + ex.StackTrace + responseBody + JsonConvert.SerializeObject(invoice);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task<Factura1> GetFacturaDataico(Modelo.FacturaCanastilla factura, Modelo.Tercero tercero, string v, ResolucionFacturaElectronica resolucion)
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
            var subtotal = Math.Round(factura.subtotal,2);
            double total = Math.Round(factura.total, 2);
            double totalmenosdescuento = Math.Round(factura.total-factura.descuento, 2);
            var facturaParticular = facturaCanastillaString.Replace("{nit}", alegraOptions.Nit).Replace("{nitreceptor}", factura.terceroId.Identificacion)
                .Replace("{resolucion}", resolucion.resolucion)
                .Replace("{prefijo}", resolucion.prefijo)
                .Replace("{numero}", resolucion.numeroActual.ToString())
                .Replace("{fecha}", factura.fecha.ToString("yyyy-MM-dd"))
                .Replace("{hora}", factura.fecha.ToString("HH:mm:ss"));
            facturaParticular = facturaParticular.Replace("{mail}", string.IsNullOrEmpty(tercero.Correo) || tercero.Correo.ToLower().Contains("no informado") ? alegraOptions.Correo : tercero.Correo);
            facturaParticular = facturaParticular.Replace("{subtotal}", subtotal.ToString("F2",
                  CultureInfo.InvariantCulture));
            facturaParticular = facturaParticular.Replace("{descuento}", factura.descuento.ToString("F2",
                  CultureInfo.InvariantCulture));
            facturaParticular = facturaParticular.Replace("{total}", total.ToString("F2",
                  CultureInfo.InvariantCulture));
            facturaParticular = facturaParticular.Replace("{totalmenosdescuento}", totalmenosdescuento.ToString("F2",
                  CultureInfo.InvariantCulture));
            facturaParticular = facturaParticular.Replace("{vendedor}", "Vendedor");
            facturaParticular = facturaParticular.Replace("{venta}", (factura.FacturasCanastillaId).ToString());
            facturaParticular = facturaParticular.Replace("{placa}", "placa");
            facturaParticular = facturaParticular.Replace("{kilometraje}", "kilometraje");
            facturaParticular = facturaParticular.Replace("{codigointerno}", "codigo interno");
            facturaParticular = facturaParticular.Replace("{surtidor}","surtidor");
            facturaParticular = facturaParticular.Replace("{cara}", "Cara");
            facturaParticular = facturaParticular.Replace("{manguera}", "Manguera");
            facturaParticular = facturaParticular.Replace("{producto}", "7702345");
            facturaParticular = facturaParticular.Replace("{tipoDocumento}", GetTipoDocumento(tercero.DescripcionTipoIdentificacion));
            facturaParticular = facturaParticular.Replace("{tiporeceptor}", GetTipoIdentificacion(tercero.DescripcionTipoIdentificacion));
            facturaParticular = facturaParticular.Replace("{nombre}", nombreCompleto.Replace("&", "&amp;"));
            facturaParticular = facturaParticular.Replace("{obligacionfiscal}", "R-99-PN");
            facturaParticular = facturaParticular.Replace("{tributoreceptor}", "ZZ");
            facturaParticular = facturaParticular.Replace("{regimen}", "49");
            facturaParticular = facturaParticular.Replace("{formapago}", GetPaymentType(factura.codigoFormaPago.Descripcion));
            facturaParticular = facturaParticular.Replace("{metodopago}", "1");
            facturaParticular = facturaParticular.Replace("{montoletra}", total.ConvertNumeroALetras());
            facturaParticular = facturaParticular.Replace("{digitoverificacion}", GetDigitoVerificaicon(tercero, factura.terceroId.Identificacion));

            var detalles = "";
            var cant = 1;
            var sinImpuesto = false;
            var conImpuesto = true;
            var subtotalsinimpuesto = 0f;
            var subtotalconimpuesto = 0f;
            var ivatotal = 0f;
            foreach(var articulo in factura.canastillas)
            {
                var ivaArticulo = (float)Math.Round(articulo.iva, 2);
                var subTotalArticulo = (float)Math.Round(articulo.subtotal, 2);
                var det = detalle;
                det = det.Replace("{idarticulo}", cant.ToString());
                det = det.Replace("{cantidad}", articulo.cantidad.ToString("F3",
                      CultureInfo.InvariantCulture));
                det = det.Replace("{unidadmedida}", "LTR");
                det = det.Replace("{subtotal}", subTotalArticulo.ToString("F2",
                      CultureInfo.InvariantCulture));
                det = det.Replace("{articulo}", articulo.Canastilla.descripcion);
                det = det.Replace("{precio}", articulo.Canastilla.precio.ToString("F2",
                      CultureInfo.InvariantCulture));
                det = det.Replace("{product}", articulo.Canastilla.CanastillaId.ToString());
                det = det.Replace("{Exento}", ivaArticulo > 0?"":"EXENTO");
                det = det.Replace("{impuesto}", ivaArticulo.ToString("F2",
                      CultureInfo.InvariantCulture));
                det = det.Replace("{tasa}", GetTasa(articulo).ToString("F2",
                      CultureInfo.InvariantCulture));
                cant++;
                detalles += det;
                if( articulo.iva > 0) {
                    conImpuesto = true;
                    subtotalconimpuesto = subTotalArticulo;
                    ivatotal = ivaArticulo;
                } else
                {
                    sinImpuesto = true;
                    subtotalsinimpuesto = subTotalArticulo;
                }
            }
            var impuestos = "";
            if (conImpuesto)
            {

                var impuestoParticular = impuesto.Replace("{subtotal}", subtotalconimpuesto.ToString("F2",
                      CultureInfo.InvariantCulture));
                impuestoParticular = impuestoParticular.Replace("{tasa}", "19.00");
                impuestos += impuestoParticular.Replace("{totalimpuesto}", ivatotal.ToString("F2",
                      CultureInfo.InvariantCulture));
            }
            if (sinImpuesto)
            {

                var impuestoParticular = impuesto.Replace("{subtotal}", subtotalsinimpuesto.ToString("F2",
                      CultureInfo.InvariantCulture));
                impuestoParticular = impuestoParticular.Replace("{tasa}", "0");
                impuestos += impuestoParticular.Replace("{totalimpuesto}", "0.00");
            }
            facturaParticular = facturaParticular.Replace("{impuestos}", impuestos);
            facturaParticular = facturaParticular.Replace("{detalle}", detalles);
            facturaParticular = facturaParticular.Replace("{totalimpuesto}", ivatotal.ToString("F2",
                  CultureInfo.InvariantCulture));

            if (factura.descuento > 0)
            {
                var porcentajeDescuento = double.Parse(factura.descuento.ToString()) * 100 / subtotal;
                var descuentoAAgregar = descuentoCuerpo.Replace("{porcentaje}", porcentajeDescuento.ToString("F2",
                  CultureInfo.InvariantCulture))
                    .Replace("{descuento}", factura.descuento.ToString("F2",
                  CultureInfo.InvariantCulture)).Replace("{subtotal}", subtotal.ToString("F2",
                  CultureInfo.InvariantCulture));
                facturaParticular = facturaParticular.Replace("{descuentoCuerpo}", descuentoAAgregar);
            }
            else
            {
                facturaParticular = facturaParticular.Replace("{descuentoCuerpo}", "");
            }
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(facturaParticular);
            var facturaBase64 = System.Convert.ToBase64String(plainTextBytes);
            return new Factura1()
            {
                contrasena = alegraOptions.Contrasena,
                usuario = alegraOptions.Usuario,
                sucursal = "",
                base64doc = facturaBase64
            };
        }

        private float GetTasa(Modelo.CanastillaFactura articulo)
        {
            return articulo.iva > 0 ? 19:0;
        }

        public Task<string> GenerarFacturaElectronica(List<Modelo.OrdenDeDespacho> ordenes, Modelo.Tercero tercero, IEnumerable<Item> items)
        {
            throw new NotImplementedException();
        }

        public Task<string> GenerarFacturaElectronica(List<Modelo.Factura> facturas, Modelo.Tercero tercero, IEnumerable<Item> items)
        {
            throw new NotImplementedException();
        }

        public Task<int> GenerarTercero(Modelo.Tercero tercero)
        {
            throw new NotImplementedException();
        }

        public Task<ResponseInvoice> GetFacturaElectronica(string id)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetFacturaElectronica(string id, Guid estacionGuid)
        {
            return null;
        }

        public Task<Item> GetItem(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<string> getJson(Modelo.OrdenDeDespacho orden, Guid estacion)
        {
            var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion.ToString());

            var factura = await GetFacturaDataico(orden, orden.Tercero, estacion.ToString(), resolucion);

            Console.WriteLine(JsonConvert.SerializeObject(factura));
            return JsonConvert.SerializeObject(factura);
        }

        public Task<ResolucionElectronica> GetResolucionElectronica(string estacion)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TerceroResponse>> GetTerceros(int start)
        {
            throw new NotImplementedException();
        }

        public Task<Item> GetItem(string name, Alegra options)
        {
            throw new NotImplementedException();
        }

        public async Task<string> ReenviarFactura(Repositorio.Entities.OrdenDeDespacho orden, Guid estacion)
        {
           
                string responseBody = "";
                var resolucion = await _resolucionRepositorio.GetFacturaelectronicaPorPRefijo(estacion.ToString());
                var invoice = orden.idFacturaElectronica.Split(":")[4];
                _semaphore.WaitOne();
                try
                {


                    var token = await GetToken(resolucion);

                    using (var client = new HttpClient())
                    {
                        try
                        {

                            client.Timeout = new TimeSpan(0, 0, 2, 0, 0);
                            client.DefaultRequestHeaders.Add("Authorization", token);
                            var content = new StringContent(invoice);
                            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                            var response = await client.PostAsync($"{alegraOptions.Url}v3/factura", content);
                            responseBody = await response.Content.ReadAsStringAsync();


                            var respuesta = JsonConvert.DeserializeObject<RespuestaFactura1>(responseBody);
                            return "Ok:" + resolucion.prefijo + resolucion.numeroActual + ":" + respuesta?.cufe + ":" + responseBody + ":" + invoice;

                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine(ex);
                            Console.WriteLine(ex.StackTrace);
                            return "error:" + ex.Message + ex.StackTrace + responseBody + invoice;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine(ex.StackTrace);
                    return "error:" + ex.Message + ex.StackTrace + responseBody + invoice;
                }
                finally
                {
                    _semaphore.Release();
                }
            
        }

        public Task<string> getJsonCanastilla(Modelo.FacturaCanastilla facturaCanastilla, Guid estacio)
        {
            throw new NotImplementedException();
        }
    }
}
