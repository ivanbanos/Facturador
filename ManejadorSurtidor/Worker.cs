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
using ManejadorSurtidor.Messages;
using System;
using ControladorEstacion.Messages;
using FactoradorEstacionesModelo.Siges;

namespace ManejadorSurtidor
{
    public class Worker : BackgroundService, IObserver<string>
    {
        private readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IOptions<Sicom> _options;

        private readonly ISicomConection _sicomConection;
        private readonly IMessageProducer _messageProducer;
        private readonly IFidelizacion _fidelizacion;
        private readonly Islas _islas;

        public override void Dispose()
        {
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await base.StartAsync(cancellationToken);
            _logger.Log(NLog.LogLevel.Info, "Iniciando");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            _logger.Log(NLog.LogLevel.Info, "Cerrando");
        }
        public Worker(ILogger<Worker> logger, IEstacionesRepositorio estacionesRepositorio, IOptions<Sicom> options, ISicomConection sicomConection, IMessageProducer messageProducer, IFidelizacion fidelizacion,
            //IMessagesReceiver messageReceiver, 
            Islas islas)
        {

           // ((IObservable<string>)messageReceiver).Subscribe(this);
            _estacionesRepositorio = estacionesRepositorio;
            _options = options;
            _sicomConection = sicomConection;
            _messageProducer = messageProducer;
            _fidelizacion = fidelizacion;
            _islas = islas;
        }
    

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Log(NLog.LogLevel.Info, "Buscando Caras");
            var surtidores = _estacionesRepositorio.GetSurtidoresSiges();

            _logger.Log(NLog.LogLevel.Info, $"Caras {JsonConvert.SerializeObject(surtidores)}");

            var tasks = surtidores
                .GroupBy(x => x.Puerto)
                .Select(group => RunOperadorCaraLoop(group.ToList(), stoppingToken))
                .ToList();

            await Task.WhenAll(tasks);
        }

        private async Task RunOperadorCaraLoop(List<SurtidorSiges> surtidores, CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var operador = new OperadorCara(_logger, surtidores, _estacionesRepositorio, _options, _sicomConection, _messageProducer, _fidelizacion, _islas);
                    await operador.OperarCara(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Log(NLog.LogLevel.Error, $"OperadorCara stopped unexpectedly. Restarting. Error: {ex.Message}");
                    _logger.Log(NLog.LogLevel.Error, ex.StackTrace ?? string.Empty);
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(string value)
        {
            _islas.AgregarIsla(value);
        }
    }
}