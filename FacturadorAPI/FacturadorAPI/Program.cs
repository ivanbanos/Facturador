using MachineUtilizationApi;
using MachineUtilizationApi.Application;
using MachineUtilizationApi.Authtentication;
using MachineUtilizationApi.Extensions;
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
builder.Services.AddRepositories();
builder.Services.AddSingleton<IConfigureOptions<SecretSettings>, ConfigureSecretSettings>();

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

