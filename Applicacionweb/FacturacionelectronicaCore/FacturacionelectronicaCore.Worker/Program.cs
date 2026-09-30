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
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("Logs/worker-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        shared: true)
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .CreateLogger();

try
{
    Log.Information("Starting Worker Service...");

    var builder = Host.CreateApplicationBuilder(args);
    
    // Clear default MEL providers and wire Serilog explicitly
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog(Log.Logger, dispose: true);
    
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
