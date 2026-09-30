using EnviadorInformacionService.Contabilidad;
using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FacturadorEstacionesRepositorio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EnviadorInformacionService
{
    public class ProtocoloSiesaCanastilla
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IConexionEstacionRemota _conexionEstacionRemota = new ConexionEstacionRemota();
        
        // Control de rate limiting: máximo 25 facturas por minuto
        private static readonly int MAX_FACTURAS_POR_MINUTO = 25;
        private static readonly Queue<DateTime> facturasTimestamps = new Queue<DateTime>();
        private static readonly object rateLimitLock = new object();

        // Valores por defecto tomados del formato de envío canastilla (FEY) acordado con Siesa.
        // Se pueden sobrescribir desde App.config con las claves indicadas en cada uso.
        private const string AuxiliarLubricantesPorDefecto = "42109003";
        private const string AuxiliarUreaPorDefecto = "42109004";
        private const string AuxiliarIvaPorDefecto = "23450101";
        private const string UnidadNegocioMovimientoPorDefecto = "03";
        private const string IdFeMovimientoPorDefecto = "1";

        public void Ejecutar()
        {
            var _estacionesRepositorio = new EstacionesRepositorioSqlServer();
            var _apiContabilidad = new ApiSiesa();

            var estacionFuente = new Guid(ConfigurationManager.AppSettings["estacionFuente"]);
            var fechaMinimaEnvioSiesa = ObtenerFechaMinimaEnvioSiesa(ConfigurationManager.AppSettings["FechaMinimaEnvioSiesa"], "appSettings.FechaMinimaEnvioSiesa");
            var fechaMaximaEnvioSiesa = ObtenerFechaMaximaEnvioSiesa(ConfigurationManager.AppSettings["FechaMaximaEnvioSiesa"], "appSettings.FechaMaximaEnvioSiesa");
            Logger.Info("Iniciando interfaz Siesa para Canastilla");
            Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");

            while (true)
            {
                try
                {
                    var facturasCanastilla = _estacionesRepositorio.BuscarFacturasCanastillaNoEnviadasSiesa().ToList();
                    if (fechaMinimaEnvioSiesa.HasValue)
                    {
                        var facturasBloqueadasPorMinima = facturasCanastilla.Where(x => EsFacturaMasViejaQueCorte(x.fecha, fechaMinimaEnvioSiesa)).ToList();
                        if (facturasBloqueadasPorMinima.Any())
                        {
                            Logger.Warn($"Canastilla - Se omiten {facturasBloqueadasPorMinima.Count} facturas por ser más antiguas que la fecha mínima de envío a Siesa ({fechaMinimaEnvioSiesa:yyyy-MM-dd HH:mm:ss}). IDs: {string.Join(", ", facturasBloqueadasPorMinima.Select(x => x.FacturasCanastillaId))}");
                        }

                        facturasCanastilla = facturasCanastilla.Where(x => !EsFacturaMasViejaQueCorte(x.fecha, fechaMinimaEnvioSiesa)).ToList();
                    }

                    if (fechaMaximaEnvioSiesa.HasValue)
                    {
                        var facturasBloqueadasPorFecha = facturasCanastilla.Where(x => EsFacturaMasNuevaQueCorte(x.fecha, fechaMaximaEnvioSiesa)).ToList();
                        if (facturasBloqueadasPorFecha.Any())
                        {
                            Logger.Warn($"Canastilla - Se omiten {facturasBloqueadasPorFecha.Count} facturas por ser más nuevas que la fecha máxima de envío a Siesa ({fechaMaximaEnvioSiesa:yyyy-MM-dd HH:mm:ss}). IDs: {string.Join(", ", facturasBloqueadasPorFecha.Select(x => x.FacturasCanastillaId))}");
                        }

                        facturasCanastilla = facturasCanastilla.Where(x => !EsFacturaMasNuevaQueCorte(x.fecha, fechaMaximaEnvioSiesa)).ToList();
                    }

                    // Procesar terceros primero
                    var terceros = facturasCanastilla.Select(x => x.terceroId)
                        .Where(x => x != null)
                        .GroupBy(t => t.terceroId)
                        .Select(g => g.First())
                        .ToList();

                    if (terceros.Any(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                    {
                        var tercerosEnviados = new List<int>();
                        var tercerosFallidos = new List<string>();

                        foreach (var t in terceros.Where(x => !x.EnviadoSiesa.HasValue || !x.EnviadoSiesa.Value))
                        {
                            Logger.Info($"Canastilla - Iniciando envío de tercero - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            if (_apiContabilidad.EnviarTercero(t))
                            {
                                tercerosEnviados.Add(t.terceroId);
                                Logger.Info($"Canastilla - Tercero enviado exitosamente - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            }
                            else
                            {
                                tercerosFallidos.Add($"ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                                Logger.Warn($"Canastilla - Fallo al enviar tercero - ID: {t.terceroId}, Identificación: {t.identificacion}, Nombre: {t.Nombre}");
                            }
                        }

                        if (tercerosEnviados.Any())
                        {
                            _estacionesRepositorio.MarcarTercerosEnviadosASiesa(tercerosEnviados);
                            Logger.Info($"Canastilla - Total terceros enviados exitosamente: {tercerosEnviados.Count} - IDs: {string.Join(", ", tercerosEnviados)}");
                        }

                        if (tercerosFallidos.Any())
                        {
                            Logger.Warn($"Canastilla - Total terceros que fallaron al enviar: {tercerosFallidos.Count} - {string.Join(" | ", tercerosFallidos)}");
                        }
                    }

                    // Procesar facturas canastilla
                    var facturasEnviadas = new List<int>();
                    var facturasFallidas = new List<string>();

                    foreach (var facturaCanastilla in facturasCanastilla)
                    {
                        try
                        {
                            Logger.Info($"Procesando factura canastilla {facturaCanastilla.FacturasCanastillaId} con forma de pago {facturaCanastilla.codigoFormaPago}");

                            // Obtener detalle de la factura canastilla
                            var detalle = _estacionesRepositorio.getFacturaCanatillaDetalle(facturaCanastilla.FacturasCanastillaId);

                            if (detalle == null || !detalle.Any())
                            {
                                Logger.Warn($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} no tiene detalle");
                                continue;
                            }

                            // Determinar si tiene IVA (al menos un item con IVA > 0)
                            bool tieneIva = detalle.Any(d => d.Canastilla != null && d.Canastilla.iva > 0);

                            // Obtener configuración según si tiene IVA o no
                            string sufijo = tieneIva ? "canastillaconiva" : "canastillasiniva";

                            var documentoCruce = ConfigurationManager.AppSettings[$"documentocruce{sufijo}"];
                            var consecutivoAutoregulado = ConfigurationManager.AppSettings[$"consecutivoautoregulado{sufijo}"];
                            var centroOperacionesDocumento = ConfigurationManager.AppSettings[$"centrooperacionesdocuemnto{sufijo}"];
                            var idfe = ConfigurationManager.AppSettings[$"idfe{sufijo}"];
                            var sucursal = ConfigurationManager.AppSettings[$"sucursal{sufijo}"];
                            var idDocumento = ConfigurationManager.AppSettings[$"iddocumento{sufijo}"];
                            var idDocumentoCliente = ConfigurationManager.AppSettings[$"iddocumentocliente{sufijo}"];

                            // Configuración común
                            var urlSiesa = ConfigurationManager.AppSettings["urlsiesa"];
                            var idSistema = ConfigurationManager.AppSettings["idsistema"];
                            var idCompania = ConfigurationManager.AppSettings["idcompania"];
                            var vendedor = ConfigurationManager.AppSettings["vendedor"];

                            Logger.Info($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} - Usando configuración: {(tieneIva ? "CON IVA" : "SIN IVA")}");

                            // Calcular totales
                            decimal subtotal = 0;
                            decimal totalIva = 0;
                            decimal total = 0;

                            foreach (var item in detalle)
                            {
                                if (item.Canastilla != null)
                                {
                                    decimal valorItem = (decimal)(item.cantidad * item.Canastilla.precio);
                                    decimal ivaItem = 0;

                                    if (item.Canastilla.iva > 0)
                                    {
                                        ivaItem = valorItem * (item.Canastilla.iva / 100m);
                                    }

                                    subtotal += valorItem;
                                    totalIva += ivaItem;
                                }
                            }

                            total = subtotal + totalIva;

                            // Crear objeto de factura para Siesa
                            var facturaParaSiesa = new
                            {
                                facturaCanastilla.FacturasCanastillaId,
                                facturaCanastilla.consecutivo,
                                facturaCanastilla.fecha,
                                Tercero = facturaCanastilla.terceroId,
                                facturaCanastilla.codigoFormaPago,
                                facturaCanastilla.codigoFormaPago2,
                                facturaCanastilla.total1,
                                facturaCanastilla.total2,
                                Detalle = detalle,
                                Subtotal = subtotal,
                                TotalIva = totalIva,
                                Total = total,
                                TieneIva = tieneIva,
                                Configuracion = new
                                {
                                    DocumentoCruce = documentoCruce,
                                    ConsecutivoAutoregulado = consecutivoAutoregulado,
                                    CentroOperacionesDocumento = centroOperacionesDocumento,
                                    Idfe = idfe,
                                    Sucursal = sucursal,
                                    IdDocumento = idDocumento,
                                    IdDocumentoCliente = idDocumentoCliente,
                                    UrlSiesa = urlSiesa,
                                    IdSistema = idSistema,
                                    IdCompania = idCompania,
                                    Vendedor = vendedor
                                }
                            };

                            Logger.Info($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} - Subtotal: {subtotal}, IVA: {totalIva}, Total: {total}");

                            // Control de rate limiting
                            WaitForRateLimit();
                            
                            // Aquí se haría el envío a Siesa
                            // Por ahora lo dejamos como placeholder - debes implementar el envío real basado en tu ApiSiesa
                            bool enviado = EnviarFacturaCanastillaASiesa(facturaParaSiesa, _apiContabilidad);

                            if (enviado)
                            {
                                facturasEnviadas.Add(facturaCanastilla.FacturasCanastillaId);
                                Logger.Info($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} enviada exitosamente a Siesa");
                            }
                            else
                            {
                                facturasFallidas.Add($"ID: {facturaCanastilla.FacturasCanastillaId}, Total: {total}");
                                Logger.Debug($"Fallo al enviar factura canastilla {facturaCanastilla.FacturasCanastillaId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            facturasFallidas.Add($"ID: {facturaCanastilla.FacturasCanastillaId}, Error: {ex.Message}");
                            Logger.Error(ex, $"Error procesando factura canastilla {facturaCanastilla.FacturasCanastillaId}");
                        }
                    }

                    // Actualizar facturas enviadas
                    if (facturasEnviadas.Any())
                    {
                        _estacionesRepositorio.ActualizarFacturasCanastillaEnviadasSiesa(facturasEnviadas);
                        Logger.Info($"Total facturas canastilla enviadas exitosamente: {facturasEnviadas.Count} - IDs: {string.Join(", ", facturasEnviadas)}");
                    }

                    if (facturasFallidas.Any())
                    {
                        Logger.Debug($"Total facturas canastilla que fallaron: {facturasFallidas.Count} - {string.Join(" | ", facturasFallidas)}");
                    }

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error en el proceso de Siesa Canastilla");
                    Thread.Sleep(5000);
                }
            }
        }

        /// <summary>
        /// Envía una factura canastilla a Siesa con la estructura del conector Documento_Contablev2
        /// (Inicial, Documentocontable, Movimientocontable, Caja, Final).
        /// </summary>
        /// <param name="facturaCanastilla">Objeto con la información de la factura</param>
        /// <param name="apiContabilidad">API de contabilidad Siesa</param>
        /// <returns>True si se envió exitosamente</returns>
        private bool EnviarFacturaCanastillaASiesa(dynamic facturaCanastilla, ApiSiesa apiContabilidad)
        {
            try
            {
                Logger.Info($"Enviando factura canastilla {facturaCanastilla.FacturasCanastillaId} a Siesa con configuración {(facturaCanastilla.TieneIva ? "CON IVA" : "SIN IVA")}");

                string consecutivo = facturaCanastilla.consecutivo.ToString();
                var config = facturaCanastilla.Configuracion;
                string tercero = facturaCanastilla.Tercero.identificacion.ToString();

                var movimientosContables = new List<object>();
                decimal totalMovimientos = 0m;
                bool tieneUrea = false;

                // Movimientos por cada item de la canastilla (ingreso + IVA si aplica)
                foreach (dynamic item in facturaCanastilla.Detalle)
                {
                    if (item.Canastilla == null)
                    {
                        continue;
                    }

                    decimal valorItem = Decimal.Round((decimal)(item.cantidad * item.Canastilla.precio), 2);
                    string descripcionProducto = item.Canastilla.descripcion?.ToString() ?? "";
                    string auxiliarItem = ObtenerAuxiliarProductoCanastilla(descripcionProducto);
                    tieneUrea = tieneUrea || EsUrea(descripcionProducto);
                    Logger.Info($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} - Producto '{descripcionProducto}' → auxiliar '{auxiliarItem}'");

                    movimientosContables.Add(ConstruirMovimientoContable(
                        config, consecutivo, tercero, auxiliarItem, valorItem,
                        $"FACTURA {consecutivo} {descripcionProducto} - Cant: {item.cantidad}"));
                    totalMovimientos += valorItem;

                    if (item.Canastilla.iva > 0)
                    {
                        decimal ivaItem = Decimal.Round(valorItem * (item.Canastilla.iva / 100m), 2);
                        movimientosContables.Add(ConstruirMovimientoContable(
                            config, consecutivo, tercero,
                            ObtenerConfig("auxiliarcanastillaiva", AuxiliarIvaPorDefecto), ivaItem,
                            $"FAC {consecutivo} IVA {item.Canastilla.iva}% {descripcionProducto}"));
                        totalMovimientos += ivaItem;
                    }
                }

                // El débito (caja / CxC / banco) usa el mismo mecanismo de formas de pago de combustible
                // y debe cuadrar exactamente con la suma de créditos redondeados.
                int formaPagoPrincipalId = ObtenerIdFormaPago(facturaCanastilla.codigoFormaPago);
                int? formaPagoSecundariaId = ConvertirEnteroNullable(facturaCanastilla.codigoFormaPago2);
                string tipoProducto = tieneUrea ? "urea" : "lubricantes";
                object documentoContable = ConstruirDocumentoContable(facturaCanastilla, config, consecutivo, tercero);

                return EnviarConFormasPagoCombustible(
                    facturaCanastilla, apiContabilidad, documentoContable, movimientosContables,
                    totalMovimientos, formaPagoPrincipalId, formaPagoSecundariaId, consecutivo, tercero, tipoProducto);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error enviando factura canastilla {facturaCanastilla.FacturasCanastillaId} a Siesa");
                return false;
            }
        }

        private static object ConstruirDocumentoContable(dynamic facturaCanastilla, dynamic config, string consecutivo, string tercero)
        {
            string consecutivoAutoRegulado = config.ConsecutivoAutoregulado;
            return new
            {
                F_CIA = "1",
                F_CONSEC_AUTO_REG = string.IsNullOrWhiteSpace(consecutivoAutoRegulado) ? "0" : consecutivoAutoRegulado.Trim(),
                F350_ID_CO = (string)config.CentroOperacionesDocumento,
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                F350_CONSEC_DOCTO = consecutivo,
                F350_FECHA = (string)facturaCanastilla.fecha.ToString("yyyyMMdd"),
                F350_ID_TERCERO = tercero,
                F350_ID_CLASE_DOCTO = "",
                F350_IND_ESTADO = "1",
                F350_IND_IMPRESION = "",
                F350_NOTAS = $"FACTURA {consecutivo}",
                f350_id_mandato = ""
            };
        }

        private static object ConstruirMovimientoContable(dynamic config, string consecutivo, string tercero, string auxiliar, decimal valorCredito, string notas)
        {
            return new
            {
                F_CIA = "1",
                F350_ID_CO = (string)config.CentroOperacionesDocumento,
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                F350_CONSEC_DOCTO = consecutivo,
                F351_ID_AUXILIAR = auxiliar,
                F351_ID_TERCERO = tercero,
                F351_ID_CO_MOV = (string)config.CentroOperacionesDocumento,
                F351_ID_UN = ObtenerConfig("unidadnegociocanastilla", UnidadNegocioMovimientoPorDefecto),
                F351_ID_CCOSTO = "",
                F351_ID_FE = ObtenerConfig("idfemovimientocanastilla", IdFeMovimientoPorDefecto),
                F351_VALOR_DB = "0",
                F351_VALOR_CR = valorCredito.ToString("0.00", CultureInfo.InvariantCulture),
                F351_VALOR_DB_ALT = "",
                F351_VALOR_CR_ALT = "",
                F351_BASE_GRAVABLE = "1",
                F351_DOCTO_BANCO = "",
                F351_NRO_DOCTO_BANCO = "",
                F351_NOTAS = notas,
                F351_ID_SUCURSAL = string.IsNullOrWhiteSpace((string)config.Sucursal) ? "001" : ((string)config.Sucursal).Trim()
            };
        }

        private static string ObtenerConfig(string key, string porDefecto)
        {
            var valor = ConfigurationManager.AppSettings[key];
            return string.IsNullOrWhiteSpace(valor) ? porDefecto : valor.Trim();
        }

        /// <summary>
        /// Realiza el envío HTTP a la API de Siesa
        /// </summary>
        private bool EnviarASiesaAPI(object requestContent, string consecutivo, string tipoFactura)
        {
            var responseString = "";
            var contentString = JsonConvert.SerializeObject(requestContent);
            
            try
            {
                var urlSiesa = ConfigurationManager.AppSettings["urlsiesa"];
                var idSistema = ConfigurationManager.AppSettings["idsistema"];
                var idCompania = ConfigurationManager.AppSettings["idcompania"];
                var idDocumento = ConfigurationManager.AppSettings["iddocumento"];
                
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                
                var request = new HttpRequestMessage(
                    HttpMethod.Post, 
                    $"{urlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={idCompania}&idSistema={idSistema}&idDocumento={idDocumento}&nombreDocumento=Documento_Contablev2"
                );
                
                request.Headers.Add("ConniKey", ConfigurationManager.AppSettings["key"]);
                request.Headers.Add("ConniToken", ConfigurationManager.AppSettings["token"]);
                
                var content = new StringContent(contentString, null, "application/json");
                request.Content = content;
                
                var response = client.SendAsync(request).Result;
                responseString = response.Content.ReadAsStringAsync().Result;
                
                // Si es Bad Request, verificar si el documento ya existe
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    if (responseString.Contains("El documento ya existe"))
                    {
                        Logger.Info($"{tipoFactura} - Factura ya existe en Siesa (marcada como exitosa) - Consecutivo: {consecutivo}. Respuesta: {responseString}");
                        return true; // Tratarla como exitosa
                    }
                    else
                    {
                        Logger.Warn($"{tipoFactura} - Factura no enviada (Bad Request) - Consecutivo: {consecutivo}. Respuesta: {responseString}");
                        return false;
                    }
                }
                
                response.EnsureSuccessStatusCode();
                Logger.Info($"{tipoFactura} - Factura enviada exitosamente - Consecutivo: {consecutivo}. Respuesta: {responseString}");
                return true;
            }
            catch (HttpRequestException ex)
            {
                Logger.Warn($"{tipoFactura} - Error HttpRequest enviando factura {consecutivo}. Respuesta: {responseString}. Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                if (responseString.Contains("El documento ya existe"))
                {
                    Logger.Info($"{tipoFactura} - Factura ya existe en Siesa (marcada como exitosa) - Consecutivo: {consecutivo}. Respuesta: {responseString}");
                    return true;
                }
                
                Logger.Error(ex, $"{tipoFactura} - Error enviando factura {consecutivo}. Respuesta: {responseString}");
                return false;
            }
        }

        private static int ObtenerIdFormaPago(dynamic formaPago)
        {
            if (formaPago == null)
            {
                return 0;
            }

            try
            {
                return Convert.ToInt32(formaPago.Id, CultureInfo.InvariantCulture);
            }
            catch
            {
                try
                {
                    return Convert.ToInt32(formaPago.FormaPagoId, CultureInfo.InvariantCulture);
                }
                catch
                {
                    return 0;
                }
            }
        }

        private static int? ConvertirEnteroNullable(dynamic valor)
        {
            if (valor == null)
            {
                return null;
            }

            try
            {
                return Convert.ToInt32(valor, CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        private static decimal? ConvertirDecimalNullable(dynamic valor)
        {
            if (valor == null)
            {
                return null;
            }

            try
            {
                return Convert.ToDecimal(valor, CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Determina el auxiliar contable para un producto de canastilla.
        /// Si la descripción contiene "urea" usa <c>auxiliarurea</c>;
        /// en cualquier otro caso usa <c>auxiliarlubricantes</c>.
        /// </summary>
        private static string ObtenerAuxiliarProductoCanastilla(string descripcion)
        {
            return EsUrea(descripcion)
                ? ObtenerConfig("auxiliarurea", AuxiliarUreaPorDefecto)
                : ObtenerConfig("auxiliarlubricantes", AuxiliarLubricantesPorDefecto);
        }

        private static bool EsUrea(string descripcion)
        {
            return !string.IsNullOrWhiteSpace(descripcion) &&
                descripcion.IndexOf("urea", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Envía la factura tratando las formas de pago igual que combustible:
        /// caja / CxC / banco según App.config y auxiliar cruce por forma de pago desde App.config
        /// (ver <see cref="ObtenerAuxiliarCruceCanastilla"/>). Los créditos (producto e IVA) se conservan tal como se arman para canastilla.
        /// </summary>
        private bool EnviarConFormasPagoCombustible(
            dynamic facturaCanastilla, ApiSiesa apiContabilidad,
            object documentoContable, List<object> movimientosContables, decimal total,
            int formaPagoPrincipalId, int? formaPagoSecundariaId, string consecutivo, string tercero, string tipoProducto)
        {
            decimal? total1 = ConvertirDecimalNullable(facturaCanastilla.total1);
            decimal? total2 = ConvertirDecimalNullable(facturaCanastilla.total2);

            var formasUsadas = new List<int> { formaPagoPrincipalId };
            if (formaPagoSecundariaId.HasValue && formaPagoSecundariaId.Value > 0 && total2.HasValue && total2.Value > 0)
            {
                formasUsadas.Add(formaPagoSecundariaId.Value);
            }

            var crucesPorForma = new Dictionary<int, string>();
            foreach (var forma in formasUsadas.Distinct())
            {
                var cruce = ObtenerAuxiliarCruceCanastilla(tipoProducto, forma);
                if (string.IsNullOrWhiteSpace(cruce))
                {
                    Logger.Warn($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} no se envió: falta en App.config la clave " +
                        $"'auxiliarcrucecanastilla_{tipoProducto}_{forma}' o 'auxiliarcrucecanastilla_{forma}' con el auxiliar cruce de la forma de pago {forma}");
                    return false;
                }
                crucesPorForma[forma] = cruce;
            }

            var pagos = apiContabilidad.ConstruirPagosComoCombustible(
                formaPagoPrincipalId, formaPagoSecundariaId, total, total1, total2, crucesPorForma,
                consecutivo, (DateTime)facturaCanastilla.fecha, tercero, $"Factura canastilla {consecutivo}");

            var request = new Dictionary<string, object>
            {
                ["Inicial"] = new List<object> { new { F_CIA = "1" } },
                ["Documentocontable"] = new List<object> { documentoContable },
                ["Movimientocontable"] = movimientosContables.Concat(pagos.MovimientosBanco).ToList()
            };
            if (pagos.Caja.Any())
            {
                request["Caja"] = pagos.Caja;
            }
            if (pagos.CxC.Any())
            {
                request["MovimientoCxC"] = pagos.CxC;
            }
            request["Final"] = new List<object> { new { F_CIA = "1" } };

            return EnviarASiesaAPI(request, consecutivo, $"Canastilla-{tipoProducto}");
        }

        /// <summary>
        /// Auxiliar cruce (caja / banco / CxC) de una forma de pago, tomado del App.config.
        /// Primero busca la clave específica del tipo de producto (auxiliarcrucecanastilla_urea_5)
        /// y luego la general de la forma de pago (auxiliarcrucecanastilla_5).
        /// </summary>
        private static string ObtenerAuxiliarCruceCanastilla(string tipoProducto, int formaPagoId)
        {
            var especifico = ConfigurationManager.AppSettings[$"auxiliarcrucecanastilla_{tipoProducto}_{formaPagoId}"];
            if (!string.IsNullOrWhiteSpace(especifico))
            {
                return especifico.Trim();
            }

            return ConfigurationManager.AppSettings[$"auxiliarcrucecanastilla_{formaPagoId}"]?.Trim();
        }

        /// <summary>
        /// Controla el rate limiting para no exceder 25 facturas por minuto
        /// </summary>
        private DateTime? ObtenerFechaMinimaEnvioSiesa(string fechaConfig, string origenConfig)
        {
            if (string.IsNullOrWhiteSpace(fechaConfig))
            {
                return null;
            }

            var formatos = new[] { "yyyy-MM-dd", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss" };
            if (DateTime.TryParseExact(fechaConfig.Trim(), formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fechaCorte))
            {
                Logger.Info($"Canastilla - Filtro de fecha mínima Siesa activo ({origenConfig}): {fechaCorte:yyyy-MM-dd HH:mm:ss}");
                return fechaCorte;
            }

            if (DateTime.TryParse(fechaConfig.Trim(), out fechaCorte))
            {
                Logger.Info($"Canastilla - Filtro de fecha mínima Siesa activo ({origenConfig}): {fechaCorte:yyyy-MM-dd HH:mm:ss}");
                return fechaCorte;
            }

            Logger.Warn($"Canastilla - No se pudo interpretar {origenConfig}='{fechaConfig}'. Se ignora filtro por fecha mínima de envío a Siesa.");
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
                Logger.Info($"Canastilla - Filtro de fecha máxima Siesa activo ({origenConfig}): {fechaCorte:yyyy-MM-dd HH:mm:ss}");
                return fechaCorte;
            }

            if (DateTime.TryParse(fechaConfig.Trim(), out fechaCorte))
            {
                fechaCorte = AjustarFechaMaximaInclusiva(fechaConfig, fechaCorte, origenConfig);
                Logger.Info($"Canastilla - Filtro de fecha máxima Siesa activo ({origenConfig}): {fechaCorte:yyyy-MM-dd HH:mm:ss}");
                return fechaCorte;
            }

            Logger.Warn($"Canastilla - No se pudo interpretar {origenConfig}='{fechaConfig}'. Se ignora filtro por fecha máxima de envío a Siesa.");
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
                Logger.Warn($"Canastilla - {origenConfig}='{fechaConfig}' se ajusta a fin de día ({fechaAjustada:yyyy-MM-dd HH:mm:ss}) para evitar excluir facturas del mismo día.");
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
