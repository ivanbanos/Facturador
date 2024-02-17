using Microsoft.Extensions.Options;

namespace MachineUtilizationApi.Repository
{
    public class ConfigureConnectionStringSettings : IConfigureOptions<ConnectionStringSettings>
    {
        private IConfiguration _configuration;
        private readonly ILogger<ConfigureConnectionStringSettings> _logger;

        public ConfigureConnectionStringSettings(IConfiguration configuration, ILogger<ConfigureConnectionStringSettings> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Configure(ConnectionStringSettings options)
        {
            _configuration.Bind("ConnectionStrings", options);

            if (string.IsNullOrEmpty(options.EstacionSiges))
            {
                _logger.LogWarning("DataBaseConnString is missing");
            }
            if (string.IsNullOrEmpty(options.Estacion))
            {
                _logger.LogWarning("DataBaseConnString is missing");
            }
            if (string.IsNullOrEmpty(options.Facturacion))
            {
                _logger.LogWarning("DataBaseConnString is missing");
            }

        }
    }
}
