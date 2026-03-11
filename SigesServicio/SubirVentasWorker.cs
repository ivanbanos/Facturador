using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FacturadorEstacionesRepositorio;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using Microsoft.Extensions.Options;
using ManejadorSurtidor.SICOM;
using System;

namespace ManejadorSurtidor
{
    public class SubirVentasWorker : BackgroundService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IOptions<Sicom> _options;

        private readonly ISicomConection _sicomConection;
        public SubirVentasWorker(ILogger<SubirVentasWorker> logger, IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _options = options;
            _sicomConection = sicomConection;
        }
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var ventas = _estacionesRepositorio.getVentaSinSubirSICOM();

                //Logger.Log(NLog.LogLevel.Error, $"Subiendo. {ventas.Count} ventas a sicom. Ventas {JsonConvert.SerializeObject(ventas.Select(x => x.ventaId))}");
                if (ventas.Any(x => x.IButton != ""))
                {

                    foreach (var venta in ventas.Where(x => x.IButton != ""))
                    {
                        try
                        {

                            if(await _sicomConection.enviarVenta(venta.IButton, (float)venta.Cantidad, venta.fecha) == true)
                            {

                                _estacionesRepositorio.actualizarVentaSubidaSicom(venta.ventaId);
                                //Logger.Log(NLog.LogLevel.Error, $"Subida {venta.ventaId}");
                            } else
                            {

                                //Logger.Log(NLog.LogLevel.Error, $"No Subida {venta.ventaId}");
                            }

                        }
                        catch (Exception ex) {

                           // Logger.Log(NLog.LogLevel.Error, $"Error. {ex.Message}.{ex.StackTrace}");
                        }
                    }
                }
                Thread.Sleep(300000);
            }
        });


    }
}