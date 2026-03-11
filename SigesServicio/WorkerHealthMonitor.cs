using Microsoft.Extensions.Hosting;

namespace SigesServicio
{
    public sealed class WorkerHealthMonitor : BackgroundService
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IWorkerHeartbeatTracker _heartbeat;

        private static readonly IReadOnlyDictionary<string, TimeSpan> WorkerTimeouts =
            new Dictionary<string, TimeSpan>(StringComparer.OrdinalIgnoreCase)
            {
                ["WorkerImpresion"] = TimeSpan.FromMinutes(3),
                ["FacturasWorker"] = TimeSpan.FromMinutes(8),
                ["ObtenerVehiculosWorker"] = TimeSpan.FromHours(13)
            };

        public WorkerHealthMonitor(IWorkerHeartbeatTracker heartbeat)
        {
            _heartbeat = heartbeat;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info("WorkerHealthMonitor iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var entry in WorkerTimeouts)
                    {
                        var workerName = entry.Key;
                        var timeout = entry.Value;
                        var lastSeen = _heartbeat.GetLastSeen(workerName);

                        if (!lastSeen.HasValue)
                        {
                            continue;
                        }

                        var elapsed = DateTimeOffset.UtcNow - lastSeen.Value;
                        if (elapsed <= timeout)
                        {
                            continue;
                        }

                        var message = $"WorkerHealthMonitor detectó worker detenido: {workerName}. Último latido hace {elapsed.TotalSeconds:F0}s (timeout {timeout.TotalSeconds:F0}s). Se reinicia el proceso para recuperación.";
                        Logger.Error(message);
                        Environment.Exit(2);
                    }
                }
                catch (Exception ex)
                {
                    var rootException = ex is AggregateException aggregateException
                        ? aggregateException.Flatten().InnerException ?? ex
                        : ex;

                    Logger.Error("Ex" + rootException.Message);
                    Logger.Error("Ex" + rootException.StackTrace);
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}
