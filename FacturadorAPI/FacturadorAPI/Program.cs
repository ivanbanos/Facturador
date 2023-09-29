using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi;
using MachineUtilizationApi.Application;
using MachineUtilizationApi.Extensions;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Swagger Machine Utilization",
        Description = "Machine Utilization"
    });

    c.CustomSchemaIds(x => x.FullName);

});
builder.Services.AddMediatR(typeof(IApplicationAnchor));
builder.Services.Configure<InfoEstacion>(options => builder.Configuration.GetSection("InfoEstacion").Bind(options));
builder.Services.Configure<ConnectionStringSettings>(options => builder.Configuration.GetSection("ConnectionStringSettings").Bind(options));

builder.Services.AddScoped<IConexionEstacionRemota, ConexionEstacionRemota>();
builder.Services.AddRepositories();

//builder.Services.AddSingleton<IAuthentication, JWTAuthentication>();
builder.Services.AddMvc(opt =>
{
    opt.Filters.Add(new ApiExceptionFilter());
});

builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


//app.UseMiddleware<JwtMiddleware>();
app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();

app.UseCors("corsapp");
app.Run();

