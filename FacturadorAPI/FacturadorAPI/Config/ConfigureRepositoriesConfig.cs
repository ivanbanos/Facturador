using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace MachineUtilizationApi.Config
{
    public class ConfigureRepositoriesConfig : IConfigureOptions<RepositoriesConfig>
    {
        private readonly IConfiguration _configuration;

        public ConfigureRepositoriesConfig(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Configure(RepositoriesConfig options)
        {
            _ = options ?? throw new ArgumentNullException(nameof(options));
            _configuration.Bind("RepositoriesConfig", options);
            if (options.Timeout <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(options.Timeout)} should be greater than zero but was:{options.Timeout}");
            }
        }
    }
}