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
        /// Envía una factura canastilla a Siesa
        /// </summary>
        /// <param name="facturaCanastilla">Objeto con la información de la factura</param>
        /// <param name="apiContabilidad">API de contabilidad Siesa</param>
        /// <returns>True si se envió exitosamente</returns>
        private bool EnviarFacturaCanastillaASiesa(dynamic facturaCanastilla, ApiSiesa apiContabilidad)
        {
            try
            {
                Logger.Info($"Enviando factura canastilla {facturaCanastilla.FacturasCanastillaId} a Siesa con configuración {(facturaCanastilla.TieneIva ? "CON IVA" : "SIN IVA")}");
                
                var consecutivo = facturaCanastilla.consecutivo.ToString();
                var config = facturaCanastilla.Configuracion;

                var formaPagoPrincipalId = ObtenerIdFormaPago(facturaCanastilla.codigoFormaPago);
                var formaPagoSecundariaId = ConvertirEnteroNullable(facturaCanastilla.codigoFormaPago2);
                List<PagoCanastillaSiesa> pagos = ConstruirPagosFactura(
                    formaPagoPrincipalId,
                    formaPagoSecundariaId,
                    ConvertirDecimal(facturaCanastilla.Total),
                    ConvertirDecimalNullable(facturaCanastilla.total1),
                    ConvertirDecimalNullable(facturaCanastilla.total2));
                
                // Construir el objeto de movimiento contable
                var movimientosContables = new List<object>();
                
                // Agregar movimientos por cada item de la canastilla
                foreach (dynamic item in facturaCanastilla.Detalle)
                {
                    if (item.Canastilla != null)
                    {
                        decimal valorItem = (decimal)(item.cantidad * item.Canastilla.precio);
                        
                        // Obtener auxiliar según el tipo de producto (urea vs lubricantes)
                        string descripcionProducto = item.Canastilla.descripcion?.ToString() ?? "";
                        string auxiliarItem = ObtenerAuxiliarProductoCanastilla(descripcionProducto);
                        Logger.Info($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} - Producto '{descripcionProducto}' → auxiliar '{auxiliarItem}'");

                        // Movimiento del producto/servicio
                        movimientosContables.Add(new
                        {
                            F_CIA = "1",
                            F350_ID_CO = config.CentroOperacionesDocumento,
                            F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                            F350_CONSEC_DOCTO = consecutivo,
                            F351_ID_AUXILIAR = auxiliarItem,
                            F351_ID_TERCERO = facturaCanastilla.Tercero.identificacion.ToString(),
                            F351_ID_CO_MOV = config.CentroOperacionesDocumento,
                            F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"],
                            F351_ID_CCOSTO = "",
                            F351_ID_FE = "",
                            F351_VALOR_DB = "0",
                            F351_VALOR_CR = valorItem.ToString("0.00", CultureInfo.InvariantCulture),
                            F351_BASE_GRAVABLE = "",
                            F351_DOCTO_BANCO = "",
                            F351_NRO_DOCTO_BANCO = "",
                            F351_NOTAS = $"Canastilla {item.Canastilla.descripcion} - Cant: {item.cantidad}"
                        });

                        // Si tiene IVA, agregar movimiento de IVA con auxiliar configurable
                        if (item.Canastilla.iva > 0)
                        {
                            decimal ivaItem = valorItem * (item.Canastilla.iva / 100m);
                            string auxiliarIva = ConfigurationManager.AppSettings["auxiliarcanastillaiva"] ?? "240801";
                            movimientosContables.Add(new
                            {
                                F_CIA = "1",
                                F350_ID_CO = config.CentroOperacionesDocumento,
                                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                                F350_CONSEC_DOCTO = consecutivo,
                                F351_ID_AUXILIAR = auxiliarIva,
                                F351_ID_TERCERO = facturaCanastilla.Tercero.identificacion.ToString(),
                                F351_ID_CO_MOV = config.CentroOperacionesDocumento,
                                F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"],
                                F351_ID_CCOSTO = "",
                                F351_ID_FE = "",
                                F351_VALOR_DB = "0",
                                F351_VALOR_CR = ivaItem.ToString("0.00", CultureInfo.InvariantCulture),
                                F351_BASE_GRAVABLE = valorItem.ToString("0.00", CultureInfo.InvariantCulture),
                                F351_DOCTO_BANCO = "",
                                F351_NRO_DOCTO_BANCO = "",
                                F351_NOTAS = $"IVA {item.Canastilla.iva}% - {item.Canastilla.descripcion}"
                            });
                        }
                    }
                }
                
                // Si tiene segunda forma de pago con valor, se envía usando ambas formas en Caja.
                if (pagos.Count > 1)
                {
                    var resumenPagos = string.Join(", ", pagos.Select(x => $"{x.FormaPagoId}:{x.Valor.ToString("0.00", CultureInfo.InvariantCulture)}").ToArray());
                    Logger.Info($"Factura canastilla {facturaCanastilla.FacturasCanastillaId} con multipago detectado. Formas: {resumenPagos}");
                    return EnviarFacturaCanastillaMultipago(facturaCanastilla, consecutivo, config, movimientosContables, pagos);
                }

                // Movimiento de caja/CXC (según forma de pago principal)
                if (EsFormaPagoEfectivo(formaPagoPrincipalId))
                {
                    return EnviarFacturaCanastillaEfectivo(facturaCanastilla, consecutivo, config, movimientosContables);
                }
                else
                {
                    return EnviarFacturaCanastillaCredito(facturaCanastilla, consecutivo, config, movimientosContables);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error enviando factura canastilla {facturaCanastilla.FacturasCanastillaId} a Siesa");
                return false;
            }
        }

        private bool EnviarFacturaCanastillaMultipago(dynamic facturaCanastilla, string consecutivo, dynamic config, List<object> movimientosContables, List<PagoCanastillaSiesa> pagos)
        {
            try
            {
                var caja = pagos.Select(pago => new
                {
                    F_CIA = "1",
                    F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionescaja"],
                    F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_NOTAS = $"Venta canastilla forma {pago.FormaPagoId}",
                    F351_ID_AUXILIAR = ConfigurationManager.AppSettings["cajaotros"] ?? "110501",
                    F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostocaja"] ?? "",
                    F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocaja"] ?? "",
                    F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociocaja"],
                    F351_VALOR_CR = "0",
                    F351_VALOR_DB = pago.Valor.ToString("0.00", CultureInfo.InvariantCulture),
                    F351_ID_FE = config.Idfe,
                    F358_COD_SEGURIDAD = "",
                    F358_FECHA_VCTO = facturaCanastilla.fecha.ToString("yyyyMMdd"),
                    F358_ID_CAJA = ConfigurationManager.AppSettings["caja"] ?? "003",
                    F358_ID_MEDIOS_PAGO = pago.MedioSiesa,
                    F358_NOTAS = $"Factura canastilla {consecutivo} forma {pago.FormaPagoId}",
                    F358_NRO_AUTORIZACION = "",
                    F358_NRO_CUENTA = "",
                    F358_REFERENCIA_OTROS = ""
                }).Cast<object>().ToList();

                var requestContent = new
                {
                    Inicial = new List<object> { new { F_CIA = "1" } },
                    Final = new List<object> { new { F_CIA = "1" } },
                    Caja = caja,
                    Documentocontable = new List<object>
                    {
                        new
                        {
                            F_CIA = "1",
                            F_CONSEC_AUTO_REG = config.ConsecutivoAutoregulado,
                            F350_ID_CO = config.CentroOperacionesDocumento,
                            F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                            F350_CONSEC_DOCTO = consecutivo,
                            F350_FECHA = facturaCanastilla.fecha.ToString("yyyyMMdd"),
                            F350_ID_TERCERO = facturaCanastilla.Tercero.identificacion.ToString(),
                            F350_IND_ESTADO = "1",
                            F350_NOTAS = $"Factura canastilla {consecutivo}"
                        }
                    },
                    Movimientocontable = movimientosContables
                };

                return EnviarASiesaAPI(requestContent, consecutivo, "Canastilla-Multipago");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error enviando factura canastilla multipago {consecutivo}");
                return false;
            }
        }

        /// <summary>
        /// Envía factura canastilla con forma de pago efectivo (usando módulo Caja)
        /// </summary>
        private bool EnviarFacturaCanastillaEfectivo(dynamic facturaCanastilla, string consecutivo, dynamic config, List<object> movimientosContables)
        {
            try
            {
                var requestContent = new
                {
                    Inicial = new List<object> { new { F_CIA = "1" } },
                    Final = new List<object> { new { F_CIA = "1" } },
                    Caja = new List<object>
                    {
                        new
                        {
                            F_CIA = "1",
                            F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionescaja"],
                            F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                            F350_CONSEC_DOCTO = consecutivo,
                            F351_NOTAS = "Venta canastilla",
                            F351_ID_AUXILIAR = ConfigurationManager.AppSettings["cajaotros"] ?? "110501",
                            F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostocaja"] ?? "",
                            F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocaja"] ?? "",
                            F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociocaja"],
                            F351_VALOR_CR = "0",
                            F351_VALOR_DB = facturaCanastilla.Total.ToString("0.00", CultureInfo.InvariantCulture),
                            F351_ID_FE = config.Idfe,
                            F358_COD_SEGURIDAD = "",
                            F358_FECHA_VCTO = facturaCanastilla.fecha.ToString("yyyyMMdd"),
                            F358_ID_CAJA = ConfigurationManager.AppSettings["caja"] ?? "003",
                            F358_ID_MEDIOS_PAGO = "EFE",
                            F358_NOTAS = $"Factura canastilla {consecutivo}",
                            F358_NRO_AUTORIZACION = "",
                            F358_NRO_CUENTA = "",
                            F358_REFERENCIA_OTROS = ""
                        }
                    },
                    Documentocontable = new List<object>
                    {
                        new
                        {
                            F_CIA = "1",
                            F_CONSEC_AUTO_REG = config.ConsecutivoAutoregulado,
                            F350_ID_CO = config.CentroOperacionesDocumento,
                            F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                            F350_CONSEC_DOCTO = consecutivo,
                            F350_FECHA = facturaCanastilla.fecha.ToString("yyyyMMdd"),
                            F350_ID_TERCERO = facturaCanastilla.Tercero.identificacion.ToString(),
                            F350_IND_ESTADO = "1",
                            F350_NOTAS = $"Factura canastilla {consecutivo}"
                        }
                    },
                    Movimientocontable = movimientosContables
                };

                return EnviarASiesaAPI(requestContent, consecutivo, "Canastilla-Efectivo");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error enviando factura canastilla efectivo {consecutivo}");
                return false;
            }
        }

        /// <summary>
        /// Envía factura canastilla con otras formas de pago (usando CXC)
        /// </summary>
        private bool EnviarFacturaCanastillaCredito(dynamic facturaCanastilla, string consecutivo, dynamic config, List<object> movimientosContables)
        {
            try
            {
                // Agregar movimiento de CXC
                movimientosContables.Add(new
                {
                    F_CIA = "1",
                    F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionesotros"],
                    F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_ID_AUXILIAR = ConfigurationManager.AppSettings["cajaotros"] ?? "110501",
                    F351_ID_TERCERO = "",
                    F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientootros"],
                    F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociootros"],
                    F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostootros"] ?? "",
                    F351_ID_FE = ConfigurationManager.AppSettings["idfeotros"],
                    F351_VALOR_DB = facturaCanastilla.Total.ToString("0.00", CultureInfo.InvariantCulture),
                    F351_VALOR_CR = "0",
                    F351_BASE_GRAVABLE = "",
                    F351_DOCTO_BANCO = "CG",
                    F351_NRO_DOCTO_BANCO = facturaCanastilla.fecha.ToString("yyyyMMdd"),
                    F351_NOTAS = $"Factura canastilla {consecutivo}"
                });

                var requestContent = new
                {
                    Inicial = new List<object> { new { F_CIA = "1" } },
                    Final = new List<object> { new { F_CIA = "1" } },
                    Documentocontable = new List<object>
                    {
                        new
                        {
                            F_CIA = "1",
                            F_CONSEC_AUTO_REG = config.ConsecutivoAutoregulado,
                            F350_ID_CO = config.CentroOperacionesDocumento,
                            F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"],
                            F350_CONSEC_DOCTO = consecutivo,
                            F350_FECHA = facturaCanastilla.fecha.ToString("yyyyMMdd"),
                            F350_ID_TERCERO = facturaCanastilla.Tercero.identificacion.ToString(),
                            F350_IND_ESTADO = "1",
                            F350_NOTAS = $"Factura canastilla {consecutivo}"
                        }
                    },
                    Movimientocontable = movimientosContables
                };

                return EnviarASiesaAPI(requestContent, consecutivo, "Canastilla-Credito");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error enviando factura canastilla crédito {consecutivo}");
                return false;
            }
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

        private List<PagoCanastillaSiesa> ConstruirPagosFactura(int formaPagoPrincipalId, int? formaPagoSecundariaId, decimal totalFactura, decimal? total1, decimal? total2)
        {
            var valorPago2 = (formaPagoSecundariaId.HasValue && total2.HasValue && total2.Value > 0)
                ? Decimal.Round(total2.Value, 2)
                : 0m;

            var valorPago1 = total1.HasValue && total1.Value > 0
                ? Decimal.Round(total1.Value, 2)
                : Decimal.Round(Math.Max(0m, totalFactura - valorPago2), 2);

            // Ajuste defensivo para que la suma de pagos no supere el total de la factura.
            if (valorPago1 + valorPago2 > totalFactura && totalFactura > 0)
            {
                valorPago1 = Decimal.Round(Math.Max(0m, totalFactura - valorPago2), 2);
            }

            var pagos = new List<PagoCanastillaSiesa>();

            if (formaPagoPrincipalId > 0 && valorPago1 > 0)
            {
                pagos.Add(new PagoCanastillaSiesa
                {
                    FormaPagoId = formaPagoPrincipalId,
                    Valor = valorPago1,
                    MedioSiesa = ObtenerMedioPagoSiesa(formaPagoPrincipalId)
                });
            }

            if (formaPagoSecundariaId.HasValue && formaPagoSecundariaId.Value > 0 && valorPago2 > 0)
            {
                pagos.Add(new PagoCanastillaSiesa
                {
                    FormaPagoId = formaPagoSecundariaId.Value,
                    Valor = valorPago2,
                    MedioSiesa = ObtenerMedioPagoSiesa(formaPagoSecundariaId.Value)
                });
            }

            if (!pagos.Any())
            {
                pagos.Add(new PagoCanastillaSiesa
                {
                    FormaPagoId = formaPagoPrincipalId,
                    Valor = Decimal.Round(totalFactura, 2),
                    MedioSiesa = ObtenerMedioPagoSiesa(formaPagoPrincipalId)
                });
            }

            return pagos
                .GroupBy(x => x.FormaPagoId)
                .Select(g => new PagoCanastillaSiesa
                {
                    FormaPagoId = g.Key,
                    Valor = Decimal.Round(g.Sum(x => x.Valor), 2),
                    MedioSiesa = g.First().MedioSiesa
                })
                .Where(x => x.Valor > 0)
                .ToList();
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

        private static decimal ConvertirDecimal(dynamic valor)
        {
            try
            {
                return Convert.ToDecimal(valor, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0m;
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

        private static bool EsFormaPagoEfectivo(int formaPagoId)
        {
            // En instalaciones históricas se usa 1 o 4 para efectivo.
            return formaPagoId == 1 || formaPagoId == 4;
        }

        /// <summary>
        /// Determina el auxiliar contable para un producto de canastilla.
        /// Si la descripción contiene "urea" usa <c>auxiliarurea</c>;
        /// en cualquier otro caso usa <c>auxiliarlubricantes</c>.
        /// </summary>
        private static string ObtenerAuxiliarProductoCanastilla(string descripcion)
        {
            if (!string.IsNullOrWhiteSpace(descripcion) &&
                descripcion.IndexOf("urea", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return ConfigurationManager.AppSettings["auxiliarurea"];
            }

            return ConfigurationManager.AppSettings["auxiliarlubricantes"];
        }

        private string ObtenerMedioPagoSiesa(int formaPagoId)
        {
            var key = $"mediopagosiesa_{formaPagoId}";
            var medioDesdeConfig = ConfigurationManager.AppSettings[key];
            if (!string.IsNullOrWhiteSpace(medioDesdeConfig))
            {
                return medioDesdeConfig.Trim().ToUpperInvariant();
            }

            if (EsFormaPagoEfectivo(formaPagoId))
            {
                return "EFE";
            }

            return (ConfigurationManager.AppSettings["mediopagosiesa_default"] ?? "OTR").Trim().ToUpperInvariant();
        }

        private class PagoCanastillaSiesa
        {
            public int FormaPagoId { get; set; }
            public decimal Valor { get; set; }
            public string MedioSiesa { get; set; }
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
