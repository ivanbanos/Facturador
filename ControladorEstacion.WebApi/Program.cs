using System.Threading.RateLimiting;
using ControladorEstacion.WebApi.Configuration;
using ControladorEstacion.WebApi.Hubs;
using ControladorEstacion.WebApi.Services;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<LegacyApiOptions>(builder.Configuration.GetSection(LegacyApiOptions.SectionName));

builder.Services
    .AddHttpClient<ILegacyFacturadorClient, LegacyFacturadorClient>()
    .ConfigureHttpClient(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    });

// Register SignalR hub
builder.Services.AddSignalR();

// Register PDF service
builder.Services.AddScoped<IReportePdfService, ReportePdfService>();

// Register RabbitMQ consumer as hosted service (runs in background)
builder.Services.AddHostedService<RabbitMQConsumerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("reportes", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 10;
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
if (allowedOrigins.Length == 0)
{
    throw new InvalidOperationException("Configure at least one origin in Cors:AllowedOrigins");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Ocurrio un error interno." });
        });
    });
}

app.UseHttpsRedirection();
app.UseCors("frontend");
app.UseRateLimiter();
app.MapControllers();
app.MapHub<SurtidorHub>("/ws/surtidores");

app.Run();
