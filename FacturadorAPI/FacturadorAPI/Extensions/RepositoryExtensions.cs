using MachineUtilizationApi.Config;
using MachineUtilizationApi.Repository;
using Microsoft.Extensions.Options;

namespace MachineUtilizationApi.Extensions
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IConfigureOptions<ConnectionStringSettings>, ConfigureConnectionStringSettings>();
            services.AddSingleton<IConfigureOptions<RepositoriesConfig>, ConfigureRepositoriesConfig>();
            services.AddScoped<IDataBaseHandler, MsSqlDataBaseHandler>();
            return services;
        }
    }
}
