using FactoradorEstacionesModelo;
using FactoradorEstacionesModelo.Fidelizacion;
using FacturadorEstacionesPOSWinForm;
using FacturadorEstacionesPOSWinForm.Repo;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Options;
using Modelo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Text.RegularExpressions;
using FactoradorEstacionesModelo.Siges;
using FactoradorEstacionesModelo.Objetos;
using SigesServicio;
using System.Runtime.CompilerServices;

namespace EnviadorInformacionService.Contabilidad
{
    public class SiesaWorker : BackgroundService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly InfoEstacion _infoEstacion;
        private readonly Siesa _siesa;
        private readonly InformacionCuenta _informacionCuenta;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly IFidelizacion _fidelizacon;
        private readonly HashSet<int> _formasPagoCaja;


        public override void Dispose()
        {
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        public SiesaWorker(IEstacionesRepositorio estacionesRepositorio, IOptions<InformacionCuenta> informacionCuenta, IOptions<InfoEstacion> infoEstacion, IOptions<Siesa> siesa, IConexionEstacionRemota conexionEstacionRemota, IFidelizacion fidelizacon)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _infoEstacion = infoEstacion.Value;
            _informacionCuenta = informacionCuenta.Value;
            _conexionEstacionRemota = conexionEstacionRemota;
            _fidelizacon = fidelizacon;
            _siesa = siesa.Value;
            _formasPagoCaja = ParseFormasPagoCaja(_siesa.FormasPagoCaja);
            Logger.Info($"Regla caja/banco SiesaWorker. Formas de pago para caja: {string.Join(",", _formasPagoCaja.OrderBy(x => x))}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Run(async () =>
            {
                Logger.Info("Iniciando interfaz Siesa");
                var fechaMinimaEnvioSiesa = ObtenerFechaMinimaEnvioSiesa(_siesa.FechaMinimaEnvioSiesa, "Siesa.FechaMinimaEnvioSiesa");
                var fechaMaximaEnvioSiesa = ObtenerFechaMaximaEnvioSiesa(_siesa.FechaMaximaEnvioSiesa, "Siesa.FechaMaximaEnvioSiesa");
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {

                        var facturas = _estacionesRepositorio
                            .BuscarFacturasNoEnviadasSiesa(fechaMinimaEnvioSiesa, fechaMaximaEnvioSiesa)
                            .ToList();
                        if (fechaMinimaEnvioSiesa.HasValue || fechaMaximaEnvioSiesa.HasValue)
                        {
                            var fechaInicioLog = fechaMinimaEnvioSiesa?.ToString("yyyy-MM-dd HH:mm:ss") ?? "sin límite";
                            var fechaFinalLog = fechaMaximaEnvioSiesa?.ToString("yyyy-MM-dd HH:mm:ss") ?? "sin límite";
                            Logger.Info($"Consulta de facturas Siesa con filtro de fechas. Inicio: {fechaInicioLog}, Final: {fechaFinalLog}, Facturas obtenidas: {facturas.Count}");
                        }

                        var terceros = facturas
                            .Select(x => x.Tercero)
                            .Where(t => t != null)
                            .GroupBy(t => t.terceroId)
                            .Select(g => g.First())
                            .ToList();

                        Logger.Info("facturas a procesar: " + facturas.Count());
                        Logger.Info("terceros a procesar: " + terceros.Count());
                        if (terceros.Any(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                        {
                            var tercerosEnviados = new List<int>();
                            var tercerosFallidos = new List<string>();

                            foreach (var t in terceros.Where(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                            {
                                if (string.IsNullOrWhiteSpace(t.identificacion))
                                {
                                    tercerosFallidos.Add($"ID: {t.terceroId}, Identificación vacía");
                                    Logger.Warn($"Tercero omitido por identificación vacía - ID: {t.terceroId}, Nombre: {t.Nombre}");
                                    continue;
                                }

                                if (await EnviarTercero(t))
                                {
                                    tercerosEnviados.Add(t.terceroId);
                                    Logger.Info($"Tercero enviado exitosamente - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                                }
                                else
                                {
                                    tercerosFallidos.Add($"ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                                    Logger.Info($"Fallo al enviar tercero - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                                }
                            }

                            if (tercerosEnviados.Any())
                            {
                                _estacionesRepositorio.MarcarTercerosEnviadosASiesa(tercerosEnviados);
                                Logger.Info($"Total terceros enviados exitosamente: {tercerosEnviados.Count} - IDs: {string.Join(", ", tercerosEnviados)}");
                            }

                            if (tercerosFallidos.Any())
                            {
                                Logger.Info($"Total terceros que fallaron al enviar: {tercerosFallidos.Count} - {string.Join(" | ", tercerosFallidos)}");
                            }
                        }
                        var facturasEnviadas = new List<int>();
                        var facturasFallidas = new List<string>();

                        foreach (var factura in facturas)
                        {
                            try
                            {
                                Logger.Info("enviando factura a Siesa - ID: " + factura.ventaId);
                                var infoTemp = "";
                                var facelec = "";

                                if (!EsFormaPagoExcluidaSiesa(factura.codigoFormaPago))
                                {
                                    try
                                    {
                                        var intentos = 0;
                                        do
                                        {
                                            try
                                            {
                                                infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(factura.ventaId, Guid.Parse(_infoEstacion.EstacionFuente), _conexionEstacionRemota.getToken());
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
                                            infoTemp = infoTemp.Replace("\n\r", " ");

                                            var facturaElectronica = infoTemp.Split(' ');
                                            if (facturaElectronica.Length < 5)
                                            {
                                                throw new InvalidOperationException($"Formato inesperado de info factura electrónica para venta {factura.ventaId}. Valor: {infoTemp}");
                                            }

                                            Match match = Regex.Match(facturaElectronica[2], @"^([A-Za-z]+)(\d+)$");
                                            facelec = facturaElectronica[4];
                                            if (match.Success)
                                            {
                                                string letras = match.Groups[1].Value;
                                                string numeros = match.Groups[2].Value;

                                                string auxiliarContable = LimpiarTexto(_estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Combustible, true, true));
                                                string auxiliarCruce = LimpiarTexto(_estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Combustible, true, false));

                                                // Obtener fecha de facturación desde Dataico
                                                try
                                                {
                                                    Logger.Warn($"Intento de busqueda factura {facturaElectronica[2]}.");
                                                    string dataicoToken = _infoEstacion.DataicoToken;
                                                    string url = $"https://api.dataico.com/dataico_api/v2/invoices?number={facturaElectronica[2]}";
                                                    int maxRetries = 10;
                                                    int retryCount = 0;
                                                    bool success = false;
                                                    System.Net.Http.HttpResponseMessage response = null;
                                                    while (retryCount < maxRetries && !success)
                                                    {
                                                        using (var client = new HttpClient())
                                                        {
                                                            try
                                                            {
                                                                var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.dataico.com/dataico_api/v2/invoices?number={facturaElectronica[2]}");
                                                                request.Headers.Add("auth-token", dataicoToken);
                                                                response = client.SendAsync(request).Result;
                                                                if (response.IsSuccessStatusCode)
                                                                {
                                                                    success = true;
                                                                    break;
                                                                }
                                                                else
                                                                {
                                                                    Logger.Warn($"Intento {retryCount + 1}: No se pudo obtener la fecha de facturación desde Dataico para factura {facturaElectronica[2]}. Código: {response.StatusCode}");
                                                                }
                                                            }
                                                            catch (Exception exHttp)
                                                            {
                                                                Logger.Error($"Intento {retryCount + 1}: Error al llamar a Dataico para factura {facturaElectronica[2]}: {exHttp.Message}");
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
                                                            if (string.IsNullOrWhiteSpace(json))
                                                            {
                                                                Logger.Error($"Respuesta vacía de Dataico para factura {facturaElectronica[2]}");
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
                                                                throw new Exception("El objeto JSON de Dataico o la propiedad 'invoice' es null");
                                                            }
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
                                                            // factura.descuento = descuento;
                                                            // factura.total = total;
                                                            // factura.subtotal = subtotal;
                                                            Logger.Info($"Factura {facturaElectronica[2]} Dataico: fecha={fechaFacturacion}, subtotal={subtotal}, total={total}, descuento={descuento}");
                                                        }
                                                        else
                                                        {
                                                            Logger.Warn($"No se pudo obtener la fecha de facturación desde Dataico para factura {facturaElectronica[2]} después de {maxRetries} intentos.");
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {

                                                    Logger.Error($"Error al obtener la fecha de facturación desde Dataico: {ex.Message}{ex.StackTrace}");
                                                }


                                                if (string.IsNullOrWhiteSpace(auxiliarContable))
                                                {
                                                    Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Combustible} no se envió no exite auxiliar contrable creado");
                                                }
                                                if (string.IsNullOrWhiteSpace(auxiliarCruce))
                                                {
                                                    Logger.Info($"Factura {factura.ventaId} con forma de pago {factura.codigoFormaPago} y combustible {factura.Combustible} no se envió no exite auxiliar cruce creado");
                                                }

                                                if (string.IsNullOrWhiteSpace(factura?.Tercero?.identificacion))
                                                {
                                                    throw new InvalidOperationException($"Factura {factura.ventaId} no se puede enviar: identificación de tercero vacía.");
                                                }

                                                if (string.IsNullOrWhiteSpace(auxiliarContable) || string.IsNullOrWhiteSpace(auxiliarCruce))
                                                {
                                                    throw new InvalidOperationException($"Factura {factura.ventaId} no se puede enviar: auxiliar contable/cruce no configurado para formaPago={factura.codigoFormaPago}, combustible='{factura.Combustible}'.");
                                                }

                                                await EnviarFactura(factura, facturaElectronica[2], numeros, auxiliarContable, auxiliarCruce);
                                                //_apiContabilidad.EnviarRecibo(factura, facturaElectronica[2], numeros, _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, true), _estacionesRepositorio.ObtenerAuxiliarContable(factura.codigoFormaPago, factura.Venta.Combustible, true, false));
                                                facturasEnviadas.Add(factura.ventaId);
                                                Logger.Info($"Factura enviada exitosamente - ID: {factura.ventaId}, Total: {factura.TOTALCalculado}, Forma Pago: {factura.codigoFormaPago}, Combustible: {factura.Combustible}");
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.TOTALCalculado}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                                        Logger.Warn($"Fallo al enviar factura - ID: {factura.ventaId}, Total: {factura.TOTALCalculado}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                                    }
                                }
                                else
                                {
                                    Logger.Info($"Factura {factura.ventaId} omitida de envío a Siesa por forma de pago excluida: {factura.codigoFormaPago}");
                                    facturasEnviadas.Add(factura.ventaId);
                                }
                            }
                            catch (Exception ex)
                            {
                                facturasFallidas.Add($"ID: {factura.ventaId}, Total: {factura.TOTALCalculado}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                                Logger.Warn($"Fallo al procesar factura - ID: {factura.ventaId}, Total: {factura.TOTALCalculado}, Forma Pago: {factura.codigoFormaPago}, Error: {ex.Message}");
                            }
                        }
                        if (facturasEnviadas.Any())
                        {
                            Logger.Info($"Total facturas enviadas exitosamente: {facturasEnviadas.Count} - IDs: {string.Join(", ", facturasEnviadas)}");
                            _estacionesRepositorio.ActuralizarFacturasEnviadosSiesa(facturasEnviadas);
                        }

                        if (facturasFallidas.Any())
                        {
                            Logger.Warn($"Total facturas que fallaron al enviar: {facturasFallidas.Count} - {string.Join(" | ", facturasFallidas)}");
                        }

                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "Error en ciclo principal de SiesaWorker");
                        Thread.Sleep(5000);
                    }
                }
            });
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
                Logger.Info($"Filtro de fecha máxima Siesa activo ({origenConfig}): {fechaCorte:yyyy-MM-dd HH:mm:ss}");
                return fechaCorte;
            }

            if (DateTime.TryParse(fechaConfig.Trim(), out fechaCorte))
            {
                Logger.Info($"Filtro de fecha máxima Siesa activo ({origenConfig}): {fechaCorte:yyyy-MM-dd HH:mm:ss}");
                return fechaCorte;
            }

            Logger.Warn($"No se pudo interpretar {origenConfig}='{fechaConfig}'. Se ignora filtro por fecha máxima de envío a Siesa.");
            return null;
        }

        private async Task EnviarFactura(FacturaSiges factura, string facturaelectronica, string consecutivo, string? auxiliarContable, string? auxiliarCruce)
        {
            var contentString = "";
            var responseString = "";
            var pagos = ConstruirPagosFactura(factura);

            if (pagos.Count > 1)
            {
                var requestContent = ConvertirAMovimientoSiesaMultipago(factura, facturaelectronica, consecutivo, auxiliarContable, auxiliarCruce, pagos);
                contentString = JsonConvert.SerializeObject(requestContent);
                Logger.Info($"Factura {factura.ventaId} enviada con multipago Siesa: {string.Join(", ", pagos.Select(x => $"forma {x.FormaPagoId}={x.Valor.ToString("0.00", CultureInfo.InvariantCulture)} ({(EsFormaPagoEfectivo(x.FormaPagoId) ? "caja" : "banco")})"))}");
            }
            else if (EsFormaPagoEfectivo(factura.codigoFormaPago))
            {
                // Pago en efectivo - usar formato con Caja
                var requestContent = ConvertirAMovimientoSiesaCaja(factura, facturaelectronica, consecutivo, auxiliarContable, auxiliarCruce);
                contentString = JsonConvert.SerializeObject(requestContent);
            }
            else
            {
                // Otros métodos de pago - usar formato sin Caja
                var requestContent = ConvertirAMovimientoSiesa(factura, facturaelectronica, consecutivo, auxiliarContable, auxiliarCruce);
                contentString = JsonConvert.SerializeObject(requestContent);
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_siesa.UrlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={_siesa.IdCompania}&idSistema={_siesa.Idsistema}&idDocumento={_siesa.IdDocumento}&nombreDocumento=Documento_Contablev2");
                    request.Headers.Add("ConniKey", _siesa.KeySiesa);
                    request.Headers.Add("ConniToken", _siesa.Tokensiesa);
                    request.Content = new StringContent(contentString, Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request);
                    responseString = await response.Content.ReadAsStringAsync();

                    // Si es Bad Request, verificar si el documento ya existe
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        if (responseString.Contains("El documento ya existe"))
                        {
                            Logger.Info($"Factura ya existe en Siesa (marcada como exitosa) - {contentString}. Respuesta: {responseString}");
                            return; // Salir sin lanzar excepción, se considera exitosa
                        }
                        else
                        {
                            Logger.Warn($"Factura no enviada (Bad Request) - {contentString}. Respuesta: {responseString}");
                            throw new HttpRequestException($"Bad Request: {responseString}");
                        }
                    }

                    response.EnsureSuccessStatusCode();
                    Logger.Info($"Factura enviada {contentString}. Respuesta {responseString}");
                }
            }
            catch (Exception ex)
            {
                if (responseString.Contains("El documento ya existe"))
                {
                    Logger.Info($"Factura ya existe en Siesa (marcada como exitosa) - {contentString}. Respuesta: {responseString}");
                    return; // Salir sin lanzar excepción, se considera exitosa
                }
                else
                {
                    Logger.Info($"Factura no enviada {contentString}. Respuesta {responseString}. Error: {ex.Message}");
                    throw;
                }
            }
        }

        private object ConvertirAMovimientoSiesaMultipago(FacturaSiges factura, string facturaelectronica, string consecutivo, string? auxiliarContable, string? auxiliarCruce, List<PagoFacturaSiesa> pagos)
        {
            var combustible = ObtenerCombustibleSeguro(factura);
            var identificacion = ObtenerIdentificacionTerceroSeguro(factura);
            var movimientos = new List<object>
            {
                new
                {
                    F_CIA = "1",
                    F350_ID_CO = _siesa.CentroOperaciones,
                    F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_ID_AUXILIAR = auxiliarContable ?? string.Empty,
                    F351_ID_TERCERO = identificacion,
                    F351_ID_CO_MOV = _siesa.Movimiento,
                    F351_ID_UN = _siesa.UnidadNegocio,
                    F351_ID_CCOSTO = _siesa.CentroCosto,
                    F351_ID_FE = consecutivo,
                    F351_VALOR_DB = "0",
                    F351_VALOR_CR = factura.TOTALCalculado.ToString("0.00", CultureInfo.InvariantCulture),
                    F351_BASE_GRAVABLE = string.Empty,
                    F351_DOCTO_BANCO = string.Empty,
                    F351_NRO_DOCTO_BANCO = string.Empty,
                    F351_NOTAS = $"Factura combustible {combustible} id local {consecutivo}"
                }
            };

            if (factura.Descuento > 0)
            {
                movimientos.Add(new
                {
                    F_CIA = "1",
                    F350_ID_CO = _siesa.CentroOperacionesContableDescuento ?? "101",
                    F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_ID_AUXILIAR = _siesa.AuxiliarDescuento ?? "58904001",
                    F351_ID_TERCERO = identificacion,
                    F351_ID_CO_MOV = _siesa.MovimientoContableDescuento ?? "101",
                    F351_ID_UN = _siesa.UnidadNegocioDescuento ?? "03",
                    F351_ID_CCOSTO = _siesa.CentroCostoDescuento ?? "0203",
                    F351_ID_FE = _siesa.IdFeDescuento ?? "1",
                    F351_VALOR_DB = factura.Descuento.ToString("0.00", CultureInfo.InvariantCulture),
                    F351_VALOR_CR = string.Empty,
                    F351_BASE_GRAVABLE = "1",
                    F351_DOCTO_BANCO = string.Empty,
                    F351_NRO_DOCTO_BANCO = string.Empty,
                    F351_NOTAS = $"FAC {consecutivo} DESCUENTO PROMOCIÓN",
                    F351_ID_SUCURSAL = _siesa.Sucursal ?? "001"
                });
            }

            foreach (var pagoBanco in pagos.Where(x => !EsFormaPagoEfectivo(x.FormaPagoId)))
            {
                movimientos.Add(new
                {
                    F_CIA = "1",
                    F350_ID_CO = _siesa.CentroOperacionesOtros ?? string.Empty,
                    F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_ID_AUXILIAR = auxiliarCruce ?? string.Empty,
                    F351_ID_TERCERO = string.Empty,
                    F351_ID_CO_MOV = _siesa.MovimientoOtros ?? string.Empty,
                    F351_ID_UN = _siesa.UnidadNegocioOtros ?? string.Empty,
                    F351_ID_CCOSTO = _siesa.CentroCostoOtros ?? string.Empty,
                    F351_ID_FE = _siesa.IdFeOtros ?? string.Empty,
                    F351_VALOR_DB = pagoBanco.Valor.ToString("0.00", CultureInfo.InvariantCulture),
                    F351_VALOR_CR = "0",
                    F351_BASE_GRAVABLE = string.Empty,
                    F351_DOCTO_BANCO = "CG",
                    F351_NRO_DOCTO_BANCO = factura.fecha.ToString("yyyyMMdd"),
                    F351_NOTAS = $"Factura combustible {combustible} id local {consecutivo} forma {pagoBanco.FormaPagoId}"
                });
            }

            var caja = pagos
                .Where(x => EsFormaPagoEfectivo(x.FormaPagoId))
                .Select(x => new Caja
                {
                    F_CIA = "1",
                    F350_ID_CO = _siesa.CentroOperacionesCaja,
                    F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_NOTAS = $"Venta combustible forma {x.FormaPagoId}",
                    F351_ID_AUXILIAR = auxiliarCruce ?? string.Empty,
                    F351_ID_CCOSTO = _siesa.CentroCostoCaja,
                    F351_ID_CO_MOV = _siesa.MovimientoCaja,
                    F351_ID_UN = _siesa.UnidadNegocioCaja,
                    F351_VALOR_CR = "0",
                    F351_VALOR_DB = x.Valor.ToString("0.00", CultureInfo.InvariantCulture),
                    F351_ID_FE = _siesa.IdFe,
                    F358_COD_SEGURIDAD = string.Empty,
                    F358_FECHA_VCTO = factura.fecha.ToString("yyyyMMdd"),
                    F358_ID_CAJA = _siesa.Caja,
                    F358_ID_MEDIOS_PAGO = ObtenerMedioPagoSiesa(x.FormaPagoId),
                    F358_NOTAS = $"Factura combustible {combustible} id local {consecutivo} forma {x.FormaPagoId}",
                    F358_NRO_AUTORIZACION = string.Empty,
                    F358_NRO_CUENTA = auxiliarCruce ?? string.Empty,
                    F358_REFERENCIA_OTROS = string.Empty
                })
                .ToList();

            return new
            {
                Inicial = new List<object> { new { F_CIA = "1" } },
                Final = new List<object> { new { F_CIA = "1" } },
                Caja = caja,
                Documentocontable = new List<object> { new {
                    F_CIA = "1",
                    F_CONSEC_AUTO_REG = _siesa.ConsecutivoAutoRegulado,
                    F350_ID_CO = _siesa.CentroOperacionesDocumento,
                    F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                    F350_CONSEC_DOCTO = consecutivo,
                    F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                    F350_ID_TERCERO = identificacion,
                    F350_IND_ESTADO = "1",
                    F350_NOTAS = $"Factura combustible {combustible} id local {consecutivo}",
                }},
                Movimientocontable = movimientos
            };
        }

        private object ConvertirAMovimientoSiesa(FacturaSiges factura, string facturaelectronica, string consecutivo, string? auxiliarContable, string? auxiliarCruce)
        {
            var combustible = ObtenerCombustibleSeguro(factura);
            var identificacion = ObtenerIdentificacionTerceroSeguro(factura);
            var movimientos = new List<object>();
            // Primer movimiento contable
            movimientos.Add(new
            {
                F_CIA = "1",
                F350_ID_CO = _siesa.CentroOperacionesContableOtros ?? "",
                F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                F350_CONSEC_DOCTO = consecutivo,
                F351_ID_AUXILIAR = auxiliarContable ?? "",
                F351_ID_TERCERO = identificacion,
                F351_ID_CO_MOV = _siesa.MovimientoContableOtros ?? "",
                F351_ID_UN = _siesa.UnidadNegocioContableOtros ?? "",
                F351_ID_CCOSTO = _siesa.CentroCostoContableOtros ?? "",
                F351_ID_FE = "",
                F351_VALOR_DB = "0",
                F351_VALOR_CR = factura.TOTALCalculado.ToString("0", CultureInfo.InvariantCulture),
                F351_BASE_GRAVABLE = "",
                F351_DOCTO_BANCO = "",
                F351_NRO_DOCTO_BANCO = "",
                F351_NOTAS = $"Factura combustible {combustible} id local {consecutivo}"
            });
            // Segundo movimiento contable
            movimientos.Add(new
            {
                F_CIA = "1",
                F350_ID_CO = _siesa.CentroOperacionesOtros ?? "",
                F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                F350_CONSEC_DOCTO = consecutivo,
                F351_ID_AUXILIAR = auxiliarCruce ?? "",
                F351_ID_TERCERO = "",
                F351_ID_CO_MOV = _siesa.MovimientoOtros ?? "",
                F351_ID_UN = _siesa.UnidadNegocioOtros ?? "",
                F351_ID_CCOSTO = _siesa.CentroCostoOtros ?? "",
                F351_ID_FE = _siesa.IdFeOtros ?? "",
                F351_VALOR_DB = factura.TOTALCalculado.ToString("0", CultureInfo.InvariantCulture),
                F351_VALOR_CR = "0",
                F351_BASE_GRAVABLE = "",
                F351_DOCTO_BANCO = "CG",
                F351_NRO_DOCTO_BANCO = factura.fecha.ToString("yyyyMMdd"),
                F351_NOTAS = $"Factura combustible {combustible} id local {consecutivo}"
            });
            // Movimiento de descuento si aplica
            if (factura.Descuento != null && factura.Descuento > 0)
            {
                movimientos.Add(new
                {
                    F_CIA = "1",
                    F350_ID_CO = _siesa.CentroOperacionesContableDescuento ?? "101",
                    F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_ID_AUXILIAR = _siesa.AuxiliarDescuento ?? "58904001",
                    F351_ID_TERCERO = identificacion,
                    F351_ID_CO_MOV = _siesa.MovimientoContableDescuento ?? "101",
                    F351_ID_UN = _siesa.UnidadNegocioDescuento ?? "03",
                    F351_ID_CCOSTO = _siesa.CentroCostoDescuento ?? "0203",
                    F351_ID_FE = _siesa.IdFeDescuento ?? "1",
                    F351_VALOR_DB = factura.Descuento.ToString("0", CultureInfo.InvariantCulture),
                    F351_VALOR_CR = "",
                    F351_BASE_GRAVABLE = "1",
                    F351_DOCTO_BANCO = "",
                    F351_NRO_DOCTO_BANCO = "",
                    F351_NOTAS = $"FAC {consecutivo} DESCUENTO PROMOCIÓN",
                    F351_ID_SUCURSAL = _siesa.Sucursal ?? "001"
                });
            }
            var requestContent = new
            {
                Inicial = new List<object> { new { F_CIA = "1" } },
                Documentocontable = new List<object> { new {
                    F_CIA = "1",
                    F_CONSEC_AUTO_REG = _siesa.ConsecutivoAutoRegulado,
                    F350_ID_CO = _siesa.CentroOperacionesDocumento,
                    F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                    F350_CONSEC_DOCTO = consecutivo,
                    F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                    F350_ID_TERCERO = identificacion,
                    F350_IND_ESTADO = "1",
                    F350_NOTAS = $"Factura combustible {combustible} id local {consecutivo}",
                }},
                Movimientocontable = movimientos,
                Final = new List<object> { new { F_CIA = "1" } }
            };
            return requestContent;
        }

        private MovimientosCaja ConvertirAMovimientoSiesaCaja(FacturaSiges factura, string facturaelectronica, string consecutivo, string? auxiliarContable, string? auxiliarCruce)
        {
            var combustible = ObtenerCombustibleSeguro(factura);
            var identificacion = ObtenerIdentificacionTerceroSeguro(factura);
            var requestContent = new MovimientosCaja()
            {
                Inicial = new List<Compania> { new Compania() { F_CIA = "1" } },
                Final = new List<Compania> { new Compania() { F_CIA = "1" } },
                Caja = new List<Caja> {
                new Caja {

                        F_CIA = "1",
                        F350_ID_CO = _siesa.CentroOperacionesCaja,
                        F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_NOTAS = "Venta combustible",
                        F351_ID_AUXILIAR = auxiliarCruce ?? "",
                        F351_ID_CCOSTO = _siesa.CentroCostoCaja,
                        F351_ID_CO_MOV = _siesa.MovimientoCaja,
                        F351_ID_UN = _siesa.UnidadNegocioCaja,
                        F351_VALOR_CR = "0",
                        F351_VALOR_DB = factura.TOTALCalculado.ToString("0.00", CultureInfo.InvariantCulture),
                        F351_ID_FE = _siesa.IdFe,
                        F358_COD_SEGURIDAD = "",
                        F358_FECHA_VCTO = factura.fecha.ToString("yyyyMMdd"),
                        F358_ID_CAJA = _siesa.Caja,
                        F358_ID_MEDIOS_PAGO = "EFE",
                        F358_NOTAS = $"Factura combustible {combustible} id local {consecutivo}",
                        F358_NRO_AUTORIZACION="",
                        F358_NRO_CUENTA=auxiliarCruce ?? "",
                        F358_REFERENCIA_OTROS=""



                }
                },
                Documentocontable = new List<Documentocontable> { new Documentocontable() {
                F_CIA = "1",
                F_CONSEC_AUTO_REG = _siesa.ConsecutivoAutoRegulado,
                F350_ID_CO = _siesa.CentroOperacionesDocumento,
                F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                F350_CONSEC_DOCTO = consecutivo,
                F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                F350_ID_TERCERO = identificacion,
                F350_IND_ESTADO = "1",
                F350_NOTAS = $"Factura combustible {combustible} id local {consecutivo}",


                }
               },
                Movimientocontable = new List<Movimientocontable>()
                {
                    new Movimientocontable()
                    {
                        F_CIA = "1",
                        F350_ID_CO = _siesa.CentroOperaciones,
                        F350_ID_TIPO_DOCTO = _siesa.DocumentoFactura,
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_BASE_GRAVABLE = "",
                        F351_NOTAS = $"Factura combustible {combustible} id local {consecutivo}",
                        F351_DOCTO_BANCO = "",
                        F351_ID_TERCERO = identificacion,
                        F351_ID_AUXILIAR = auxiliarContable ?? "",
                        F351_ID_CCOSTO = _siesa.CentroCosto,
                        F351_ID_CO_MOV = _siesa.Movimiento,
                        F351_ID_FE = consecutivo,
                        F351_NRO_DOCTO_BANCO="",
                        F351_ID_UN = _siesa.UnidadNegocio,
                        F351_VALOR_CR = factura.TOTALCalculado.ToString("0.00", CultureInfo.InvariantCulture),
                        F351_VALOR_DB = "0",



                    }

                },

            };
            return requestContent;
        }

        private List<PagoFacturaSiesa> ConstruirPagosFactura(FacturaSiges factura)
        {
            var totalFactura = decimal.Round(Convert.ToDecimal(factura.TOTALCalculado), 2);
            var valorPago2 = factura.codigoFormaPago2.HasValue && factura.total2.HasValue && factura.total2.Value > 0
                ? decimal.Round(Convert.ToDecimal(factura.total2.Value), 2)
                : 0m;
            var valorPago1 = factura.total1.HasValue && factura.total1.Value > 0
                ? decimal.Round(Convert.ToDecimal(factura.total1.Value), 2)
                : decimal.Round(Math.Max(0m, totalFactura - valorPago2), 2);

            if (valorPago1 + valorPago2 > totalFactura && totalFactura > 0)
            {
                valorPago1 = decimal.Round(Math.Max(0m, totalFactura - valorPago2), 2);
            }

            var pagos = new List<PagoFacturaSiesa>();
            if (factura.codigoFormaPago > 0 && valorPago1 > 0)
            {
                pagos.Add(new PagoFacturaSiesa
                {
                    FormaPagoId = factura.codigoFormaPago,
                    Valor = valorPago1
                });
            }

            if (factura.codigoFormaPago2.HasValue && factura.codigoFormaPago2.Value > 0 && valorPago2 > 0)
            {
                pagos.Add(new PagoFacturaSiesa
                {
                    FormaPagoId = factura.codigoFormaPago2.Value,
                    Valor = valorPago2
                });
            }

            if (!pagos.Any())
            {
                pagos.Add(new PagoFacturaSiesa
                {
                    FormaPagoId = factura.codigoFormaPago,
                    Valor = totalFactura
                });
            }

            return pagos
                .GroupBy(x => x.FormaPagoId)
                .Select(g => new PagoFacturaSiesa
                {
                    FormaPagoId = g.Key,
                    Valor = decimal.Round(g.Sum(x => x.Valor), 2)
                })
                .Where(x => x.Valor > 0)
                .ToList();
        }

            private static string ObtenerCombustibleSeguro(FacturaSiges factura)
            {
                return (factura?.Combustible ?? string.Empty).Trim();
            }

            private static string ObtenerIdentificacionTerceroSeguro(FacturaSiges factura)
            {
                return factura?.Tercero?.identificacion?.ToString() ?? string.Empty;
            }

        private bool EsFormaPagoEfectivo(int formaPagoId)
        {
            return _formasPagoCaja.Contains(formaPagoId);
        }

        private string ObtenerMedioPagoSiesa(int formaPagoId)
        {
            return EsFormaPagoEfectivo(formaPagoId) ? "EFE" : "OTR";
        }

        private static HashSet<int> ParseFormasPagoCaja(string? formasPagoCajaConfig)
        {
            var valores = string.IsNullOrWhiteSpace(formasPagoCajaConfig) ? "4" : formasPagoCajaConfig;

            var parsed = valores
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .ToHashSet();

            if (!parsed.Any())
            {
                parsed.Add(4);
            }

            return parsed;
        }

        private static string LimpiarTexto(string? valor)
        {
            return (valor ?? string.Empty)
                .Replace("\r\n", string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Trim();
        }

        private static bool EsFormaPagoExcluidaSiesa(int codigoFormaPago)
        {
            return codigoFormaPago == 6 || codigoFormaPago == 10 || codigoFormaPago == 98;
        }

        private sealed class PagoFacturaSiesa
        {
            public int FormaPagoId { get; init; }
            public decimal Valor { get; init; }
        }

        private async Task<bool> EnviarTercero(Tercero t)
        {
            var requestContent = ConvertirATercero(t);
            var responseString = "";

            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{_siesa.UrlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={_siesa.IdCompania}&idSistema={_siesa.Idsistema}&idDocumento={_siesa.IdDOcumentoCliente}&nombreDocumento=TERCERO_CLIENTE_INTEGRADO");
                    request.Headers.Add("ConniKey", _siesa.KeySiesa);
                    request.Headers.Add("ConniToken", _siesa.Tokensiesa);
                    request.Content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request);
                    responseString = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();

                    Logger.Info($"Tercero enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                    return true;
                }
            }
            catch (Exception ex)
            {

                if (ex is HttpRequestException && responseString.Contains("No tiene acceso a modificar"))
                {
                    Logger.Info($"Tercero enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                    return true;
                }

                Logger.Info($"Tercero no enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
                return false;
            }
        }

        private Root ConvertirATercero(Tercero x)
        {
            var nombre = "";
            var apellido = "";
            var nombreCompleto = x?.Nombre?.Trim() ?? "";
            var identificacion = x?.identificacion?.Trim() ?? string.Empty;
            var telefono = (x?.Telefono ?? string.Empty).Trim();
            var telefonoCorto = telefono.Length > 20 ? telefono.Substring(0, 20) : telefono;
            var direccion = (x?.Direccion ?? string.Empty).Trim();
            var direccionCorta = direccion.Length > 40 ? direccion.Substring(0, 40) : direccion;
            var correo = x?.Correo ?? string.Empty;
            var tipoIdentificacion = x?.tipoIdentificacionS ?? string.Empty;

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


            return new Root()
            {
                Inicial = new List<Compania> { new Compania() { F_CIA = "1" } },
                Final = new List<Compania> { new Compania() { F_CIA = "1" } },
                Imptos_Reten = new List<ImptosReten>
                {
                    new ImptosReten
                    {
                        F_TIPO_REG = "46",
                        F_CIA = "1",
                        F_ID_TERCERO= identificacion,
                        F_ID_SUCURSAL = _siesa.Sucursal,
                        F_ID_CLASE = "1",
                        F_ID_VALOR_TERCERO = "1"
                    },

                    new ImptosReten
                    {
                        F_TIPO_REG = "46",
                        F_CIA = "1",
                        F_ID_TERCERO= identificacion,
                        F_ID_SUCURSAL = _siesa.Sucursal,
                        F_ID_CLASE = "2",
                        F_ID_VALOR_TERCERO = "1"
                    }
                },

                Clientes = new List<ClienteSiesa> {
                    new ClienteSiesa
                    {
                        F_CIA = "1",
                        F201_ID_TERCERO = identificacion,
                        F201_ID_SUCURSAL =_siesa.Sucursal,
                        F201_DESCRIPCION_SUCURSAL = "YAVEGAS",
                        F201_ID_VENDEDOR = "",
                        F201_ID_COND_PAGO = "001",
                        F201_CUPO_CREDITO = "",
                        F201_ID_TIPO_CLI = "0004",
                        F201_ID_LISTA_PRECIO ="",
                        F201_IND_BLOQUEADO = "1",
                        F201_IND_BLOQUEO_CUPO = "0",
                        F201_IND_BLOQUEO_MORA = "0",
                        F201_ID_CO_FACTURA = "001",
                        F015_CONTACTO = "SIGES",
                        F015_DIRECCION1 = "1",
                        F015_DIRECCION2 = "1",
                        F015_DIRECCION3 = "1",
                        F015_ID_PAIS = "169",
                        F015_ID_DEPTO="05",
                        F015_ID_CIUDAD = "001",
                            F015_TELEFONO = telefonoCorto,
                        F015_EMAIL = correo,
                    F201_FECHA_INGRESO = DateTime.Now.ToString("yyyyMMdd"),
                    F201_ID_CO_MOVTO_FACTURA = "",
                    F201_ID_UN_MOVTO_FACTURA = "",
                    f015_celular = "1"
                    }
                },

                Terceros = new List<TerceroSiesa> {
                    new TerceroSiesa
                    {
                        F_CIA = "1",
                        F200_ID = identificacion,
                        F200_NIT = identificacion,
                    F200_ID_TIPO_IDENT = tipoIdentificacion == "Nit" ? "N" : "C",
                    F200_IND_TIPO_TERCERO = tipoIdentificacion == "Nit" ? "2" :"1",
                    F200_RAZON_SOCIAL = nombreCompleto.Length > 40 ? nombreCompleto.Substring(0, 40) : nombreCompleto,
                    F200_APELLIDO1 = apellido,
                    F200_APELLIDO2 = "NA",
                    F200_NOMBRES = nombre,
                    F200_NOMBRE_EST = nombre,
                    F015_CONTACTO = "SIGES",
                        F015_DIRECCION1 = direccionCorta,
                        F015_DIRECCION2 = "",
                        F015_DIRECCION3 = "",
                    F015_ID_PAIS = "169",
                    F015_ID_DEPTO = "05",
                    F015_ID_CIUDAD = "001",
                    F015_TELEFONO = telefonoCorto,
                    F015_EMAIL = correo,
                    F200_FECHA_NACIMIENTO = "20000101",
                    F200_ID_CIIU = "0010",
                    F015_CELULAR = telefonoCorto
                }
                }
            };
        }
    }
}