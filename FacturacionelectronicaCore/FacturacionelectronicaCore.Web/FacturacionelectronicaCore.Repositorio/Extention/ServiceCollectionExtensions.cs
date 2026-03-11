using EstacionesServicio.Repositorio.Common.SQLHelper;
using EstacionesServicio.Repositorio.Repositorios;
using FacturacionelectronicaCore.Repositorio;
using FacturacionelectronicaCore.Repositorio.Mongodb;
using FacturacionelectronicaCore.Repositorio.Repositorios;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EstacionesServicio.Respositorio.Extention
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRespositoryDependencies(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            services.AddTransient<ISQLHelper>(s => new SQLHelper(configuration.GetConnectionString("ConnStr")));
            services.Configure<RepositorioConfig>(options => configuration.GetSection("RepositorioConfig").Bind(options)); 
            services.AddTransient<IMongoHelper>(s => new MongoHelper(configuration));
            services.AddScoped<IUsuarioRespositorio, UsuarioRepositorio>();
            services.AddScoped<ITerceroRepositorio, TerceroRepositorio>();
            services.AddScoped<IOrdenDeDespachoRepositorio, OrdenDeDespachoRepositorio>();
            services.AddScoped<IResolucionRepositorio, ResolucionRepositorio>();
            services.AddScoped<ITipoIdentificacionRepositorio, TipoIdentificacionRepositorio>();
            services.AddScoped<IEstacionesRepository, EstacionesRepository>();
            services.AddScoped<ICanastillaRepositorio, CanastillaRepositorio>();
            services.AddScoped<IFacturaCanastillaRepository, FacturaCanastillaRepository>();
            services.AddScoped<IEmpleadoRepositorio, EmpleadoRepositorio>();
            services.AddScoped<ITurnoRepositorio, TurnoRepositorio>();
            services.AddScoped<ICupoRepositorio, CupoRepositorio>();
            return services;
        }
    }
}
