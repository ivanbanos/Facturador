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

namespace SigesServicio
{
    public class FacturasWorker : BackgroundService
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly InfoEstacion _infoEstacion;
        private readonly InformacionCuenta _informacionCuenta;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly IFidelizacion _fidelizacon;
        private readonly IWorkerHeartbeatTracker _heartbeat;

        public override void Dispose()
        {
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Info("FacturasWorker StartAsync");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.Info("FacturasWorker StopAsync");
            await base.StopAsync(cancellationToken);
        }
        public FacturasWorker(IEstacionesRepositorio estacionesRepositorio, IOptions<InformacionCuenta> informacionCuenta, IOptions<InfoEstacion> infoEstacion, IConexionEstacionRemota conexionEstacionRemota, IFidelizacion fidelizacon, IWorkerHeartbeatTracker heartbeat)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _infoEstacion = infoEstacion.Value;
            _informacionCuenta = informacionCuenta.Value;
            _conexionEstacionRemota = conexionEstacionRemota;
            _fidelizacon = fidelizacon;
            _heartbeat = heartbeat;
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info("FacturasWorker iniciado");
            _heartbeat.MarkStarted(nameof(FacturasWorker));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _heartbeat.Pulse(nameof(FacturasWorker));
                    //Logger.Debug("FacturasWorker ciclo iniciado");
                    EnviarFacturas();
                    //Logger.Debug("FacturasWorker ciclo finalizado");
                    await Task.Delay(300000, stoppingToken);
                }
                catch (Exception ex)
                {
                    var rootException = ex is AggregateException aggregateException
                        ? aggregateException.Flatten().InnerException ?? ex
                        : ex;

                    Logger.Error("Ex" + rootException.Message);
                    Logger.Error("Ex" + rootException.StackTrace);
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(300000, stoppingToken);
                    }
                }
            }
        }



        private void EnviarFacturas()
        {
            Logger.Info("FacturasWorker - iniciando envío de facturas");
            string token = _conexionEstacionRemota.getToken();
            var estacionFuente = Guid.Parse(_infoEstacion.EstacionFuente);

            var ResolucionesRemota = _conexionEstacionRemota.GetResolucionEstacion(estacionFuente, token);
            var resolucion = _estacionesRepositorio.BuscarResolucionActiva(ResolucionesRemota);


            var terceros = _estacionesRepositorio.BuscarTercerosNoEnviados();
            //Logger.Debug($"FacturasWorker - terceros pendientes: {terceros.Count}");
            if (terceros.Any(t => t.identificacion != null))
            {
                terceros = terceros.Where(t => t.identificacion != null).ToList();

                var okTercero = _conexionEstacionRemota.EnviarTerceros(terceros, token);
                if (okTercero)
                {
                    _estacionesRepositorio.ActuralizarTercerosEnviados(terceros.Select(x => x.terceroId));
                }
                else
                {

                    Logger.Info("No subieron tereceros");
                }
            }
            var facturas = _estacionesRepositorio.BuscarFacturasNoEnviadasSiges();
            //Logger.Debug($"FacturasWorker - facturas pendientes: {facturas.Count}");
            if (facturas.Any())
            {
                var formas = _estacionesRepositorio.BuscarFormasPagosSiges();

                var okFacturas = _conexionEstacionRemota.EnviarFacturas(facturas, formas, estacionFuente, token);
                if (okFacturas)
                {
                    _estacionesRepositorio.ActuralizarFacturasEnviados(facturas.Select(x => x.ventaId));
                }
                else
                {

                    Logger.Info("No subieron facturas");
                }
            }

            var facturasFechas = _estacionesRepositorio.BuscarFechasReportesNoEnviadasSiges();
            //Logger.Debug($"FacturasWorker - fechas reporte pendientes: {facturasFechas.Count}");
            if (facturasFechas.Any())
            {

                var okFacturasFechas = _conexionEstacionRemota.AgregarFechaReporteFactura(facturasFechas.Where(x=>x.FechaReporte != null).ToList(), estacionFuente, token);
                if (okFacturasFechas)
                {
                    _estacionesRepositorio.ActuralizarFechasReportesEnviadas(facturasFechas.Select(x => x.IdVentaLocal));
                }
                else
                {

                    Logger.Warn("No subieron facturas");
                }
            }

            //var tercerosRecibidos = _conexionEstacionRemota.RecibirTercerosActualizados(Guid.Parse(_infoEstacion.EstacionFuente), token);
            //foreach (var tercero in tercerosRecibidos)
            //{

            //    _estacionesRepositorio.ActuralizarTerceros(tercero);

            //}

            var facturasIdImprimir = _conexionEstacionRemota.RecibirFacturasImprimir(estacionFuente, token);
            var ordenesIdImprimir = _conexionEstacionRemota.RecibirOrdenesImprimir(estacionFuente, token);
           // Logger.Debug($"FacturasWorker - ordenes imprimir: {ordenesIdImprimir.Count()}, facturas imprimir: {facturasIdImprimir.Count()}");


            foreach (var orden in ordenesIdImprimir)
            {
                _estacionesRepositorio.MandarImprimir(orden.IdVentaLocal);
            }
            foreach (var factura in facturasIdImprimir)
            {
                var tieneFacturaElectronica = TieneFacturaElectronicaDisponible(factura.IdVentaLocal, estacionFuente, token);
                if (tieneFacturaElectronica)
                {
                    Logger.Info($"Factura {factura.IdVentaLocal} con info de factura electrónica disponible. Se manda a imprimir.");
                }
                else
                {
                    Logger.Warn($"Factura {factura.IdVentaLocal} sin info de factura electrónica después de reintentos. Se manda a impresión temporal.");
                }

                _estacionesRepositorio.MandarImprimir(factura.IdVentaLocal);
            }

            Logger.Info("FacturasWorker - envío finalizado");

        }

        private bool TieneFacturaElectronicaDisponible(int ventaId, Guid estacionFuente, string token)
        {
            for (var intento = 1; intento <= 3; intento++)
            {
                try
                {
                    var tokenActual = string.IsNullOrWhiteSpace(token) ? _conexionEstacionRemota.getToken() : token;
                    var infoFacturaElectronica = _conexionEstacionRemota.GetInfoFacturaElectronica(ventaId, estacionFuente, tokenActual);
                    if (EsInfoFacturaElectronicaValida(infoFacturaElectronica))
                    {
                        return true;
                    }

                    Logger.Warn($"Factura {ventaId} sin información electrónica válida en intento {intento}/3.");
                }
                catch (Exception ex)
                {
                    Logger.Warn($"Error validando factura electrónica para venta {ventaId}, intento {intento}/3: {ex.Message}");
                }

                token = string.Empty;
                if (intento < 3)
                {
                    Thread.Sleep(500);
                }
            }

            return false;
        }

        private static bool EsInfoFacturaElectronicaValida(string infoFacturaElectronica)
        {
            if (string.IsNullOrWhiteSpace(infoFacturaElectronica))
            {
                return false;
            }

            var infoNormalizada = infoFacturaElectronica.Replace("\n\r", " ").Trim();
            var partes = infoNormalizada.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return partes.Length >= 5;
        }
    }

}

