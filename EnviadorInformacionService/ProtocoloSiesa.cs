using EnviadorInformacionService.Contabilidad;
using EnviadorInformacionService.Models;
using FacturadorEstacionesRepositorio;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EnviadorInformacionService
{
    public class ProtocoloSiesa
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IConexionEstacionRemota _conexionEstacionRemota = new ConexionEstacionRemota();
        
        // Control de rate limiting: máximo 25 facturas por minuto
        private static readonly int MAX_FACTURAS_POR_MINUTO = 25;
        private static readonly Queue<DateTime> facturasTimestamps = new Queue<DateTime>();
        private static readonly object rateLimitLock = new object();
        
        public void Ejecutar()
        {

            var _estacionesRepositorio = new EstacionesRepositorioSqlServer();
            var _apiContabilidad = new ApiSiesa();

            var estacionFuente = new Guid(ConfigurationManager.AppSettings["estacionFuente"]);
            var fechaMinimaEnvioSiesa = ObtenerFechaMinimaEnvioSiesa(ConfigurationManager.AppSettings["FechaMinimaEnvioSiesa"], "appSettings.FechaMinimaEnvioSiesa");
            var fechaMaximaEnvioSiesa = ObtenerFechaMaximaEnvioSiesa(ConfigurationManager.AppSettings["FechaMaximaEnvioSiesa"], "appSettings.FechaMaximaEnvioSiesa");
            Logger.Info("Iniciando interfaz Siesa");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");
            while (true)
            {
                try
                {

                    var facturas = _estacionesRepositorio
                        .BuscarFacturasNoEnviadasSiesa(fechaMinimaEnvioSiesa, fechaMaximaEnvioSiesa)
                        .ToList();
                    Logger.Info($"Facturas obtenidas de getFacturaSinEnviarSiesa: {facturas.Count}");
                    // if (fechaMinimaEnvioSiesa.HasValue)
                    // {
                    //     var facturasBloqueadasPorMinima = facturas.Where(x => EsFacturaMasViejaQueCorte(x.fecha, fechaMinimaEnvioSiesa)).ToList();
                    //     if (facturasBloqueadasPorMinima.Any())
                    //     {
                    //         Logger.Warn($"Se omiten {facturasBloqueadasPorMinima.Count} facturas por ser más antiguas que la fecha mínima de envío a Siesa ({fechaMinimaEnvioSiesa:yyyy-MM-dd HH:mm:ss}). IDs: {string.Join(", ", facturasBloqueadasPorMinima.Select(x => x.ventaId))}");
                    //     }

                    //     facturas = facturas.Where(x => !EsFacturaMasViejaQueCorte(x.fecha, fechaMinimaEnvioSiesa)).ToList();
                    // }

                    // if (fechaMaximaEnvioSiesa.HasValue)
                    // {
                    //     var facturasBloqueadasPorFecha = facturas.Where(x => EsFacturaMasNuevaQueCorte(x.fecha, fechaMaximaEnvioSiesa)).ToList();
                    //     if (facturasBloqueadasPorFecha.Any())
                    //     {
                    //         Logger.Warn($"Se omiten {facturasBloqueadasPorFecha.Count} facturas por ser más nuevas que la fecha máxima de envío a Siesa ({fechaMaximaEnvioSiesa:yyyy-MM-dd HH:mm:ss}). IDs: {string.Join(", ", facturasBloqueadasPorFecha.Select(x => x.ventaId))}");
                    //     }

                    //     facturas = facturas.Where(x => !EsFacturaMasNuevaQueCorte(x.fecha, fechaMaximaEnvioSiesa)).ToList();
                    // }
                    // Logger.Info($"Facturas pendientes por enviar a Siesa después de filtros: {facturas.Count}");

                    var terceros = facturas.Select(x => x.Tercero).GroupBy(t => t.terceroId).Select(g => g.First()).ToList();
                    if (terceros.Any(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                    {
                        var tercerosEnviados = new List<int>();
                        var tercerosFallidos = new List<string>();

                        foreach (var t in terceros.Where(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                        {
                            Logger.Info($"Iniciando envío de tercero - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            if (_apiContabilidad.EnviarTercero(t))
                            {
                                tercerosEnviados.Add(t.terceroId);
                                Logger.Info($"Tercero enviado exitosamente - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            }
                            else
                            {
                                tercerosFallidos.Add($"ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                                Logger.Warn($"Fallo al enviar tercero - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            }
                        }

                        if (tercerosEnviados.Any())
                        {
                            _estacionesRepositorio.MarcarTercerosEnviadosASiesa(tercerosEnviados);
                            Logger.Info($"Total terceros enviados exitosamente: {tercerosEnviados.Count} - IDs: {string.Join(", ", tercerosEnviados)}");
                        }

                        if (tercerosFallidos.Any())
                        {
                            Logger.Warn($"Total terceros que fallaron al enviar: {tercerosFallidos.Count} - {string.Join(" | ", tercerosFallidos)}");
                        }
                    }
                    var facturasEnviadas = new List<int>();
                    var facturasFallidas = new List<string>();

                    foreach (var factura in facturas)
                    {
                        try
                        {
                            var infoTemp = "";
                            var facelec = "";
                            var consecutivo = "";
                            var cantidadRedondeada = Math.Round((double)factura.Venta.CANTIDAD, 2);
                            var precioCalculado = (cantidadRedondeada > 0)
                                ? Math.Round(((double)factura.Venta.TOTAL + (double)factura.Venta.Descuento) / cantidadRedondeada, 2)
                                : 0.0;
                            factura.descuento = factura.Venta.Descuento;
                            factura.total = (decimal)(cantidadRedondeada * precioCalculado) - factura.Venta.Descuento;
                            factura.subtotal = (decimal)cantidadRedondeada * (decimal)precioCalculado;

                            if (factura.codigoFormaPago != 6)
                            {
                                Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Venta.Combustible} Enviandose a Siesa");

                                try
                                {
                                    var intentos = 0;
                                    do
                                    {
                                        try
                                        {
                                            infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(factura.ventaId, estacionFuente, _conexionEstacionRemota.getToken());
                                        }
                                        catch (Exception ex)
                                        {
                                            infoTemp = "";
                                            Logger.Error($"Error al obtener información de la factura electrónica para la factura {factura.ventaId}: {ex.Message}");
                                        }
                                        Thread.Sleep(100);
                                    } while (infoTemp == null || intentos++ < 3);

                                    if (!string.IsNullOrEmpty(infoTemp))
                                    {
                                        // Normalize different newline sequences to spaces so splitting and regexes work reliably
                                        infoTemp = infoTemp.Replace("\r\n", "$").Replace("\n", "$").Replace("\r", "$");
                                        infoTemp = infoTemp.Replace("$$", "$");
                                        //Logger.Info($"Info factura electrónica para factura {factura.ventaId}: {infoTemp}");
                                        var facturaElectronica = infoTemp.Split('$');

                                        // Use trimmed value and ignore case when matching invoice id (letters + digits)
                                        var facturaIdForMatch = facturaElectronica.Length > 2 ? facturaElectronica[1].Trim() : string.Empty;
                                        Match match = Regex.Match(facturaIdForMatch, @"^([A-Za-z]+)(\d+)$", RegexOptions.IgnoreCase);
                                        facelec = facturaElectronica[3].Trim();

                                        // Extraer la fecha de emisión de la factura electrónica desde el texto raw (infoTemp)
                                        // Busca el primer patrón dd/MM/yyyy HH:mm:ss y lo parsea. Si no existe, intenta usar facturaElectronica[6].
                                        DateTime parsedFecha;
                                        // Log facturaElectronica parts to help diagnose parsing issues
                                        //Logger.Info($"facturaElectronica parts: count={facturaElectronica.Length}; [2]='{(facturaElectronica.Length>2?facturaElectronica[1]:"<missing>")}'; [4]='{(facturaElectronica.Length>4?facturaElectronica[3]:"<missing>")}'");
                                        var dateMatch = Regex.Match(infoTemp, @"\b(\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}:\d{2})\b");
                                        if (dateMatch.Success)
                                        {
                                            if (DateTime.TryParseExact(dateMatch.Groups[1].Value, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedFecha))
                                            {
                                                factura.fecha = parsedFecha;
                                            }
                                            else
                                            {
                                                Logger.Warn($"No se pudo parsear la fecha encontrada en factura electrónica: {dateMatch.Groups[1].Value}");
                                            }
                                        }
                                        else if (facturaElectronica.Length > 6)
                                        {
                                            if (DateTime.TryParseExact(facturaElectronica[5], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedFecha))
                                            {
                                                factura.fecha = parsedFecha;
                                            }
                                            else
                                            {
                                                Logger.Warn($"No se pudo parsear la fecha en posicion 5 de facturaElectronica: {facturaElectronica[5]}");
                                            }
                                        }
                                        else
                                        {
                                            Logger.Warn("No se encontró fecha de emisión en factura electrónica; usando la fecha actual.");
                                        }
                                        // Log regex match result for the invoice number
                                        //Logger.Info($"Regex on facturaElectronica[2] -> value='{(facturaElectronica.Length>2?facturaElectronica[1]:"<missing>")}', matchSuccess={match.Success}");
                                        if (match.Success)
                                        {
                                            string letras = match.Groups[1].Value;
                                            string numeros = match.Groups[2].Value;

                                            string auxiliarContable = _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, true).Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
                                            string auxiliarCruce = _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, false).Replace("\r\n", "").Replace("\r", "").Replace("\n", "");

                                            // Dynamic discount auxiliary lookup (after cruce)
                                            string auxiliarDescuento = ObtenerAuxiliarDescuento(factura.codigoFormaPago, factura.Venta.Combustible);

                                            // Obtener fecha de facturación desde Dataico
                                            bool dataicoHasInfo = false;
                                            try
                                            {
                                                Logger.Warn($"Intento de busqueda factura {facturaElectronica[1]}.");
                                                string dataicoToken = ConfigurationManager.AppSettings["dataico_token"];
                                                string url = $"https://api.dataico.com/dataico_api/v2/invoices?number={facturaElectronica[1]}";
                                                int maxRetries = 2;
                                                int retryCount = 0;
                                                bool success = false;
                                                System.Net.Http.HttpResponseMessage response = null;
                                                while (retryCount < maxRetries && !success)
                                                {
                                                    using (var client = new HttpClient())
                                                    {
                                                        try
                                                        {
                                                            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.dataico.com/dataico_api/v2/invoices?number={facturaElectronica[1]}");
                                                            request.Headers.Add("auth-token", dataicoToken);
                                                            response = client.SendAsync(request).Result;
                                                            if (response.IsSuccessStatusCode)
                                                            {
                                                                success = true;
                                                                Logger.Info($"Éxito al obtener la fecha de facturación desde Dataico para factura {facturaElectronica[1]}.");
                                                                
                                                            }
                                                            else
                                                            {
                                                                Logger.Warn($"Intento {retryCount + 1}: No se pudo obtener la fecha de facturación desde Dataico para factura {facturaElectronica[1]}. Código: {response.StatusCode}");
                                                            }
                                                        }
                                                        catch (Exception exHttp)
                                                        {
                                                            Logger.Error($"Intento {retryCount + 1}: Error al llamar a Dataico para factura {facturaElectronica[1]}: {exHttp.Message}");
                                                        }
                                                    }
                                                    retryCount++;
                                                    if (!success && retryCount < maxRetries)
                                                    {
                                                        Thread.Sleep(1000); // Espera 1 segundo antes de reintentar
                                                    }
                                                    if (success && response != null)
                                                    {
                                                        var json = response.Content.ReadAsStringAsync().Result;
                                                        //Logger.Info($"Respuesta de Dataico para factura {facturaElectronica[1]}: {json}");
                                                        if (string.IsNullOrWhiteSpace(json))
                                                        {
                                                            Logger.Error($"Respuesta vacía de Dataico para factura {facturaElectronica[1]}");
                                                            throw new Exception("Respuesta vacía de Dataico");
                                                        }
                                                        dynamic obj = null;
                                                        try
                                                        {
                                                            obj = JsonConvert.DeserializeObject(json);
                                                        }
                                                        catch (Exception exJson)
                                                        {
                                                            Logger.Error($"Error deserializando JSON de Dataico: {exJson.Message}. JSON: {json}");
                                                            throw;
                                                        }
                                                        if (obj == null || obj.invoice == null)
                                                        {
                                                            Logger.Error($"El objeto JSON de Dataico o la propiedad 'invoice' es null. JSON: {json}");
                                                            // no data from Dataico for this invoice
                                                            dataicoHasInfo = false;
                                                            throw new Exception("El objeto JSON de Dataico o la propiedad 'invoice' es null");
                                                        }
                                                        dataicoHasInfo = true;
                                                        string fechaFacturacion = obj.invoice.issue_date;
                                                        // Extraer totales desde items y/o qrcode
                                                        decimal total = 0, subtotal = 0, descuento = 0;
                                                        try
                                                        {
                                                            // Sumar los totales de los items
                                                            if (obj.invoice.items != null && obj.invoice.items.HasValues)
                                                            {
                                                                foreach (var item in obj.invoice.items)
                                                                {
                                                                    decimal itemTotal = 0;
                                                                    decimal itemPrice = 0;
                                                                    decimal itemQty = 0;
                                                                    try { itemPrice = (decimal)item.price; } catch { }
                                                                    try { itemQty = (decimal)item.quantity; } catch { }
                                                                    itemTotal = itemPrice * itemQty;
                                                                    subtotal += itemTotal;
                                                                }
                                                            }
                                                            // Leer total desde qrcode si existe
                                                            string qrcode = obj.invoice.qrcode;
                                                            if (!string.IsNullOrWhiteSpace(qrcode))
                                                            {
                                                                // Buscar ValTolFac y ValFac
                                                                var lines = qrcode.Split('\n');
                                                                foreach (var line in lines)
                                                                {
                                                                    if (line.StartsWith("ValTolFac="))
                                                                    {
                                                                        var val = line.Substring("ValTolFac=".Length);
                                                                        decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out total);
                                                                    }
                                                                    if (line.StartsWith("ValFac="))
                                                                    {
                                                                        var val = line.Substring("ValFac=".Length);
                                                                        decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out subtotal);
                                                                    }
                                                                    if (line.StartsWith("ValOtroIm="))
                                                                    {
                                                                        var val = line.Substring("ValOtroIm=".Length);
                                                                        decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out descuento);
                                                                    }
                                                                }
                                                            }
                                                            // Si no hay qrcode, intentar con los items
                                                            if (total == 0 && subtotal > 0)
                                                                total = subtotal;
                                                        }
                                                        catch (Exception exNum)
                                                        {
                                                            Logger.Error($"Error extrayendo totales de Dataico. Error: {exNum.Message}");
                                                        }
                                                        if (string.IsNullOrWhiteSpace(fechaFacturacion))
                                                        {
                                                            Logger.Error($"Campo issue_date vacío en respuesta de Dataico. JSON: {json}");
                                                            throw new Exception("Campo issue_date vacío en respuesta de Dataico");
                                                        }
                                                        try
                                                        {
                                                            factura.fecha = DateTime.ParseExact(fechaFacturacion, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                                        }
                                                        catch (Exception exFecha)
                                                        {
                                                            Logger.Error($"Error parseando fecha de facturación '{fechaFacturacion}': {exFecha.Message}");
                                                            throw;
                                                        }
                                                        if (descuento > 0){

                                                            factura.descuento = descuento;
                                                            factura.total = total;
                                                            factura.subtotal = subtotal;
                                                        } else{
                                                            factura.total = total;
                                                            factura.subtotal = factura.descuento + total;
                                                        }

                                                        if (auxiliarContable == null)
                                                        {
                                                         Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Venta.Combustible} no se envió no exite auxiliar contrable creado");
                                                         }
                                                        if (auxiliarCruce == null)
                                                        {   
                                                         Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Venta.Combustible} no se envió no exite auxiliar cruce creado");
                                                        }
                                                                          Logger.Info($"Factura {facturaElectronica[1]} Dataico: fecha={fechaFacturacion}, subtotal={subtotal}, total={total}, descuento={descuento}");
                                        
                                                    }
                                                    else
                                                    {
                                                        Logger.Warn($"No se pudo obtener la fecha de facturación desde Dataico para factura {facturaElectronica[1]} después de {maxRetries} intentos.");
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Logger.Error($"Error al obtener la fecha de facturación desde Dataico: {ex.Message}{ex.StackTrace}");
                                            }

                                            // Enviar la factura a Siesa SOLO si Dataico devolvió información válida
                                            if (dataicoHasInfo)
                                            {
                                                try
                                                {
                                                    // Control de rate limiting
                                                    WaitForRateLimit();
                                                    Logger.Info($"Iniciando envío de factura - ID: {factura.ventaId}, Total: {factura.total}, Forma Pago: {factura.codigoFormaPago}, Combustible: {factura.Venta.Combustible}");
                                                    _apiContabilidad.EnviarFactura(factura, facturaElectronica[1], numeros, auxiliarContable, auxiliarCruce, auxiliarDescuento);
                                                    facturasEnviadas.Add(factura.ventaId);
                                                    Logger.Info($"Factura enviada exitosamente - ID: {factura.ventaId}, Total: {factura.total}, Forma Pago: {factura.codigoFormaPago}, Combustible: {factura.Venta.Combustible}");
                                                }
                                                catch (Exception exSend)
                                                {
                                                    facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: {exSend.Message}");
                                                    Logger.Debug($"Fallo al enviar factura - ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: {exSend.Message}");
                                                }
                                            }
                                            else
                                            {
                                                facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: no hubo información de Dataico");
                                                Logger.Debug($"No se envió factura {factura.ventaId} a Siesa porque Dataico no devolvió información.");
                                            }

                                        }
                                        else
                                        {
                                            facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: no tiene factura electrónica");
                                            Logger.Debug($"Fallo al enviar factura - ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: no tiene factura electrónica");

                                        }
                                    }
                                    else
                                    {

                                        facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: no tiene factura electrónica");
                                        Logger.Debug($"Fallo al enviar factura - ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: no tiene factura electrónica");

                                    }

                                }
                                catch (Exception ex)
                                {
                                    facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                                    Logger.Debug($"Fallo al enviar factura - ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                                }
                            }
                            else
                            {
                                facturasEnviadas.Add(factura.ventaId);
                            }
                        }
                        catch (Exception ex)
                        {
                            facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                            Logger.Warn($"Fallo al procesar factura - ID: {factura.ventaId}, Total: {factura.Venta.TOTAL}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                        }
                    }
                    if (facturasEnviadas.Any())
                    {
                        Logger.Info($"Total facturas enviadas exitosamente: {facturasEnviadas.Count} - IDs: {string.Join(", ", facturasEnviadas)}");
                        _estacionesRepositorio.ActuralizarFacturasEnviadosSiesa(facturasEnviadas);
                    }

                    if (facturasFallidas.Any())
                    {
                        Logger.Debug($"Total facturas que fallaron al enviar: {facturasFallidas.Count} - {string.Join(" | ", facturasFallidas)}");
                    }

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {

                    Logger.Info("Ex" + ex.Message);
                    Thread.Sleep(5000);
                }
            }
        }
        /// <summary>
        /// Busca el auxiliar de descuento según forma de pago y combustible.
        /// </summary>
        /// <param name="codigoFormaPago">Código de forma de pago</param>
        /// <param name="combustible">Nombre del combustible</param>
        /// <returns>Auxiliar de descuento</returns>
        private string ObtenerAuxiliarDescuento(int codigoFormaPago, string combustible)
        {
            // Normalizar nombre de combustible
            var combustibleKey = (combustible ?? "").Trim().ToLower();
            if (combustibleKey == "corriente") combustibleKey = "corriente";
            // Puedes agregar más normalizaciones si es necesario

            // Buscar clave específica
            string key = $"auxiliardescuento_{codigoFormaPago}_{combustibleKey}";
            var valor = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrEmpty(valor))
                return valor;
            // Si no existe, usar el general
            return ConfigurationManager.AppSettings["auxiliardescuento"];
        }

        private DateTime? ObtenerFechaMinimaEnvioSiesa(string fechaConfig, string origenConfig)
        {
            if (string.IsNullOrWhiteSpace(fechaConfig))
            {
                return null;
            }

            var formatos = new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss" };
            if (DateTime.TryParseExact(fechaConfig.Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaCorte))
            {
                Logger.Info($"Filtro de fecha mínima Siesa activo ({origenConfig}): {fechaCorte:yyyy-MM-dd HH:mm:ss}");
                return fechaCorte;
            }

            if (DateTime.TryParse(fechaConfig.Trim(), out fechaCorte))
            {
                Logger.Info($"Filtro de fecha mínima Siesa activo ({origenConfig}): {fechaCorte:yyyy-MM-dd HH:mm:ss}");
                return fechaCorte;
            }

            Logger.Warn($"No se pudo interpretar {origenConfig}='{fechaConfig}'. Se ignora filtro por fecha mínima de envío a Siesa.");
            return null;
        }

        private DateTime? ObtenerFechaMaximaEnvioSiesa(string fechaConfig, string origenConfig)
        {
            if (string.IsNullOrWhiteSpace(fechaConfig))
            {
                return null;
            }

            var formatos = new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss" };
            if (DateTime.TryParseExact(fechaConfig.Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaCorte))
            {
                fechaCorte = AjustarFechaMaximaInclusiva(fechaConfig, fechaCorte, origenConfig);
                Logger.Info($"Filtro de fecha máxima Siesa activo ({origenConfig}): {fechaCorte:yyyy-MM-dd HH:mm:ss}");
                return fechaCorte;
            }

            if (DateTime.TryParse(fechaConfig.Trim(), out fechaCorte))
            {
                fechaCorte = AjustarFechaMaximaInclusiva(fechaConfig, fechaCorte, origenConfig);
                Logger.Info($"Filtro de fecha máxima Siesa activo ({origenConfig}): {fechaCorte:yyyy-MM-dd HH:mm:ss}");
                return fechaCorte;
            }

            Logger.Warn($"No se pudo interpretar {origenConfig}='{fechaConfig}'. Se ignora filtro por fecha máxima de envío a Siesa.");
            return null;
        }

        private DateTime AjustarFechaMaximaInclusiva(string fechaConfig, DateTime fechaCorte, string origenConfig)
        {
            var valor = (fechaConfig ?? string.Empty).Trim();
            var fechaSolo = Regex.IsMatch(valor, @"^\d{4}-\d{2}-\d{2}$|^\d{2}/\d{2}/\d{4}$");
            var medianocheExplicita = Regex.IsMatch(valor, @"^(?:\d{4}-\d{2}-\d{2}|\d{2}/\d{2}/\d{4})\s+00:00:00$");

            if (fechaSolo || medianocheExplicita)
            {
                var fechaAjustada = fechaCorte.Date.AddDays(1).AddTicks(-1);
                Logger.Warn($"{origenConfig}='{fechaConfig}' se ajusta a fin de día ({fechaAjustada:yyyy-MM-dd HH:mm:ss}) para evitar excluir facturas del mismo día.");
                return fechaAjustada;
            }

            return fechaCorte;
        }

        private static bool EsFacturaMasNuevaQueCorte(DateTime fechaFactura, DateTime? fechaCorteMaxima)
        {
            return fechaCorteMaxima.HasValue && fechaFactura.Date > fechaCorteMaxima.Value.Date;
        }

        private static bool EsFacturaMasViejaQueCorte(DateTime fechaFactura, DateTime? fechaCorteMinima)
        {
            return fechaCorteMinima.HasValue && fechaFactura.Date < fechaCorteMinima.Value.Date;
        }
        
        /// <summary>
        /// Controla el rate limiting para no exceder 25 facturas por minuto
        /// </summary>
        private void WaitForRateLimit()
        {
            lock (rateLimitLock)
            {
                DateTime now = DateTime.Now;
                DateTime oneMinuteAgo = now.AddMinutes(-1);

                // Eliminar timestamps antiguos (fuera de la ventana de 1 minuto)
                while (facturasTimestamps.Count > 0 && facturasTimestamps.Peek() < oneMinuteAgo)
                {
                    facturasTimestamps.Dequeue();
                }

                // Si ya alcanzamos el límite, esperar hasta que la factura más antigua salga de la ventana
                if (facturasTimestamps.Count >= MAX_FACTURAS_POR_MINUTO)
                {
                    DateTime oldestTimestamp = facturasTimestamps.Peek();
                    TimeSpan waitTime = oldestTimestamp.AddMinutes(1).AddSeconds(1) - now;
                    
                    if (waitTime.TotalSeconds > 0)
                    {
                        int waitSeconds = (int)Math.Ceiling(waitTime.TotalSeconds);
                        Logger.Warn($"⏳ Límite de {MAX_FACTURAS_POR_MINUTO} facturas/minuto alcanzado. Esperando {waitSeconds} segundos...");
                        Thread.Sleep(waitTime);
                        
                        // Limpiar timestamps antiguos después de esperar
                        now = DateTime.Now;
                        oneMinuteAgo = now.AddMinutes(-1);
                        while (facturasTimestamps.Count > 0 && facturasTimestamps.Peek() < oneMinuteAgo)
                        {
                            facturasTimestamps.Dequeue();
                        }
                    }
                }

                // Registrar el nuevo timestamp
                facturasTimestamps.Enqueue(now);
            }
        }
    }

}
