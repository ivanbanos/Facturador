using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FacturacionelectronicaCore.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Secretos y datos del cliente en archivos no versionados (ver appsettings.example.json).
                    // Se re-agregan variables de entorno y argumentos para que sigan teniendo prioridad (Docker).
                    var entorno = context.HostingEnvironment.EnvironmentName;
                    config.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{entorno}.Local.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.UseKestrel();
                    //webBuilder.UseContentRoot(Directory.GetCurrentDirectory());

                    //webBuilder.UseIISIntegration();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
