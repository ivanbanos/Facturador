using System.Drawing.Printing;
using FacturadorEstacionesRepositorio;
using FactoradorEstacionesModelo.Siges;
using FacturadorEstacionesPOSWinForm;
using Microsoft.Extensions.Options;
using System.Drawing;
using System.Text;
using ServicioSIGES;
using System.Net.NetworkInformation;
using Modelo;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
using FacturadorEstacionesPOSWinForm.Repo;
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System.Drawing.Imaging;

namespace SigesServicio
{
    public class WorkerImpresion : BackgroundService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private int imprimiendo = 0;
        private bool ImpresionAutomatica = false;
        private bool impresionFormaDePagoOrdenDespacho = false;
        private string firstMacAddress;
        private readonly Guid estacionFuente;
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly IFidelizacion _fidelizacion;
        private readonly IWorkerHeartbeatTracker _heartbeat;

        private readonly bool generaFacturaElectronica;
        private readonly InfoEstacion _infoEstacion;
        private readonly List<CaraImpresora> _caraImpresoras;
        public WorkerImpresion(IEstacionesRepositorio estacionesRepositorio, IOptions<InfoEstacion> infoEstacion, IOptions<List<CaraImpresora>> caraImpresoras, IFidelizacion fidelizacion, IConexionEstacionRemota conexionEstacionRemota, IWorkerHeartbeatTracker heartbeat)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _infoEstacion = infoEstacion.Value;
            _caraImpresoras = caraImpresoras.Value;
            estacionFuente = Guid.TryParse(_infoEstacion.EstacionFuente, out var estacion) ? estacion : Guid.Empty;

            firstMacAddress = NetworkInterface
        .GetAllNetworkInterfaces()
        .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .Select(nic => nic.GetPhysicalAddress().ToString())
        .FirstOrDefault();
            _fidelizacion = fidelizacion;
            _conexionEstacionRemota = conexionEstacionRemota;
            generaFacturaElectronica = _infoEstacion.GeneraFacturaElectronica;
            _heartbeat = heartbeat;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Info("WorkerImpresion StartAsync");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.Info("WorkerImpresion StopAsync");
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info("WorkerImpresion iniciado");
            _heartbeat.MarkStarted(nameof(WorkerImpresion));
            Logger.Info($"WorkerImpresion configuración razón: {_infoEstacion.Razon}");
            ImpresionAutomatica = _infoEstacion.ImpresionAutomatica;
            impresionFormaDePagoOrdenDespacho = _infoEstacion.ImpresionFormaDePagoOrdenDespacho;
            if (estacionFuente == Guid.Empty)
            {
                Logger.Warn($"EstacionFuente invalida en configuración: '{_infoEstacion.EstacionFuente}'.");
            }

            try
            {
                formas = _estacionesRepositorio.BuscarFormasPagosSiges();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error cargando formas de pago al iniciar WorkerImpresion: {ex.Message}");
                Logger.Error(ex.StackTrace);
                formas = new List<FormaPagoSiges>();
            }

            Logger.Info("WorkerImpresion formas de pago cargadas");
            imprimiendo = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _heartbeat.Pulse(nameof(WorkerImpresion));
                    Logger.Info($"WorkerImpresion ciclo iniciado. imprimiendo={imprimiendo}");

                    if (imprimiendo == 0)





