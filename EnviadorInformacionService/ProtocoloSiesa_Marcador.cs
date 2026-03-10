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
    /// <summary>
    /// Script auxiliar: llama a GetInfoFacturaElectronica para cada factura no enviada,
    /// genera el arreglo facturaElectronica y marca la factura como enviada.
    /// Si hay error antes de generar el arreglo, también marca la factura como enviada.
    /// Uso: llamar ProtocoloSiesa_Marcador.RunOnce() desde un runner o invocarlo temporalmente.
    /// </summary>
    public class ProtocoloSiesa_Marcador
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IConexionEstacionRemota _conexionEstacionRemota = new ConexionEstacionRemota();

        /// <summary>
        /// Ejecuta una pasada: por cada factura no enviada intenta obtener la info electrónica,
        /// genera facturaElectronica[] y marca la factura como enviada; si falla antes de generar el arreglo,
        /// también marca la factura como enviada y registra el error.
        /// </summary>
        public void RunOnce()
        {
            var _estacionesRepositorio = new EstacionesRepositorioSqlServer();

            Guid estacionFuente;
            try
            {
                estacionFuente = new Guid(ConfigurationManager.AppSettings["estacionFuente"]);
            }
            catch (Exception ex)
            {
                Logger.Error($"No se pudo leer estacionFuente desde appsettings: {ex.Message}");
                return;
            }

            var facturas = _estacionesRepositorio.BuscarFacturasNoEnviadasSiesa();
            if (facturas == null || !facturas.Any())
            {
                Logger.Info("No hay facturas pendientes de envío a Siesa.");
                return;
            }

            foreach (var factura in facturas)
            {
                try
                {
                    string infoTemp = string.Empty;
                    int intentos = 0;

                    // Intentar obtener la información de la factura electrónica hasta 3 intentos
                    do
                    {
                        try
                        {
                            infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(factura.ventaId, estacionFuente, _conexionEstacionRemota.getToken());
                        }
                        catch (Exception ex)
                        {
                            infoTemp = string.Empty;
                            Logger.Error($"Error al obtener información de la factura electrónica (ventaId={factura.ventaId}) intento {intentos + 1}: {ex.Message}");
                        }

                        if (!string.IsNullOrEmpty(infoTemp)) break;

                        intentos++;
                        Thread.Sleep(1000);
                    } while (intentos < 3);

                    // Si no obtuvimos info, marcamos como enviada (según la regla solicitada)
                    if (string.IsNullOrWhiteSpace(infoTemp))
                    {
                        Logger.Warn($"No se obtuvo info electrónica para factura {factura.ventaId} después de {intentos} intentos. Marcando como enviada.");
                        try
                        {
                            _estacionesRepositorio.ActuralizarFacturasEnviadosSiesa(new List<int> { factura.ventaId });
                        }
                        catch (Exception exDb)
                        {
                            Logger.Error($"Error marcando factura {factura.ventaId} como enviada: {exDb.Message}");
                        }
                        continue;
                    }

                    // Normalizar saltos de línea y generar el arreglo facturaElectronica
                    try
                    {
                        // Convertimos saltos de línea a un delimitador único
                        infoTemp = infoTemp.Replace("\r\n", "$").Replace("\n", "$").Replace("\r", "$").Replace("$$", "$");
                        var facturaElectronica = infoTemp.Split('$');

                        Logger.Info($"Generado facturaElectronica para ventaId={factura.ventaId}. Parts={facturaElectronica.Length}");

                        // Validaciones: buscar un componente que sea factura id (letras + numeros)
                        var invoiceIdPart = facturaElectronica.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p) && Regex.IsMatch(p.Trim(), @"^([A-Za-z]+)(\d+)$", RegexOptions.IgnoreCase));
                        var dateMatch = Regex.Match(infoTemp, @"\b(\d{2}/\d{2}/\d{4}\s+\d{2}:\d{2}:\d{2})\b");

                        bool missingInvoiceId = string.IsNullOrWhiteSpace(invoiceIdPart);
                        bool missingDate = !dateMatch.Success;

                        if (missingInvoiceId || missingDate)
                        {
                            var reasons = new List<string>();
                            if (missingInvoiceId) reasons.Add("invoice id (letras+numeros)");
                            if (missingDate) reasons.Add("fecha de emisión");
                            Logger.Warn($"Factura {factura.ventaId}: datos incompletos en facturaElectronica ({string.Join(", ", reasons)}). Marcando como enviada.");
                            try
                            {
                                _estacionesRepositorio.ActuralizarFacturasEnviadosSiesa(new List<int> { factura.ventaId });
                                Logger.Info($"Factura {factura.ventaId} marcada como enviada por datos incompletos.");
                            }
                            catch (Exception exDb)
                            {
                                Logger.Error($"Error marcando factura {factura.ventaId} como enviada: {exDb.Message}");
                            }
                        }
                        else
                        {
                            // Datos mínimos presentes: continuar con el flujo (marcar como enviada)
                            try
                            {
                                //_estacionesRepositorio.ActuralizarFacturasEnviadosSiesa(new List<int> { factura.ventaId });
                                Logger.Info($"Factura {factura.ventaId} no marcada como enviada existe factura electronica.");
                            }
                            catch (Exception exDb)
                            {
                                Logger.Error($"Error marcando factura {factura.ventaId} como enviada: {exDb.Message}");
                            }
                        }
                    }
                    catch (Exception exParse)
                    {
                        // Si hay error generando el arreglo, marcar como enviada y loguear
                        Logger.Error($"Error al parsear infoTemp para factura {factura.ventaId}: {exParse.Message}");
                        try
                        {
                            _estacionesRepositorio.ActuralizarFacturasEnviadosSiesa(new List<int> { factura.ventaId });
                            Logger.Info($"Factura {factura.ventaId} marcada como enviada tras error de parse.");
                        }
                        catch (Exception exDb)
                        {
                            Logger.Error($"Error marcando factura {factura.ventaId} como enviada: {exDb.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error inesperado procesando factura {factura.ventaId}: {ex.Message}");
                    // En caso extremo, intentar marcar como enviada para no volver a reprocesar
                    try
                    {
                        _estacionesRepositorio.ActuralizarFacturasEnviadosSiesa(new List<int> { factura.ventaId });
                    }
                    catch (Exception exDb)
                    {
                        Logger.Error($"Error marcando factura {factura.ventaId} como enviada tras excepción: {exDb.Message}");
                    }
                }
            }
        }
    }
}
