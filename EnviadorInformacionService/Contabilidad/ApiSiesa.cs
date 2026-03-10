using FactoradorEstacionesModelo.Objetos;
using Newtonsoft.Json;
using OfficeOpenXml.Drawing.Slicer.Style;
using OfficeOpenXml.Drawing.Vml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace EnviadorInformacionService.Contabilidad
{
    public class ApiSiesa
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string urlSiesa = ConfigurationManager.AppSettings["urlsiesa"].ToString();
        private readonly string idsistema = ConfigurationManager.AppSettings["idsistema"].ToString();
        private readonly string idCompania = ConfigurationManager.AppSettings["idcompania"].ToString();
        private readonly string idDocumento = ConfigurationManager.AppSettings["iddocumento"].ToString();
        private readonly string idDocumentoCliente = ConfigurationManager.AppSettings["iddocumentocliente"].ToString();

        internal void EnviarRecibo(Factura factura, string facturaelectronica, string consecutivo, string auxiliarContable, string cruce)
        {
            var requestContent = ConvertirAReciboSiesa(factura, facturaelectronica, consecutivo, auxiliarContable, cruce);
            var responseString = "";
            try
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{urlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={idCompania}&idSistema={idsistema}&idDocumento={idDocumento}&nombreDocumento=Documento_Contablev2");
                request.Headers.Add("ConniKey", ConfigurationManager.AppSettings["key"].ToString());
                request.Headers.Add("ConniToken", ConfigurationManager.AppSettings["token"].ToString());
                var content = new StringContent(JsonConvert.SerializeObject(requestContent), null, "application/json");
                request.Content = content;
                var response = client.SendAsync(request).Result;
                responseString = response.Content.ReadAsStringAsync().Result;
                
                // Si es Bad Request, verificar si el documento ya existe
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    if (responseString.Contains("El documento ya existe"))
                    {
                        Logger.Info($"Recibo ya existe en Siesa (marcado como exitoso) - {JsonConvert.SerializeObject(requestContent)}. Respuesta: {responseString}");
                        return; // Salir sin lanzar excepción, se considera exitoso
                    }
                    else
                    {
                        Logger.Warn($"Recibo no enviado (Bad Request) - {JsonConvert.SerializeObject(requestContent)}. Respuesta: {responseString}");
                        throw new HttpRequestException($"Bad Request: {responseString}");
                    }
                }
                
                response.EnsureSuccessStatusCode();
                Logger.Info($"Recibo enviado {JsonConvert.SerializeObject(requestContent)}. Respuesta {responseString}");
            }

            catch (HttpRequestException)
            {
                // Re-lanzar HttpRequestException (ya manejada arriba)
                throw;
            }
            catch (Exception ex)
            {
                if (responseString.Contains("El documento ya existe"))
                {
                    Logger.Info($"Recibo ya existe en Siesa (marcado como exitoso) - {JsonConvert.SerializeObject(requestContent)}. Respuesta: {responseString}");
                    return; // Salir sin lanzar excepción, se considera exitoso
                }
                else
                {
                    Logger.Warn($"Recibo no enviado - {JsonConvert.SerializeObject(requestContent)}. Respuesta: {responseString}. Error: {ex.Message}");
                    throw;
                }
            }
        }

        private object ConvertirAReciboSiesa(Factura factura, string facturaelectronica, string consecutivo, string auxiliarContable, string cruce)
        {
            var requestContent = new Movimientos()
            {
                Inicial = new List<Compania> { new Compania() { F_CIA = "1" } },
                Final = new List<Compania> { new Compania() { F_CIA = "1" } },
                MovimientoCxC = new List<MovimientoCxC> {
                new MovimientoCxC {

                        F_CIA = "1",
                        F350_ID_CO = ConfigurationManager.AppSettings["centrooperaciones"].ToString(),
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentorecibo"].ToString(),
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",
                        F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                        F351_ID_AUXILIAR = cruce,
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocosto"].ToString(),
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimiento"].ToString(),
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"].ToString(),
                        F351_VALOR_CR = "0",
                        F351_VALOR_DB = factura.total.ToString("0.00", CultureInfo.InvariantCulture),
                        F353_ID_SUCURSAL = ConfigurationManager.AppSettings["sucursal"].ToString(),
                        F353_CONSEC_DOCTO_CRUCE= ConfigurationManager.AppSettings["documentocruce"].ToString(),
                        F353_ID_TIPO_DOCTO_CRUCE=ConfigurationManager.AppSettings["documentorecibo"].ToString(),
                        F353_NRO_CUOTA_CRUCE="11",
                        F353_FECHA_DSCTO_PP=factura.fecha.ToString("yyyyMMdd"),
                        F353_FECHA_VCTO=factura.fecha.ToString("yyyyMMdd"),
                        F354_NOTAS=$"{ConfigurationManager.AppSettings["documentofactura"].ToString()} {factura.ventaId}",
                        F354_TERCERO_VEND= ConfigurationManager.AppSettings["vendedor"].ToString(),

                }
                },
                Documentocontable = new List<Documentocontable> { new Documentocontable() {
                F_CIA = "1",
                F_CONSEC_AUTO_REG = "1",
                F350_ID_CO = ConfigurationManager.AppSettings["centrooperaciones"].ToString(),
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentorecibo"].ToString(),
                F350_CONSEC_DOCTO = consecutivo,
                F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                F350_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                F350_IND_ESTADO = "1",
                F350_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}"


                }
               },
                Movimientocontable = new List<Movimientocontable>()
                {
                    new Movimientocontable()
                    {
                        F_CIA = "1",
                        F350_ID_CO = ConfigurationManager.AppSettings["movimiento"].ToString(),
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentorecibo"].ToString(),
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_BASE_GRAVABLE = "",
                        F351_NOTAS = $"Recibo combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",
                        F351_DOCTO_BANCO = "",
                        F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                        F351_ID_AUXILIAR = auxiliarContable,
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocosto"].ToString(),
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimiento"].ToString(),
                        F351_ID_FE = ConfigurationManager.AppSettings["idfe"].ToString(),
                        F351_NRO_DOCTO_BANCO="",
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"].ToString(),
                        F351_VALOR_CR = factura.total.ToString("0.00", CultureInfo.InvariantCulture),
                        F351_VALOR_DB = "0",



                    }

                },

            };
            return requestContent;
        }

        internal void EnviarFactura(Factura factura, string facturaelectronica, string consecutivo, string auxiliarContable, string cruce, string auxiliarDescuento)
        {
            var contentString = "";
            var responseString = "";
            var pagos = ConstruirPagosFactura(
                factura.codigoFormaPago,
                factura.codigoFormaPago2,
                factura.total,
                factura.total1,
                factura.total2);

            if (pagos.Count > 1)
            {
                var requestContent = ConvertirAMovimientoSiesaCajaMultipago(factura, facturaelectronica, consecutivo, auxiliarContable, cruce, auxiliarDescuento, pagos);
                contentString = JsonConvert.SerializeObject(requestContent);
            }
            else if (EsFormaPagoEfectivo(factura.codigoFormaPago))
            {
                // Pago en efectivo - usar formato con Caja
                var requestContent = ConvertirAMovimientoSiesaCaja(factura, facturaelectronica, consecutivo, auxiliarContable, cruce, auxiliarDescuento);
                contentString = JsonConvert.SerializeObject(requestContent);
            }
            else
            {
                // Otros métodos de pago - usar formato con MovimientoCxC
                var requestContent = ConvertirAMovimientoSiesa(factura, facturaelectronica, consecutivo, auxiliarContable, cruce, auxiliarDescuento);
                contentString = JsonConvert.SerializeObject(requestContent);
            }

            try
            {
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{urlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={idCompania}&idSistema={idsistema}&idDocumento={idDocumento}&nombreDocumento=Documento_Contablev2");
                request.Headers.Add("ConniKey", ConfigurationManager.AppSettings["key"].ToString());
                request.Headers.Add("ConniToken", ConfigurationManager.AppSettings["token"].ToString());

                var content = new StringContent(contentString, null, "application/json");
                request.Content = content;
                var response = client.SendAsync(request).Result;
                responseString = response.Content.ReadAsStringAsync().Result;
                
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

            catch (HttpRequestException)
            {
                // Re-lanzar HttpRequestException (ya manejada arriba)
                throw;
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
                    Logger.Warn($"Factura no enviada - {contentString}. Respuesta: {responseString}. Error: {ex.Message}");
                    throw;
                }
            }
        }

        private object ConvertirAMovimientoSiesa(Factura factura, string facturaelectronica, string consecutivo, string auxiliarContable, string cruce, string auxiliarDescuento)
        {
            var movimientos = new List<object>();
            // Primer movimiento contable
            movimientos.Add(new
            {
                F_CIA = "1",
                F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionescontableotros"].ToString(),
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                F350_CONSEC_DOCTO = consecutivo,
                F351_ID_AUXILIAR = auxiliarContable,
                F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocontableotros"].ToString(),
                F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociocontableotros"].ToString(),
                F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostocontableotros"].ToString(),
                F351_ID_FE = "",
                F351_VALOR_DB = "0",
                F351_VALOR_CR = factura.subtotal.ToString("0.00", CultureInfo.InvariantCulture),
                F351_BASE_GRAVABLE = "",
                F351_DOCTO_BANCO = "",
                F351_NRO_DOCTO_BANCO = "",
                F351_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}"
            });
            // Segundo movimiento contable
            movimientos.Add(new
            {
                F_CIA = "1",
                F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionesotros"].ToString(),
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                F350_CONSEC_DOCTO = consecutivo,
                F351_ID_AUXILIAR = cruce,
                F351_ID_TERCERO = "",
                F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientootros"].ToString(),
                F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociootros"].ToString(),
                F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostootros"].ToString(),
                F351_ID_FE = ConfigurationManager.AppSettings["idfeotros"].ToString(),
                F351_VALOR_DB = factura.total.ToString("0.00", CultureInfo.InvariantCulture),
                F351_VALOR_CR = "0",
                F351_BASE_GRAVABLE = "",
                F351_DOCTO_BANCO = "CG",
                F351_NRO_DOCTO_BANCO = factura.fecha.ToString("yyyyMMdd"),
                F351_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}"
            });
            // Movimiento de descuento si aplica
            if (factura.Venta.Descuento > 0)
            {
                movimientos.Add(new
                {
                    F_CIA = "1",
                    F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionescontabledescuento"]?.ToString() ?? "101",
                    F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_ID_AUXILIAR = auxiliarDescuento,
                    F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                    F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocontabledescuento"]?.ToString() ?? "101",
                    F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociodescuento"]?.ToString() ?? "03",
                    F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostodescuento"]?.ToString() ?? "",
                    F351_ID_FE = ConfigurationManager.AppSettings["idfedescuento"]?.ToString() ?? "1",
                    F351_VALOR_DB = factura.descuento.ToString("0.00", CultureInfo.InvariantCulture),
                    F351_VALOR_CR = "",
                    F351_BASE_GRAVABLE = "1",
                    F351_DOCTO_BANCO = "",
                    F351_NRO_DOCTO_BANCO = "",
                    F351_NOTAS = $"FAC {consecutivo} DESCUENTO PROMOCIÓN",
                    F351_ID_SUCURSAL = ConfigurationManager.AppSettings["sucursal"]?.ToString() ?? "001"
                });
            }
            var requestContent = new
            {
                Inicial = new List<object> { new { F_CIA = "1" } },
                Final = new List<object> { new { F_CIA = "1" } },
                Documentocontable = new List<object> { new {
                    F_CIA = "1",
                    F_CONSEC_AUTO_REG = "0",
                    F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionesdocuemnto"].ToString(),
                    F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                    F350_CONSEC_DOCTO = consecutivo,
                    F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                    F350_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                    F350_IND_ESTADO = "1",
                    F350_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",
                }},
                Movimientocontable = movimientos
            };
            return requestContent;
        }

        private object ConvertirAMovimientoSiesaCajaMultipago(Factura factura, string facturaelectronica, string consecutivo, string auxiliarContable, string cruce, string auxiliarDescuento, List<PagoFacturaSiesa> pagos)
        {
            var movimientos = new List<Movimientocontable>();
            movimientos.Add(new Movimientocontable()
            {
                F_CIA = "1",
                F350_ID_CO = ConfigurationManager.AppSettings["centrooperaciones"].ToString(),
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                F350_CONSEC_DOCTO = consecutivo,
                F351_BASE_GRAVABLE = "",
                F351_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",
                F351_DOCTO_BANCO = "",
                F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                F351_ID_AUXILIAR = auxiliarContable,
                F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocosto"].ToString(),
                F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimiento"].ToString(),
                F351_ID_FE = consecutivo,
                F351_NRO_DOCTO_BANCO = "",
                F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"].ToString(),
                F351_VALOR_CR = factura.subtotal.ToString("0.00", CultureInfo.InvariantCulture),
                F351_VALOR_DB = "0",
            });

            // Movimiento de descuento si aplica
            if (factura.descuento > 0)
            {
                movimientos.Add(new Movimientocontable()
                {
                    F_CIA = "1",
                    F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionescontabledescuento"]?.ToString() ?? "101",
                    F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_ID_AUXILIAR = auxiliarDescuento,
                    F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                    F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocontabledescuento"]?.ToString() ?? "101",
                    F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociodescuento"]?.ToString() ?? "03",
                    F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostodescuento"]?.ToString() ?? "",
                    F351_ID_FE = ConfigurationManager.AppSettings["idfedescuento"]?.ToString() ?? "1",
                    F351_VALOR_DB = factura.descuento.ToString("0.00", CultureInfo.InvariantCulture),
                    F351_VALOR_CR = "",
                    F351_BASE_GRAVABLE = "1",
                    F351_DOCTO_BANCO = "",
                    F351_NRO_DOCTO_BANCO = "",
                    F351_NOTAS = $"FAC {consecutivo} DESCUENTO PROMOCIÓN",
                });
            }

            var pagosCaja = pagos.Where(p => EsFormaPagoEfectivo(p.FormaPagoId)).ToList();
            var pagosBanco = pagos.Where(p => !EsFormaPagoEfectivo(p.FormaPagoId)).ToList();

            var caja = pagosCaja.Select(p => new Caja
            {
                F_CIA = "1",
                F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionescaja"].ToString(),
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                F350_CONSEC_DOCTO = consecutivo,
                F351_NOTAS = $"Venta combustible forma {p.FormaPagoId}",
                F351_ID_AUXILIAR = cruce,
                F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostocaja"].ToString(),
                F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocaja"].ToString(),
                F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociocaja"].ToString(),
                F351_VALOR_CR = "0",
                F351_VALOR_DB = p.Valor.ToString("0.00", CultureInfo.InvariantCulture),
                F351_ID_FE = ConfigurationManager.AppSettings["idfe"].ToString(),
                F358_COD_SEGURIDAD = "",
                F358_FECHA_VCTO = factura.fecha.ToString("yyyyMMdd"),
                F358_ID_CAJA = ConfigurationManager.AppSettings["caja"].ToString(),
                F358_ID_MEDIOS_PAGO = p.MedioSiesa,
                F358_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {consecutivo} forma {p.FormaPagoId}",
                F358_NRO_AUTORIZACION = "",
                F358_NRO_CUENTA = cruce,
                F358_REFERENCIA_OTROS = ""
            }).ToList();

            foreach (var pagoBanco in pagosBanco)
            {
                movimientos.Add(new Movimientocontable()
                {
                    F_CIA = "1",
                    F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionesotros"].ToString(),
                    F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_ID_AUXILIAR = cruce,
                    F351_ID_TERCERO = "",
                    F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientootros"].ToString(),
                    F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociootros"].ToString(),
                    F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostootros"].ToString(),
                    F351_ID_FE = ConfigurationManager.AppSettings["idfeotros"].ToString(),
                    F351_VALOR_DB = pagoBanco.Valor.ToString("0.00", CultureInfo.InvariantCulture),
                    F351_VALOR_CR = "0",
                    F351_BASE_GRAVABLE = "",
                    F351_DOCTO_BANCO = "CG",
                    F351_NRO_DOCTO_BANCO = factura.fecha.ToString("yyyyMMdd"),
                    F351_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId} forma {pagoBanco.FormaPagoId}"
                });
            }

            return new
            {
                Inicial = new List<Compania> { new Compania() { F_CIA = "1" } },
                Final = new List<Compania> { new Compania() { F_CIA = "1" } },
                Caja = caja,
                Documentocontable = new List<Documentocontable> { new Documentocontable() {
                    F_CIA = "1",
                    F_CONSEC_AUTO_REG = ConfigurationManager.AppSettings["consecutivoautoregulado"].ToString(),
                    F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionesdocuemnto"].ToString(),
                    F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                    F350_CONSEC_DOCTO = consecutivo,
                    F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                    F350_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                    F350_IND_ESTADO = "1",
                    F350_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",
                }},
                Movimientocontable = movimientos
            };
        }

        private List<PagoFacturaSiesa> ConstruirPagosFactura(int formaPagoPrincipalId, int? formaPagoSecundariaId, decimal totalFactura, decimal? total1, decimal? total2)
        {
            var valorPago2 = (formaPagoSecundariaId.HasValue && total2.HasValue && total2.Value > 0)
                ? decimal.Round(total2.Value, 2)
                : 0m;

            var valorPago1 = (total1.HasValue && total1.Value > 0)
                ? decimal.Round(total1.Value, 2)
                : decimal.Round(Math.Max(0m, totalFactura - valorPago2), 2);

            if (valorPago1 + valorPago2 > totalFactura && totalFactura > 0)
            {
                valorPago1 = decimal.Round(Math.Max(0m, totalFactura - valorPago2), 2);
            }

            var pagos = new List<PagoFacturaSiesa>();

            if (formaPagoPrincipalId > 0 && valorPago1 > 0)
            {
                pagos.Add(new PagoFacturaSiesa
                {
                    FormaPagoId = formaPagoPrincipalId,
                    Valor = valorPago1,
                    MedioSiesa = ObtenerMedioPagoSiesa(formaPagoPrincipalId)
                });
            }

            if (formaPagoSecundariaId.HasValue && formaPagoSecundariaId.Value > 0 && valorPago2 > 0)
            {
                pagos.Add(new PagoFacturaSiesa
                {
                    FormaPagoId = formaPagoSecundariaId.Value,
                    Valor = valorPago2,
                    MedioSiesa = ObtenerMedioPagoSiesa(formaPagoSecundariaId.Value)
                });
            }

            if (!pagos.Any())
            {
                pagos.Add(new PagoFacturaSiesa
                {
                    FormaPagoId = formaPagoPrincipalId,
                    Valor = decimal.Round(totalFactura, 2),
                    MedioSiesa = ObtenerMedioPagoSiesa(formaPagoPrincipalId)
                });
            }

            return pagos
                .GroupBy(x => x.FormaPagoId)
                .Select(g => new PagoFacturaSiesa
                {
                    FormaPagoId = g.Key,
                    Valor = decimal.Round(g.Sum(x => x.Valor), 2),
                    MedioSiesa = g.First().MedioSiesa
                })
                .Where(x => x.Valor > 0)
                .ToList();
        }

        private static bool EsFormaPagoEfectivo(int formaPagoId)
        {
            return formaPagoId == 1 || formaPagoId == 4;
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

        private class PagoFacturaSiesa
        {
            public int FormaPagoId { get; set; }
            public decimal Valor { get; set; }
            public string MedioSiesa { get; set; }
        }

        private MovimientosCaja ConvertirAMovimientoSiesaCaja(Factura factura, string facturaelectronica, string consecutivo, string auxiliarContable, string cruce, string auxiliarDescuento)
        {
            var movimientos = new List<Movimientocontable>();
            movimientos.Add(new Movimientocontable()
            {
                F_CIA = "1",
                F350_ID_CO = ConfigurationManager.AppSettings["centrooperaciones"].ToString(),
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                F350_CONSEC_DOCTO = consecutivo,
                F351_BASE_GRAVABLE = "",
                F351_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {factura.ventaId}",
                F351_DOCTO_BANCO = "",
                F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                F351_ID_AUXILIAR = auxiliarContable,
                F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocosto"].ToString(),
                F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimiento"].ToString(),
                F351_ID_FE = consecutivo,
                F351_NRO_DOCTO_BANCO="",
                F351_ID_UN = ConfigurationManager.AppSettings["unidadnegocio"].ToString(),
                F351_VALOR_CR = factura.subtotal.ToString("0.00", CultureInfo.InvariantCulture),
                F351_VALOR_DB = "0",
            });
            // Movimiento de descuento si aplica
            if (factura.descuento > 0)
            {
                movimientos.Add(new Movimientocontable()
                {
                    F_CIA = "1",
                    F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionescontabledescuento"]?.ToString() ?? "101",
                    F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                    F350_CONSEC_DOCTO = consecutivo,
                    F351_ID_AUXILIAR = auxiliarDescuento,
                    F351_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                    F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocontabledescuento"]?.ToString() ?? "101",
                    F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociodescuento"]?.ToString() ?? "03",
                    F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostodescuento"]?.ToString() ?? "",
                    F351_ID_FE = ConfigurationManager.AppSettings["idfedescuento"]?.ToString() ?? "1",
                    F351_VALOR_DB = factura.descuento.ToString("0.00", CultureInfo.InvariantCulture),
                    F351_VALOR_CR = "",
                    F351_BASE_GRAVABLE = "1",
                    F351_DOCTO_BANCO = "",
                    F351_NRO_DOCTO_BANCO = "",
                    F351_NOTAS = $"FAC {consecutivo} DESCUENTO PROMOCIÓN",
                    //F351_ID_SUCURSAL = ConfigurationManager.AppSettings["sucursal"]?.ToString() ?? "001"
                });
            }
            var requestContent = new MovimientosCaja()
            {
                Inicial = new List<Compania> { new Compania() { F_CIA = "1" } },
                Final = new List<Compania> { new Compania() { F_CIA = "1" } },
                Caja = new List<Caja> {
                new Caja {
                        F_CIA = "1",
                        F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionescaja"].ToString(),
                        F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                        F350_CONSEC_DOCTO = consecutivo,
                        F351_NOTAS = "Venta combustible",
                        F351_ID_AUXILIAR = cruce,
                        F351_ID_CCOSTO = ConfigurationManager.AppSettings["centrocostocaja"].ToString(),
                        F351_ID_CO_MOV = ConfigurationManager.AppSettings["movimientocaja"].ToString(),
                        F351_ID_UN = ConfigurationManager.AppSettings["unidadnegociocaja"].ToString(),
                        F351_VALOR_CR = "0",
                        F351_VALOR_DB = factura.total.ToString("0.00", CultureInfo.InvariantCulture),
                        F351_ID_FE =ConfigurationManager.AppSettings["idfe"].ToString(),
                        F358_COD_SEGURIDAD = "",
                        F358_FECHA_VCTO = factura.fecha.ToString("yyyyMMdd"),
                        F358_ID_CAJA = ConfigurationManager.AppSettings["caja"].ToString(),
                        F358_ID_MEDIOS_PAGO = ObtenerMedioPagoSiesa(factura.codigoFormaPago),
                        F358_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {consecutivo}",
                        F358_NRO_AUTORIZACION="",
                        F358_NRO_CUENTA=cruce,
                        F358_REFERENCIA_OTROS=""
                }
                },
                Documentocontable = new List<Documentocontable> { new Documentocontable() {
                F_CIA = "1",
                F_CONSEC_AUTO_REG = ConfigurationManager.AppSettings["consecutivoautoregulado"].ToString(),
                F350_ID_CO = ConfigurationManager.AppSettings["centrooperacionesdocuemnto"].ToString(),
                F350_ID_TIPO_DOCTO = ConfigurationManager.AppSettings["documentofactura"].ToString(),
                F350_CONSEC_DOCTO = consecutivo,
                F350_FECHA = factura.fecha.ToString("yyyyMMdd"),
                F350_ID_TERCERO = factura.Tercero.identificacion.ToString(),
                F350_IND_ESTADO = "1",
                F350_NOTAS = $"Factura combustible {factura.Venta.Combustible.Trim()} id local {consecutivo}",
                }
               },
                Movimientocontable = movimientos
            };
            return requestContent;
        }

        public bool EnviarTercero(Tercero tercero)
        {
            var requestContent = ConvertirATercerosSiesa(tercero);
            var responseString = "";

            try
            {


                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(20);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{urlSiesa}/api/siesa/v3.1/conectoresimportar?idCompania={idCompania}&idSistema={idsistema}&idDocumento={idDocumentoCliente}&nombreDocumento=TERCERO_CLIENTE_INTEGRADO");
                request.Headers.Add("ConniKey", ConfigurationManager.AppSettings["key"].ToString());
                request.Headers.Add("ConniToken", ConfigurationManager.AppSettings["token"].ToString());
                var content = new StringContent(JsonConvert.SerializeObject(requestContent), null, "application/json");
                request.Content = content;
                var response = client.SendAsync(request).Result;
                responseString = response.Content.ReadAsStringAsync().Result;
                
                // Si es Bad Request, verificar si ya existe o no tiene permisos
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    if (responseString.Contains("El documento ya existe") || responseString.Contains("No tiene acceso a modificar"))
                    {
                        Logger.Info($"Tercero ya existe o sin permisos (marcado como exitoso) - {JsonConvert.SerializeObject(requestContent)}. Respuesta: {responseString}");
                        return true; // Tratarlo como exitoso
                    }
                    else
                    {
                        Logger.Warn($"Tercero no enviado (Bad Request) - {JsonConvert.SerializeObject(requestContent)}. Respuesta: {responseString}");
                        return false;
                    }
                }
                
                response.EnsureSuccessStatusCode();
                Logger.Info($"Tercero enviado {JsonConvert.SerializeObject(requestContent)}.Respuesta {responseString}");
                return true;
            }
            catch (HttpRequestException ex)
            {
                // HttpRequestException ya fue manejada arriba en Bad Request
                Logger.Warn($"Tercero no enviado (HttpRequestException) - {JsonConvert.SerializeObject(requestContent)}. Respuesta: {responseString}. Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // Verificar casos especiales en Exception general
                if (responseString.Contains("El documento ya existe") || responseString.Contains("No tiene acceso a modificar"))
                {
                    Logger.Info($"Tercero ya existe o sin permisos (marcado como exitoso) - {JsonConvert.SerializeObject(requestContent)}. Respuesta: {responseString}");
                    return true;
                }
                
                Logger.Warn($"Tercero no enviado - {JsonConvert.SerializeObject(requestContent)}. Respuesta: {responseString}. Error: {ex.Message}");
                return false;
            }
        }

        private Root ConvertirATercerosSiesa(Tercero x)
        {

            var nombre = "";
            var apellido = "";
            var nombreCompleto = x?.Nombre?.Trim();
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
                        F_ID_TERCERO= x.identificacion.Trim(),
                        F_ID_SUCURSAL = ConfigurationManager.AppSettings["sucursal"].ToString(),
                        F_ID_CLASE = "1",
                        F_ID_VALOR_TERCERO = "1"
                    },

                    new ImptosReten
                    {
                        F_TIPO_REG = "46",
                        F_CIA = "1",
                        F_ID_TERCERO= x.identificacion.Trim(),
                        F_ID_SUCURSAL = ConfigurationManager.AppSettings["sucursal"].ToString(),
                        F_ID_CLASE = "2",
                        F_ID_VALOR_TERCERO = "1"
                    }
                },

                Clientes = new List<ClienteSiesa> {
                    new ClienteSiesa
                    {
                        F_CIA = "1",
                        F201_ID_TERCERO = x.identificacion.Trim(),
                        F201_ID_SUCURSAL = ConfigurationManager.AppSettings["sucursal"].ToString(),
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
                         F015_TELEFONO = x.Telefono.Trim().Length > 20 ? x.Telefono.Substring(0, 20) : x.Telefono.Trim(),
                    F015_EMAIL = x.Correo,
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
                        F200_ID = x.identificacion.Trim(),
                        F200_NIT = x.identificacion.Trim(),
                    F200_ID_TIPO_IDENT = x.tipoIdentificacionS == "Nit" ? "N" : "C",
                    F200_IND_TIPO_TERCERO = x.tipoIdentificacionS == "Nit" ? "2" :"1",
                    F200_RAZON_SOCIAL = nombreCompleto.Length > 40 ? nombreCompleto.Substring(0, 40) : nombreCompleto,
                    F200_APELLIDO1 = apellido,
                    F200_APELLIDO2 = "NA",
                    F200_NOMBRES = nombre,
                    F200_NOMBRE_EST = nombre,
                    F015_CONTACTO = "SIGES",
                        F015_DIRECCION1 = x.Direccion.Trim().Length > 40 ? x.Direccion.Substring(0, 40) : x.Direccion.Trim(),
                        F015_DIRECCION2 = "",
                        F015_DIRECCION3 = "",
                    F015_ID_PAIS = "169",
                    F015_ID_DEPTO = "05",
                    F015_ID_CIUDAD = "001",
                    F015_TELEFONO = x.Telefono.Trim().Length > 20 ? x.Telefono.Substring(0, 20) : x.Telefono.Trim(),
                    F015_EMAIL = x.Correo,
                    F200_FECHA_NACIMIENTO = "20000101",
                    F200_ID_CIIU = "0010",
                    F015_CELULAR = x.Telefono.Trim().Length > 20 ? x.Telefono.Substring(0, 20) : x.Telefono.Trim()
                }
                }
            };
        }
    }
}
