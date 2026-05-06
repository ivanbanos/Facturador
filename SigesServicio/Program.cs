using EnviadorInformacionService;
using EnviadorInformacionService.Contabilidad;
using FactoradorEstacionesModelo;
using FacturadorEstacionesPOSWinForm.Repo;
using FacturadorEstacionesRepositorio;
using ManejadorSurtidor;
using ManejadorSurtidor.Messages;
using ManejadorSurtidor.SICOM;
using Modelo;
using NLog.Targets;
using SigesServicio;

var config = new NLog.Config.LoggingConfiguration();

// Targets where to log to: File and Console
var logfile = new NLog.Targets.FileTarget("logfile")
{
    Layout = "${longdate} ${logger} ${message} ${exception}",
    FileName = "${basedir}/logs/${shortdate}.log",
    ArchiveNumbering = ArchiveNumberingMode.DateAndSequence,
    ArchiveAboveSize = 5000000,
};
var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
// Rules for mapping loggers to targets            
config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Error, logconsole);
config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Error, logfile);

// Apply config           
NLog.LogManager.Configuration = config;
var logger = NLog.LogManager.GetCurrentClassLogger();
logger.Info("Iniciando");

await Task.Delay(2000, default);
try
{
    IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<HostOptions>(options =>
        {
            options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        });

        services.Configure<InfoEstacion>(options => hostContext.Configuration.GetSection("InfoEstacion").Bind(options));
        services.Configure<Siesa>(options => hostContext.Configuration.GetSection("Siesa").Bind(options));
        services.AddSingleton<IEstacionesRepositorio, EstacionesRepositorioSqlServer>();
        services.Configure<ConnectionStrings>(options => hostContext.Configuration.GetSection("ConnectionStrings").Bind(options));
        services.Configure<List<ServicioSIGES.CaraImpresora>>(options => hostContext.Configuration.GetSection("CarasImpresoras").Bind(options));
        services.AddSingleton<ISicomConection, SicomConection>();
        services.AddSingleton<IMessageProducer, RabbitMQProducer>();
        services.AddSingleton<IFidelizacion, FidelizacionConexionApi>();
        services.Configure<Sicom>(options => hostContext.Configuration.GetSection("Sicom").Bind(options));
        services.Configure<InformacionCuenta>(options => hostContext.Configuration.GetSection("InformacionCuenta").Bind(options));

        services.AddSingleton<IConexionEstacionRemota, ConexionEstacionRemota>();
     services.AddSingleton<IWorkerHeartbeatTracker, WorkerHeartbeatTracker>();
    //     services.AddHostedService<WorkerImpresion>();
    //     // services.AddHostedService<SubirVentasWorker>();
    //     services.AddHostedService<ObtenerVehiculosWorker>();
    //     // // services.AddHostedService<CanastillaWorker>();
    //     services.AddHostedService<FacturasWorker>();
    // services.AddHostedService<WorkerHealthMonitor>();
        services.AddHostedService<SiesaWorker>();

    })

    .UseWindowsService()
    .Build();

    await host.RunAsync();
}
catch (Exception ex)
{
    //NLog: catch setup errors
    logger.Info(ex.Message);
    logger.Info(ex.StackTrace);
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}


