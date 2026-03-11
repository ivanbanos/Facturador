using EstacionesServicio.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using FacturacionelectronicaCore.Worker;
using Serilog;

// Configure Serilog from appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting Worker Service...");

    var builder = Host.CreateApplicationBuilder(args);
    
    // Add Serilog
    builder.Services.AddSerilog();
    
    builder.Services.AddFacturaElectronica(builder.Configuration);
    builder.Services.AddServicesDependencies(builder.Configuration);
    builder.Services.AddSingleton<ResolucionNumber>();
    // Register AutoMapper so IMapper is available to business services
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    builder.Services.AddHostedService<Worker>();

    // Configure for Windows Service with extended timeout and better error handling
    builder.Services.Configure<HostOptions>(hostOptions =>
    {
        hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
        hostOptions.ShutdownTimeout = TimeSpan.FromSeconds(30);
        hostOptions.ServicesStartConcurrently = false;
        hostOptions.ServicesStopConcurrently = false;
    });

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
