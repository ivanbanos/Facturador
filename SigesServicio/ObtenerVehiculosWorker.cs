using FacturadorEstacionesRepositorio;
using NLog;
using Microsoft.Extensions.Options;
using ManejadorSurtidor.SICOM;
using FactoradorEstacionesModelo.Siges;
using Modelo;
using System.IO;

namespace ManejadorSurtidor
{
    public class ObtenerVehiculosWorker : BackgroundService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IOptions<Sicom> _options;
        private readonly IOptions<InfoEstacion> _optionsInfo;

        private readonly ISicomConection _sicomConection;
        private readonly SigesServicio.IWorkerHeartbeatTracker _heartbeat;

        public override void Dispose()
        {
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            Logger.Info("ObtenerVehiculosWorker StartAsync");
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.Info("ObtenerVehiculosWorker StopAsync");
            await base.StopAsync(cancellationToken);
        }
        public ObtenerVehiculosWorker(IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection, IOptions<InfoEstacion> optionsInfo, SigesServicio.IWorkerHeartbeatTracker heartbeat)
        {
            _estacionesRepositorio = estacionesRepositorio;
            _options = options;
            _sicomConection = sicomConection;
            _optionsInfo = optionsInfo;
            _heartbeat = heartbeat;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info("ObtenerVehiculosWorker iniciado");
            _heartbeat.MarkStarted(nameof(ObtenerVehiculosWorker));
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _heartbeat.Pulse(nameof(ObtenerVehiculosWorker));
                    Logger.Info("ObtenerVehiculosWorker ciclo iniciado");
                    Logger.Info("Bajando SUIC");
                    string suic = await _sicomConection.GetInfoCarros();
                    if (suic == "Fail")
                    {
                        var suicFilePath = GetSuicFilePath();
                        if (!File.Exists(suicFilePath))
                        {
                            Logger.Warn($"No se encontró archivo de respaldo SUIC en la ruta {suicFilePath}");
                            await Task.Delay(1000 * 60 * 5, stoppingToken);
                            continue;
                        }

                        suic = File.ReadAllText(suicFilePath);
                    }
                    await setCarInDatabase(suic);
                    Logger.Info("ObtenerVehiculosWorker ciclo finalizado");

                    await Task.Delay(1000 * 60 * 60 * 12, stoppingToken);
                } catch(Exception ex)
                {
                    var rootException = ex is AggregateException aggregateException
                        ? aggregateException.Flatten().InnerException ?? ex
                        : ex;

                    Logger.Error("Ex" + rootException.Message);
                    Logger.Error("Ex" + rootException.StackTrace);
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(1000 * 60 * 5, stoppingToken);
                    }
                }
            }
        }

        private async Task setCarInDatabase(string suic)
        {
            if (string.IsNullOrWhiteSpace(suic))
            {
                Logger.Warn("Contenido SUIC vacío; no se procesan vehículos.");
                return;
            }

            var lines = suic.Split('\n');
            var vehiculos = new List<VehiculoSuic>();
            for(var i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Split(',');
                try
                {

                    vehiculos.Add(new VehiculoSuic()
                    {
                        idrom = line[0],
                        fechaInicio = string.IsNullOrEmpty(line[1])?DateTime.MinValue :DateTime.Parse(line[1]),
                        fechaFin = string.IsNullOrEmpty(line[2]) ? DateTime.MinValue : DateTime.Parse(line[2]),
                        placa = line[3],
                        vin = line[4],
                        servicio = line[5],
                        capacidad = line[6],
                        estado = string.IsNullOrEmpty(line[7]) ? 0 : Int32.Parse(line[7]),
                        motivo = line[8],
                        motivoTexto = line[9],
                    });
                } catch(Exception ex)
                {
                    Logger.Error("Ex" + ex.Message);
                    Logger.Error("Ex" + ex.StackTrace);
                    
                }
                try
                {

                    if (vehiculos.Count() == 10000)
                    {

                        Logger.Info($"procesado {i} de {lines.Length}");
                        _estacionesRepositorio.ActualizarCarros(vehiculos);
                        vehiculos.Clear();
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error("Ex" + ex.Message);
                    Logger.Error("Ex" + ex.StackTrace);
                    await Task.Delay(1000 * 60 * 5);
                }
            }
            try
            {

                if (vehiculos.Count() < 10000)
                {

                    Logger.Info($"procesado {vehiculos.Count()} de {lines.Length}");
                    _estacionesRepositorio.ActualizarCarros(vehiculos);
                    vehiculos.Clear();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Ex" + ex.Message);
                Logger.Error("Ex" + ex.StackTrace);
                await Task.Delay(1000 * 60 * 5);
            }
           
        }

        private string GetSuicFilePath()
        {
            var configuredPath = _optionsInfo.Value.ArchivoSiCOM;
            var basePath = string.IsNullOrWhiteSpace(configuredPath)
                ? AppContext.BaseDirectory
                : configuredPath;

            if (!Path.IsPathRooted(basePath))
            {
                basePath = Path.Combine(AppContext.BaseDirectory, basePath);
            }

            var fullDirectory = Path.GetFullPath(basePath);
            return Path.Combine(fullDirectory, "SUIC.txt");
        }
    }
}