                    {

                        var turnoimprimir = _estacionesRepositorio.getTurnosSinImprimir();
                        Logger.Debug($"WorkerImpresion turno pendiente: {(turnoimprimir != null ? turnoimprimir.Id : 0)}");
                        if (imprimiendo == 0 && turnoimprimir != null)
                        {
                            if (imprimiendo == 0)
                            {
                                imprimiendo++;
                                Logger.Info($"Imprimiendo turno {JsonConvert.SerializeObject(turnoimprimir)}");
                                ImprimirTurno(turnoimprimir);
                                turnoimprimir.impresa++;

                                _estacionesRepositorio.ActualizarTurnoImpreso(turnoimprimir.Id);
                            }
                            else
                            {
                                await Task.Delay(100, stoppingToken);
                            }
                            await Task.Delay(1000, stoppingToken);
                        }
                        var factura = _estacionesRepositorio.getFacturasImprimir();
                        Logger.Debug($"WorkerImpresion factura pendiente: {(factura != null ? factura.ventaId : 0)}");
                        
                        if (factura != null)
                        {
                            Logger.Info($"Factura recuperada - VentaId: {factura.ventaId}, Impresa: {factura.impresa}, ImpresionAutomatica: {ImpresionAutomatica}");
                        }
                        
                        if (imprimiendo == 0 && factura != null
                            && ((factura.impresa == 0 && ImpresionAutomatica) || factura.impresa <= -1))
                        {
                            Logger.Info($"Iniciando impresion de factura VentaId: {factura.ventaId}");

                            if (imprimiendo == 0)
                            {
                                imprimiendo++;
                                try
                                {
                                    Imprimir(factura);
                                    factura.impresa++;
                                    Logger.Info($"Factura VentaId: {factura.ventaId} impresa exitosamente");
                                }
                                catch (Exception exImprimir)
                                {
                                    Logger.Error($"Error imprimiendo factura VentaId: {factura.ventaId} - {exImprimir.Message}");
                                    Logger.Error(exImprimir.StackTrace);
                                    imprimiendo = 0;
                                    factura.impresa++; // Mark as attempted to avoid retry inmediato infinito
                                }
                            }
                            else
                            {
                                await Task.Delay(100, stoppingToken);
                            }
                            await Task.Delay(1000, stoppingToken);
                        }
                        else if (factura == null)
                        {
                            Logger.Debug("No hay facturas pendientes de imprimir");
                        }


                        await Task.Delay(1000, stoppingToken);
                        Logger.Info("WorkerImpresion ciclo finalizado");

                    }
                    else
                    {
                        await Task.Delay(1000, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    var rootException = ex is AggregateException aggregateException
                        ? aggregateException.Flatten().InnerException ?? ex
                        : ex;

                    imprimiendo = 0;
                    Logger.Error("Ex" + rootException.Message);
                    Logger.Error("Ex" + rootException.StackTrace);
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(1000, stoppingToken);
                    }
                }

            }
        }

        private void ImprimirTurno(TurnoSiges turnoimprimir)
        {

            _turno = turnoimprimir;
            //imprimir
            try
            {

                getLineasImprimirTurno(turnoimprimir);
                try
                {
                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintTurno);
                    pd.DefaultPageSettings.Margins.Bottom = 20;

                    pd.PrinterSettings.PrinterName = _caraImpresoras.First().Impresora.Trim();
                    pd.Print();

                }
                catch (Exception ex)
                {

                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintTurno);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    pd.Print();

                }
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Error("Ex" + ex.Message);
                Logger.Error("Ex" + ex.StackTrace);
                Thread.Sleep(5000);
            }
        }

        private void getLineasImprimirTurno(TurnoSiges turnoimprimir)
        {
            lineasImprimirTurno = new List<LineasImprimir>();
            var guiones = new StringBuilder();
            guiones.Append('-', _infoEstacion.CaracteresPorPagina);
            // Iterate over the file, printing each line.
            lineasImprimirTurno.Add(new LineasImprimir(".", true));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Razon, true));
            lineasImprimirTurno.Add(new LineasImprimir("NIT             " + _infoEstacion.NIT, false));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Nombre, false));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Direccion, false));
            lineasImprimirTurno.Add(new LineasImprimir(_infoEstacion.Telefono, false));
            lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimirTurno.Add(new LineasImprimir("Empleado:       " + turnoimprimir.Empleado, false));
            lineasImprimirTurno.Add(new LineasImprimir("Isla:           " + turnoimprimir.Isla, false));
            lineasImprimirTurno.Add(new LineasImprimir("Fecha apertura: " + turnoimprimir.FechaApertura.ToString(), false));
            var reporteCierrePorTotal = new List<FacturaSiges>();
            if (turnoimprimir.FechaCierre.HasValue)
            {
                lineasImprimirTurno.Add(new LineasImprimir("Fecha cierre:   " + turnoimprimir.FechaCierre.Value.ToString(), false));
                reporteCierrePorTotal = _estacionesRepositorio.GetReporteCierrePorTotal(turnoimprimir.Id);
                var cantidadOriginal = reporteCierrePorTotal.Count;
                reporteCierrePorTotal = reporteCierrePorTotal
                    .Where(x => x.fecha >= turnoimprimir.FechaApertura && x.fecha <= turnoimprimir.FechaCierre.Value)
                    .ToList();
                if (cantidadOriginal != reporteCierrePorTotal.Count)
                {
                    Logger.Warn($"GetReporteCierrePorTotal devolvió registros fuera de rango para turno {turnoimprimir.Id}. Original: {cantidadOriginal}, Filtrado: {reporteCierrePorTotal.Count}, Apertura: {turnoimprimir.FechaApertura:yyyy-MM-dd HH:mm:ss}, Cierre: {turnoimprimir.FechaCierre.Value:yyyy-MM-dd HH:mm:ss}");
                }
            }


            var totalCantidad = 0d;
            var totalVenta = 0d;

            lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
            foreach (var turnosurtidor in turnoimprimir.turnoSurtidores)
            {
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Manguera :", turnosurtidor.Manguera.Descripcion), false));
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Combustible :", turnosurtidor.Combustible.Descripcion), false));
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Precio :", $"${turnosurtidor.Combustible.Precio:F2}"), false));
                lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Apertura :", turnosurtidor.Apertura.ToString()), false));
                if (turnoimprimir.FechaCierre.HasValue)
                {
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Cierre :", turnosurtidor.Cierre.ToString()), false));

                    Logger.Debug("reporte " + JsonConvert.SerializeObject(reporteCierrePorTotal));
                    Logger.Debug("turno surtidor " + JsonConvert.SerializeObject(turnosurtidor));
                    totalCantidad += turnosurtidor.Cierre.Value - turnosurtidor.Apertura;
                    totalVenta += (turnosurtidor.Cierre.Value - turnosurtidor.Apertura) * turnosurtidor.Combustible.Precio;
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Cantidad :", $"{turnosurtidor.Cierre - turnosurtidor.Apertura:F2}"), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total :", $"${(turnosurtidor.Cierre - turnosurtidor.Apertura) * turnosurtidor.Combustible.Precio:F2}"), false));

                }

                lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
            }
            if (turnoimprimir.FechaCierre.HasValue)
            {
                if (reporteCierrePorTotal != null && reporteCierrePorTotal.Any())
                {

                    //Por forma
                    lineasImprimirTurno.Add(new LineasImprimir($"Resumen por forma de pago", true));
                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                    var groupForma = reporteCierrePorTotal.GroupBy(x => x.codigoFormaPago);
                    Logger.Debug("facturas turno " + JsonConvert.SerializeObject(groupForma));

                    var cantidadTotalmenosEfectivo = 0d;
                    var ventaTotalmenosEfectivo = 0d;
                    foreach (var forma in groupForma)
                    {
                        if (formas.Any(x => x.Id == forma.Key) && forma.Key != 1)
                        {
                            cantidadTotalmenosEfectivo += forma.Sum(x => x.Cantidad);
                            ventaTotalmenosEfectivo += forma.Sum(x => x.Total);
                            lineasImprimirTurno.Add(new LineasImprimir(formatoTotales($"{formas.First(x => x.Id == forma.Key).Descripcion.Trim()} :", $"${string.Format("{0:#,0.00}", forma.Sum(x => x.Total))}"), false));

                        }
                    }
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales($"{formas.First(x => x.Id == 1).Descripcion.Trim()} :", $"${totalVenta - ventaTotalmenosEfectivo:F2}"), false));

                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total :", $"${totalVenta:F2}"), false));


                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));

                    lineasImprimirTurno.Add(new LineasImprimir($"Resumen por Combustibles", true));
                    //Totalizador
                    lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Combustible :", reporteCierrePorTotal.First().Combustible), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Precio :", $"${reporteCierrePorTotal.First().Precio:F2}"), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Subtotal :", $"${totalVenta:F2}"), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Calibracion :", "$0,00"), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Descuento :", $"${totalVenta:F2}"), false));
                    lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("Total :", $"${totalVenta:F2}"), false));


                }

            }
            lineasImprimirTurno.Add(new LineasImprimir(guiones.ToString(), false));

            lineasImprimirTurno.Add(new LineasImprimir("Fabricado por:" + " SIGES SOLUCIONES SAS ", true));
            lineasImprimirTurno.Add(new LineasImprimir("Nit:" + " 901430393-2 ", true));
            lineasImprimirTurno.Add(new LineasImprimir("Nombre:" + " Facturador SIGES ", true));
            lineasImprimirTurno.Add(new LineasImprimir(formatoTotales("SERIAL MAQUINA: ", firstMacAddress ?? ""), false));
            lineasImprimirTurno.Add(new LineasImprimir(".", true));
        }

        private Font printFont;
        private FacturaSiges _factura;
        private List<FormaPagoSiges> formas;
        private List<LineasImprimir> lineasImprimir;
        private TurnoSiges _turno;
        private List<LineasImprimir> lineasImprimirTurno;

        private void Imprimir(FacturaSiges factura)
        {
            Logger.Info($"Iniciando Imprimir - VentaId: {factura.ventaId}, Cara: {factura.Cara}");
            _factura = factura;

            try
            {
                Logger.Info("Generando lineas de impresion");
                getLineasImprimir();
                Logger.Info($"Lineas generadas: {lineasImprimir?.Count ?? 0}");
            }
            catch (Exception exLineas)
            {
                Logger.Error($"Error generando lineas de impresion - {exLineas.Message}");
                Logger.Error(exLineas.StackTrace);
                imprimiendo = 0;
                throw;
            }

            //imprimir
            try
            {
                printFont = new Font("Console", 9);
                PrintDocument pd = new PrintDocument();
                pd.PrintPage += new PrintPageEventHandler(pd_PrintPageOnly);
                pd.DefaultPageSettings.Margins.Bottom = 20;
                
                // Print the document.
                if (_caraImpresoras.Any(x => x.Cara == factura.Cara))
                {
                    var impresora = _caraImpresoras.First(x => x.Cara == factura.Cara).Impresora.Trim();
                    Logger.Info($"Seleccionando impresora: {impresora} para Cara: {factura.Cara}");
                    pd.PrinterSettings.PrinterName = impresora;
                }
                else
                {
                    Logger.Warn($"No se encontro impresora configurada para Cara: {factura.Cara}, usando impresora por defecto");
                }

                Logger.Info($"Enviando a imprimir en: {pd.PrinterSettings.PrinterName}");
                pd.Print();
                Logger.Info("Documento enviado a impresora exitosamente");
            }
            catch (Exception ex)
            {
                Logger.Error($"Error en impresion primaria - {ex.Message}");
                Logger.Error(ex.StackTrace);
                
                try
                {
                    Logger.Warn("Intentando impresion con impresora por defecto");
                    printFont = new Font("Console", 9);
                    PrintDocument pd = new PrintDocument();
                    pd.PrintPage += new PrintPageEventHandler(pd_PrintPageOnly);
                    pd.DefaultPageSettings.Margins.Bottom = 20;
                    pd.Print();
                    Logger.Info("Impresion por defecto exitosa");
                }
                catch (Exception exFallback)
                {
                    imprimiendo = 0;
                    Logger.Error($"Error en impresion por defecto - {exFallback.Message}");
                    Logger.Error(exFallback.StackTrace);
                    throw;
                }
            }
        }

        private void getLineasImprimir()
        {


            if (_infoEstacion.CaracteresPorPagina == 0)
            {
                _infoEstacion.CaracteresPorPagina = 40;
            }
            lineasImprimir = new List<LineasImprimir>();
            var guiones = new StringBuilder();
            guiones.Append('-', _infoEstacion.CaracteresPorPagina);
            // Iterate over the file, printing each line.
            lineasImprimir.Add(new LineasImprimir(".", true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Razon, true));
            lineasImprimir.Add(new LineasImprimir("NIT " + _infoEstacion.NIT, true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Nombre, true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Direccion, true));
            lineasImprimir.Add(new LineasImprimir(_infoEstacion.Telefono, true));
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            var infoTemp = ObtenerInfoFacturaElectronica(_factura.ventaId);
            var esFacturaElectronicaValida = EsInfoFacturaElectronicaValida(infoTemp, out var facturaElectronica);
            if (esFacturaElectronicaValida)
            {
                lineasImprimir.Add(new LineasImprimir("Factura Electrónica " + facturaElectronica[2], true));
                lineasImprimir.Add(new LineasImprimir(facturaElectronica[3], true));
                var anchoLinea = _infoEstacion.CaracteresPorPagina > 0 ? _infoEstacion.CaracteresPorPagina : 40;
                var cufeLimpio = LimpiarPrefijoCufe(facturaElectronica[4]);
                var cufePartido = PartirTextoEnBloques(cufeLimpio, anchoLinea).ToList();
                if (cufePartido.Any())
                {
                    foreach (var parte in cufePartido)
                    {
                        lineasImprimir.Add(new LineasImprimir(parte, true));
                    }
                }
            }
            else if (_factura.Consecutivo == 0)
            {

                lineasImprimir.Add(new LineasImprimir("Orden de despacho No: " + _factura.facturaPOSId, true));
            }
            else
            {
                lineasImprimir.Add(new LineasImprimir("Orden de Servicio Temporal: " + _factura.facturaPOSId, true));
            }

            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            var placa = (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _factura.Placa + "").Trim();
            var nombreCompletoTercero = ObtenerNombreCompletoTercero(_factura.Tercero);
            if (_factura.codigoFormaPago != 1)
            {

                lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a : ", nombreCompletoTercero), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", _factura.Tercero.identificacion.Trim()), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Placa : ", placa), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje : "").Trim()), false));
                var codigoInterno = _factura.CodigoInterno;
                if (codigoInterno != null)
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Cod Int : ", codigoInterno), false));
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(nombreCompletoTercero))
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a :", " CONSUMIDOR FINAL".Trim()), false));
                }
                else
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendido a : ", nombreCompletoTercero) + "", false));

                    lineasImprimir.AddRange(getPuntos(_factura.ventaId));
                }
                if (string.IsNullOrEmpty(_factura.Tercero.identificacion))
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", "222222222222".Trim()), false));
                }
                else
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Nit/C.C. : ", _factura.Tercero.identificacion.Trim()), false));
                }
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Placa : ", (!string.IsNullOrEmpty(_factura.Placa) ? _factura.Placa : _factura.Placa + "").Trim()), false));
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Kilometraje : ", (!string.IsNullOrEmpty(_factura.Kilometraje) ? _factura.Kilometraje : "").Trim()), false));
                var codigoInterno = _factura.CodigoInterno;
                if (codigoInterno != null)
                {
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Cod Int : ", codigoInterno), false));
                }
            }

            if (_factura.fechaProximoMantenimiento.HasValue)
            {
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Proximo mantenimiento : ", _factura.fechaProximoMantenimiento.Value.ToString("dd/MM/yyyy").Trim()), false));
            }

            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Fecha : ", _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss")), false));

            lineasImprimir.Add(new LineasImprimir(formatoTotales("Surtidor : ", _factura.Surtidor + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Cara : ", _factura.Cara + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Manguera : ", _factura.Mangueras + ""), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Vendedor : ", _factura.Empleado?.Trim() + ""), false));
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            if (_infoEstacion.ImpresionPDA)
            {
                lineasImprimir.Add(new LineasImprimir($"Producto: {_factura.Combustible.Trim()}", false));
                lineasImprimir.Add(new LineasImprimir($"Cantidad: {_factura.Cantidad:F2}", false));
                lineasImprimir.Add(new LineasImprimir($"Precio: ${_factura.Cantidad:F2}", false));
                lineasImprimir.Add(new LineasImprimir($"Total: ${_factura.Cantidad:F2}", false));

            }
            else
            {
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas("Producto", "   Cant.", "  Precio", "   Total") + "", false));
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas(_factura.Combustible.Trim(), $"{_factura.Cantidad:F2}", $"{_factura.Precio:F2}", $"{_factura.Total:F2}", true) + "", false));
            }
            lineasImprimir.Add(new LineasImprimir(guiones.ToString() + "", false));
            lineasImprimir.Add(new LineasImprimir("DISCRIMINACION TARIFAS IVA" + "", true));
            //  lineasImprimir.Add(new LineasImprimir(guiones.ToString() + "", false));
            if (_infoEstacion.ImpresionPDA)
            {
                lineasImprimir.Add(new LineasImprimir($"Producto: {_factura.Combustible.Trim()}", false));
                lineasImprimir.Add(new LineasImprimir($"Cantidad: ${_factura.Cantidad:F2}", false));
                lineasImprimir.Add(new LineasImprimir($"Tafira: 0 % ", false));
                lineasImprimir.Add(new LineasImprimir($"Total: {_factura.Total}", false));

            }
            else
            {
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas("Producto", "   Cant.", "  Tafira", "   Total") + "", false));
                lineasImprimir.Add(new LineasImprimir(getLienaTarifas(_factura.Combustible.Trim(), $"{_factura.Cantidad:F2}", "0%", $"{_factura.Total:F2}", true) + "", false));
            }
            lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Descuento: ", $"{_factura.Descuento:F2}"), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Subtotal sin IVA : ", $"{_factura.Subtotal:F2}"), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("Subtotal IVA :", "0,00"), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("TOTAL : ", $"{_factura.Total:F2}"), false));
            //lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));

            if (_factura.Consecutivo != 0 || impresionFormaDePagoOrdenDespacho)
            {
                var formaPagoPrincipal = formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago)?.Descripcion?.Trim() ?? "Efectivo";
                lineasImprimir.Add(new LineasImprimir(formatoTotales("Forma de pago : ", formaPagoPrincipal), false));

                var usaMultipago = _factura.codigoFormaPago2.HasValue;
                if (usaMultipago)
                {
                    if (_factura.total1.HasValue)
                    {
                        lineasImprimir.Add(new LineasImprimir(formatoTotales("Valor pago 1 : ", $"{_factura.total1.Value:F2}"), false));
                    }

                    var formaPago2 = formas.FirstOrDefault(x => x.Id == _factura.codigoFormaPago2.Value)?.Descripcion?.Trim() ?? "No informado";
                    lineasImprimir.Add(new LineasImprimir(formatoTotales("Forma de pago 2 : ", formaPago2), false));

                    if (_factura.total2.HasValue)
                    {
                        lineasImprimir.Add(new LineasImprimir(formatoTotales("Valor pago 2 : ", $"{_factura.total2.Value:F2}"), false));
                    }
                }
            }


            else if (_factura.Consecutivo != 0)
            {


                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("Resolucion de Facturacion No. ", false));
                lineasImprimir.Add(new LineasImprimir(_factura.Autorizacion + " de " + _factura.FechaInicioResolucion.ToString("dd/MM/yyyy") + " ", false));
                var numeracion = "Numeracion Autorizada por la DIAN";
                if (_factura.habilitada)
                {
                    numeracion = "Numeracion Habilitada por la DIAN";
                }
                lineasImprimir.Add(new LineasImprimir(numeracion + " ", false));
                lineasImprimir.Add(new LineasImprimir("Del " + _factura.DescripcionResolucion + "-" + _factura.Inicio + " al " + _factura.DescripcionResolucion + "-" + _factura.Final + "", false));

            }
            if (!string.IsNullOrEmpty(_infoEstacion.Linea1))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea1, false));
            }
            if (!string.IsNullOrEmpty(_infoEstacion.Linea2))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea2, false));
            }
            if (!string.IsNullOrEmpty(_infoEstacion.Linea3))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea3, false));
            }
            if (!string.IsNullOrEmpty(_infoEstacion.Linea4))
            {
                lineasImprimir.Add(new LineasImprimir(_infoEstacion.Linea4, false));
            }
            lineasImprimir.Add(new LineasImprimir("Fabricado por:" + " SIGES SOLUCIONES SAS ", true));
            lineasImprimir.Add(new LineasImprimir("Nit:" + " 901430393-2 ", true));
            lineasImprimir.Add(new LineasImprimir("Nombre:" + " Facturador SIGES ", true));
            lineasImprimir.Add(new LineasImprimir(formatoTotales("SERIAL MAQUINA: ", firstMacAddress ?? ""), false));

            var esCredito = _factura.codigoFormaPago == 6 || (_factura.codigoFormaPago2.HasValue && _factura.codigoFormaPago2.Value == 6);
            if (_infoEstacion.Rifa && !esCredito && _factura.Total >= (double)_infoEstacion.MontoMinimoRifa)
            {
                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));

                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("CÉDULA", false));

                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("CEL/TEL", false));

                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir("NRO RECIBO DE TANQUEO ", false));

                lineasImprimir.Add(new LineasImprimir(" ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
                lineasImprimir.Add(new LineasImprimir($"FECHA DE TANQUEO {DateTime.Now} ", false));
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false));
            }

            lineasImprimir.Add(new LineasImprimir(".", true));
            if (esFacturaElectronicaValida)
            {
                lineasImprimir.Add(new LineasImprimir(guiones.ToString(), false, $"https://catalogo-vpfe.dian.gov.co/User/SearchDocument?DocumentKey={facturaElectronica[4]}"));
            }
        }

        private static string ObtenerNombreCompletoTercero(FactoradorEstacionesModelo.Objetos.Tercero tercero)
        {
            if (tercero == null)
            {
                return string.Empty;
            }

            var nombre = string.IsNullOrWhiteSpace(tercero.Nombre) ? string.Empty : tercero.Nombre.Trim();
            var apellidos = string.IsNullOrWhiteSpace(tercero.Apellidos) ? string.Empty : tercero.Apellidos.Trim();
            return $"{nombre} {apellidos}".Trim();
        }

        private string ObtenerInfoFacturaElectronica(int ventaId)
        {
            if (estacionFuente == Guid.Empty)
            {
                Logger.Warn($"No se consulta factura electrónica para venta {ventaId} porque EstacionFuente es inválida.");
                return string.Empty;
            }

            Logger.Info($"Obteniendo info factura electronica - VentaId: {ventaId}");
            for (var intento = 1; intento <= 3; intento++)
            {
                try
                {
                    var token = _conexionEstacionRemota.getToken();
                    Logger.Debug($"Token obtenido, intento {intento}/3");
                    var infoTemp = _conexionEstacionRemota.GetInfoFacturaElectronica(ventaId, estacionFuente, token);

                    if (!string.IsNullOrWhiteSpace(infoTemp))
                    {
                        Logger.Info($"Info factura electronica obtenida para venta {ventaId}.");
                        return infoTemp;
                    }

                    Logger.Warn($"Info factura electronica vacia en intento {intento}/3 para venta {ventaId}.");
                }
                catch (Exception exIntento)
                {
                    Logger.Warn($"Error en intento {intento}/3 obteniendo factura electronica para venta {ventaId}: {exIntento.Message}");
                }

                if (intento < 3)
                {
                    Thread.Sleep(500);
                }
            }

            Logger.Warn($"No se pudo obtener info factura electronica para venta {ventaId} despues de 3 intentos.");
            return string.Empty;
        }

        private static bool EsInfoFacturaElectronicaValida(string infoFacturaElectronica, out string[] facturaElectronica)
        {
            facturaElectronica = Array.Empty<string>();
            if (string.IsNullOrWhiteSpace(infoFacturaElectronica))
            {
                return false;
            }

            var infoNormalizada = infoFacturaElectronica.Replace("\n\r", " ").Trim();
            var partes = infoNormalizada.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (partes.Length < 5)
            {
                return false;
            }

            facturaElectronica = partes;
            return true;
        }

        private static IEnumerable<string> PartirTextoEnBloques(string texto, int longitudBloque)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                return Array.Empty<string>();
            }

            var longitud = longitudBloque > 0 ? longitudBloque : 40;
            var bloques = new List<string>();
            for (var i = 0; i < texto.Length; i += longitud)
            {
                var largoActual = Math.Min(longitud, texto.Length - i);
                bloques.Add(texto.Substring(i, largoActual));
            }

            return bloques;
        }

        private static string LimpiarPrefijoCufe(string cufe)
        {
            if (string.IsNullOrWhiteSpace(cufe))
            {
                return string.Empty;
            }

            var cufeLimpio = cufe.Trim();
            if (cufeLimpio.StartsWith("CUFE", StringComparison.OrdinalIgnoreCase))
            {
                cufeLimpio = cufeLimpio.Substring(4).TrimStart(':', ' ', '-');
            }

            return cufeLimpio;
        }

        private IEnumerable<LineasImprimir> getPuntos(int ventaId)
        {
            try
            {
                Logger.Debug($"Obteniendo puntos fidelizacion - VentaId: {ventaId}");
                var fidelizado = _estacionesRepositorio.getFidelizado(ventaId);
                
                if (fidelizado != null)
                {
                    Logger.Debug($"Fidelizado encontrado: {fidelizado.Documento}");
                    
                    try
                    {
                        var fidelizadoRemoto = _fidelizacion.GetFidelizados(fidelizado.Documento).Result;
                        fidelizado = fidelizadoRemoto != null ? fidelizadoRemoto.FirstOrDefault() : fidelizado;
                    }
                    catch (Exception exFid)
                    {
                        Logger.Warn($"Error obteniendo fidelizado remoto: {exFid.Message}");
                    }
                    
                    if (fidelizado != null)
                    {
                        Logger.Info($"Puntos fidelizacion: {fidelizado.Puntos}");
                        return new List<LineasImprimir>() {
                            new LineasImprimir(formatoTotales("Fidelizado:", fidelizado.Nombre??fidelizado.Documento), false),
                            new LineasImprimir(formatoTotales("Puntos:", fidelizado.Puntos.ToString()), false)
                        };
                    }
                }
                
                Logger.Debug("Usuario no fidelizado");
                return new List<LineasImprimir>() { new LineasImprimir("Usuario no fidelizado", false) };
            }
            catch (Exception ex)
            {
                Logger.Error($"Error obteniendo puntos - {ex.Message}");
                Logger.Error(ex.StackTrace);
                return new List<LineasImprimir>() { new LineasImprimir("Usuario no fidelizado", false) };
            }
        }

        private void pd_PrintPageOnly(object sender, PrintPageEventArgs ev)
        {
            try
            {
                Logger.Debug($"Iniciando pd_PrintPageOnly - VentaId: {_factura.ventaId}");

                float yPos = 0;
                int count = 0;
                float leftMargin = 5;
                float topMargin = 10;
                string line = null;
                int sizePaper = ev.PageSettings.PaperSize.Width;
                int fonSizeInches = 72 / 9;
                if (_infoEstacion.CaracteresPorPagina == 0)
                {
                    _infoEstacion.CaracteresPorPagina = fonSizeInches * sizePaper / 100;
                }
                
                Logger.Debug($"Imprimiendo {lineasImprimir?.Count ?? 0} lineas");
                
                foreach (var linea in lineasImprimir)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(linea.qr))
                        {
                            count = printLine(linea.qr, ev, count, leftMargin, topMargin, false, isQr: true);
                        }
                        else
                        {
                            count = printLine(linea.linea, ev, count, leftMargin, topMargin, linea.centrada);
                        }
                    }
                    catch (Exception exLinea)
                    {
                        Logger.Error($"Error imprimiendo linea: {linea.linea} - {exLinea.Message}");
                    }
                }
                
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;

                try
                {
                    Logger.Debug($"Marcando factura como impresa - VentaId: {_factura.ventaId}");
                    _estacionesRepositorio.SetFacturaImpresa(_factura.ventaId);
                    Logger.Info($"Factura marcada como impresa - VentaId: {_factura.ventaId}");
                }
                catch (Exception exDb)
                {
                    Logger.Error($"Error actualizando estado de factura en BD - {exDb.Message}");
                    Logger.Error(exDb.StackTrace);
                }
                
                imprimiendo--;
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Error($"Error en pd_PrintPageOnly - {ex.Message}");
                Logger.Error(ex.StackTrace);
                throw;
            }

        }


        private void pd_PrintTurno(object sender, PrintPageEventArgs ev)
        {
            try
            {

                float yPos = 0;
                int count = 0;
                float leftMargin = 5;
                float topMargin = 10;
                string line = null;
                int sizePaper = ev.PageSettings.PaperSize.Width;
                int fonSizeInches = 72 / 9;
                if (_infoEstacion.CaracteresPorPagina == 0)
                {
                    _infoEstacion.CaracteresPorPagina = fonSizeInches * sizePaper / 100;
                }
                foreach (var linea in lineasImprimirTurno)
                {

                    count = printLine(linea.linea, ev, count, leftMargin, topMargin, linea.centrada);
                }
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                count = printLine(" ", ev, count, leftMargin, topMargin, false);
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;


                imprimiendo--;
                if (line != null)
                    ev.HasMorePages = true;
                else
                    ev.HasMorePages = false;
            }
            catch (Exception ex)
            {
                imprimiendo = 0;
                Logger.Error("Ex" + ex.Message);
                Logger.Error("Ex" + ex.StackTrace);
                Thread.Sleep(5000);
            }

        }

        private string formatoTotales(string v1, string v2)
        {
            var result = v1;
            var tabs = new StringBuilder();
            tabs.Append(v1);
            var whitespaces = _infoEstacion.CaracteresPorPagina - v1.Length - v2.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(v2);
            return tabs.ToString();
        }

        private string getLienaTarifas(string v1, string v2, string v3, string v4, bool after = false)
        {
            var spacesInPage = _infoEstacion.CaracteresPorPagina / 4;
            var tabs = new StringBuilder();
            if (true)
            {
                tabs.Append(v1.Substring(0, v1.Length < 12 ? v1.Length : 12));
                var whitespaces = 12 - v1.Length;
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);


                if (after)
                {
                    whitespaces = 8 - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v2.Substring(0, v2.Length < 8 ? v2.Length : 8));

                    whitespaces = 8 - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v3.Substring(0, v3.Length < 8 ? v3.Length : 8));

                    whitespaces = 12 - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v4.Substring(0, v4.Length < 12 ? v4.Length : 12));
                }
                else
                {
                    tabs.Append(v2.Substring(0, v2.Length < 8 ? v2.Length : 8));
                    whitespaces = 8 - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v3.Substring(0, v3.Length < 8 ? v3.Length : 8));
                    whitespaces = 8 - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v4.Substring(0, v4.Length < 12 ? v4.Length : 12));
                    whitespaces = 12 - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);


                }
                return tabs.ToString();
            }
            else
            {
                tabs.Append(v1.Substring(0, v1.Length < spacesInPage ? v1.Length : spacesInPage));
                var whitespaces = spacesInPage - v1.Length;
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);


                if (after)
                {
                    whitespaces = spacesInPage - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v2.Substring(0, v2.Length < spacesInPage ? v2.Length : spacesInPage));

                    whitespaces = spacesInPage - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v3.Substring(0, v3.Length < spacesInPage ? v3.Length : spacesInPage));

                    whitespaces = spacesInPage - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);
                    tabs.Append(v4.Substring(0, v4.Length < spacesInPage ? v4.Length : spacesInPage));
                }
                else
                {
                    tabs.Append(v2.Substring(0, v2.Length < spacesInPage ? v2.Length : spacesInPage));
                    whitespaces = spacesInPage - v2.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v3.Substring(0, v3.Length < spacesInPage ? v3.Length : spacesInPage));
                    whitespaces = spacesInPage - v3.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);

                    tabs.Append(v4.Substring(0, v4.Length < spacesInPage ? v4.Length : spacesInPage));
                    whitespaces = spacesInPage - v4.Length;
                    whitespaces = whitespaces < 0 ? 0 : whitespaces;
                    tabs.Append(' ', whitespaces);


                }
                return tabs.ToString();
            }
        }

        private void GenerateQRCode(string content, int size)
        {
            QrEncoder encoder = new QrEncoder(ErrorCorrectionLevel.H);
            QrCode qrCode;
            encoder.TryEncode(content, out qrCode);

            GraphicsRenderer gRenderer = new GraphicsRenderer(new FixedModuleSize(4, QuietZoneModules.Two), System.Drawing.Brushes.Black, System.Drawing.Brushes.White);
            //Graphics g = gRenderer.Draw(qrCode.Matrix);

            MemoryStream ms = new MemoryStream();
            gRenderer.WriteToStream(qrCode.Matrix, ImageFormat.Bmp, ms);

            var imageTemp = new Bitmap(ms);

            var image = new Bitmap(imageTemp, new System.Drawing.Size(new System.Drawing.Point(size, size)));

            image.Save($"{AppContext.BaseDirectory}/file.bmp", ImageFormat.Bmp);

        }

        private int printLine(string text, PrintPageEventArgs ev, int count, float leftMargin, float topMargin, bool center = false, bool isQr = false)
        {
            if (center)
            {
                var whitespaces = (_infoEstacion.CaracteresPorPagina - text.Length) / 2;
                var tabs = new StringBuilder();
                whitespaces = whitespaces < 0 ? 0 : whitespaces;
                tabs.Append(' ', whitespaces);
                text = tabs.ToString() + text;
            }
            float yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));

            if (isQr)
            {
                GenerateQRCode(text, 160);
                Image newImage = Image.FromFile($"{AppContext.BaseDirectory}/file.bmp");

                RectangleF srcRect = new RectangleF(0, 0, 160F, 160F);
                GraphicsUnit units = GraphicsUnit.Pixel;
                ev.Graphics.DrawImage(newImage, leftMargin, yPos, srcRect, units);
            }
            else
            {
                ev.Graphics.DrawString(text, printFont, Brushes.Black, leftMargin, yPos, new StringFormat() { });
            }
            count++;
            return count;
        }

    }

    public class LineasImprimir
    {
        public LineasImprimir(string linea, bool centrada, string qr = null)
        {
            this.linea = linea;
            this.centrada = centrada;
            this.qr = qr;
        }

        public string linea;
        public bool centrada;
        public string qr;
    }
}