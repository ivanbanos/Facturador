namespace FacturacionelectronicaCore.Web.HealthChecks
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson;
    using MongoDB.Driver;

    /// <summary>
    /// Verifica que SQL Server y MongoDB respondan. Lo usan Docker y el proxy para saber si el API está listo.
    /// </summary>
    public class BaseDatosHealthCheck : IHealthCheck
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        private readonly IConfiguration _configuration;
        private readonly ILogger<BaseDatosHealthCheck> _logger;

        public BaseDatosHealthCheck(IConfiguration configuration, ILogger<BaseDatosHealthCheck> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var fallas = new List<string>();

            if (!await SqlDisponible(cancellationToken))
            {
                fallas.Add("sqlserver");
            }

            if (!await MongoDisponible(cancellationToken))
            {
                fallas.Add("mongodb");
            }

            // No se exponen detalles de conexión en la respuesta, solo qué componente falló.
            return fallas.Count == 0
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Sin conexión: " + string.Join(", ", fallas));
        }

        private async Task<bool> SqlDisponible(CancellationToken cancellationToken)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("ConnStr"))
                {
                    ConnectTimeout = (int)Timeout.TotalSeconds
                };
                using (var connection = new SqlConnection(builder.ConnectionString))
                using (var command = new SqlCommand("SELECT 1", connection))
                {
                    await connection.OpenAsync(cancellationToken);
                    await command.ExecuteScalarAsync(cancellationToken);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check: SQL Server no disponible");
                return false;
            }
        }

        private async Task<bool> MongoDisponible(CancellationToken cancellationToken)
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString(_configuration.GetConnectionString("MongoDB"));
                settings.ServerSelectionTimeout = Timeout;
                settings.ConnectTimeout = Timeout;
                var client = new MongoClient(settings);
                await client.GetDatabase("admin").RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check: MongoDB no disponible");
                return false;
            }
        }
    }
}
