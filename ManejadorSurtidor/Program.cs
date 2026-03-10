
using FacturadorEstacionesRepositorio;
using NLog.Targets;
using ManejadorSurtidor.SICOM;
using ManejadorSurtidor.Messages;
using ControladorEstacion.Messages;
using Modelo;

namespace ManejadorSurtidor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
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
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logfile);

            // Apply config           
            NLog.LogManager.Configuration = config;
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info("Iniciando");

            await Task.Delay(5000, default);
            try
            {
                var host = CreateHostBuilder(args).UseWindowsService().Build();


                host.Run();
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
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", optional: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<IEstacionesRepositorio, EstacionesRepositorioSqlServer>();
                    services.AddSingleton<IMessageProducer, RabbitMQProducer>();
                    services.Configure<ConnectionStrings>(options => hostContext.Configuration.GetSection("ConnectionStrings").Bind(options));
                    services.AddTransient<ISicomConection, SicomConection>();
                    services.AddTransient<IMessagesReceiver, RabbitMQMessagesReceiver>();
                    services.AddTransient<IFidelizacion, FidelizacionConexionApi>();
                    services.AddSingleton<Islas>();
                    services.Configure<Sicom>(options => hostContext.Configuration.GetSection("Sicom").Bind(options));
                    services.AddHostedService<Worker>();
                    services.Configure<InfoEstacion>(options => hostContext.Configuration.GetSection("InfoEstacion").Bind(options));
                    services.Configure<List<ServicioSIGES.CaraImpresora>>(options => hostContext.Configuration.GetSection("CarasImpresoras").Bind(options));

                }).UseWindowsService();
    }
}
