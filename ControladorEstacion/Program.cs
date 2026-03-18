using ControladorEstacion.Messages;
using EnviadorInformacionService;
using FacturadorEstacionesPOSWinForm;
using FacturadorEstacionesPOSWinForm.Repo;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Modelo;
using NLog.Targets;
using NLog.Web;
using System.Reflection.Metadata;

namespace ControladorEstacion
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // NLog: setup the logger first to catch all errors
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


            try
            {
                logger.Info("Starting");
                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var host = CreateHostBuilder(args).Build();

                var services = host.Services;
                var mainForm = services.GetRequiredService<Form1>();
                

                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                //NLog: catch setup errors
                //NLog: catch setup errors
                logger.Info(ex.Message);
                logger.Info(ex.StackTrace);
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
            Console.Read();
        }


        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", optional: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IEstacionesRepositorio, EstacionesRepositorioSqlServer>();
                    services.AddSingleton<IConexionEstacionRemota, ConexionEstacionRemota>();
                    services.AddSingleton<Form1>();
                    services.Configure<ConnectionStrings>(options => hostContext.Configuration.GetSection("ConnectionStrings").Bind(options));
                    services.Configure<InfoEstacion>(options => hostContext.Configuration.GetSection("InfoEstacion").Bind(options));
                });
        }
    }
}