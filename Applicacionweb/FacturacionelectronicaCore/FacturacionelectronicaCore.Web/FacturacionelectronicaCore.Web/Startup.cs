using EstacionesServicio.Negocio.Extention;
using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using FacturacionelectronicaCore.Web.Authtentication;
using FacturacionelectronicaCore.Web.Filters;
using FacturacionelectronicaCore.Web.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FacturacionelectronicaCore.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            JsonConfig = ConfigurationRoot;
        }

        public IConfiguration Configuration { get; }
        public IConfiguration JsonConfig { get; set; }
        public IConfigurationRoot ConfigurationRoot { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddFacturaElectronica(Configuration);
            services.AddServicesDependencies(Configuration);
            services.AddSingleton<IAuthentication, JWTAuthentication>();
            services.AddSingleton<ResolucionNumber>();
            
            // configure strongly typed settings objects
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);

            // configure jwt authentication
            var appSettings = appSettingsSection.Get<AppSettings>();
            if (string.IsNullOrWhiteSpace(appSettings?.Secret))
            {
                throw new InvalidOperationException(
                    "Falta AppSettings:Secret (secreto JWT). Configúrelo en appsettings.Local.json o con la variable AppSettings__Secret.");
            }
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            services.AddCors();
            services.AddHealthChecks().AddCheck<BaseDatosHealthCheck>("basedatos");

            // Detrás de un proxy inverso (Docker + Caddy) se confía en X-Forwarded-* para conocer el esquema
            // real (https) y la IP del cliente. Solo se habilita por configuración: el API no debe ser accesible
            // directamente cuando esto está activo.
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            services.AddMvc(opt => 
            {
                opt.Filters.Add(new ApiExceptionFilter());
                // Restringe a los usuarios cliente a sus paginas permitidas
                opt.Filters.Add(new ClienteAccesoFilter());
            });

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // El token expira exactamente a la hora (sin los 5 minutos de tolerancia por defecto)
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Sin token no se puede llamar al API: todo endpoint exige un usuario autenticado
            // salvo los marcados con [AllowAnonymous] (login).
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Swagger Estaciones de servicios",
                    Description = "Estaciones de servicios"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { In = ParameterLocation.Header, Description = "JWT con Bearer", Name = "Autorization", Type = SecuritySchemeType.ApiKey });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement{ 
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference{
                                Id = "Bearer", //The name of the previously defined security scheme.
                                Type = ReferenceType.SecurityScheme
                            }
                        },new List<string>()
                    }});

                c.CustomSchemaIds(x => x.FullName);

            });
            services.AddControllers();
        }

        private string[] ObtenerOrigenesCors()
        {
            var origenes = Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? Array.Empty<string>();
            var adicionales = (Configuration["Cors:OrigenesAdicionales"] ?? string.Empty)
                .Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(origen => origen.Contains("://") ? origen : "https://" + origen);

            return origenes
                .Concat(adicionales)
                .Where(origen => !string.IsNullOrWhiteSpace(origen))
                .Select(origen => origen.Trim().TrimEnd('/'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (Configuration.GetValue<bool>("ProxyInverso:Habilitado"))
            {
                app.UseForwardedHeaders();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // HSTS: tells browsers to only use HTTPS for 1 year, including subdomains.
                app.UseHsts();
            }

            // A01 fix: redirect all HTTP traffic to HTTPS before any auth or routing logic.
            // /health queda fuera para que el healthcheck interno de Docker pueda llamarlo por HTTP.
            app.UseWhen(context => !context.Request.Path.StartsWithSegments("/health"),
                branch => branch.UseHttpsRedirection());

            // Cors:Origenes restringe los orígenes permitidos (ej. https://cliente.sigesmas.com).
            // Cors:OrigenesAdicionales acepta una lista separada por espacios o comas (dominios extra del cliente).
            // Si no se configura ninguno se mantiene el comportamiento anterior (cualquier origen).
            var origenesPermitidos = ObtenerOrigenesCors();
            app.UseCors(p =>
            {
                p.AllowAnyHeader().AllowAnyMethod();
                if (origenesPermitidos.Length > 0)
                {
                    p.WithOrigins(origenesPermitidos);
                }
                else
                {
                    p.AllowAnyOrigin();
                }
            });

            // A02 fix: add security headers on every response.
            app.Use(async (context, next) =>
            {
                var headers = context.Response.Headers;

                // Prevent MIME-type sniffing
                headers["X-Content-Type-Options"] = "nosniff";

                // Prevent clickjacking by disallowing iframes
                headers["X-Frame-Options"] = "DENY";

                // Legacy XSS filter for older browsers
                headers["X-XSS-Protection"] = "1; mode=block";

                // Control referrer information sent in requests
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

                // Content Security Policy: restricts resource origins
                headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline'; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "img-src 'self' data:; " +
                    "font-src 'self'; " +
                    "connect-src 'self'; " +
                    "frame-ancestors 'none';";

                // Remove Server header to avoid technology version disclosure
                headers.Remove("Server");

                await next();
            });
            // Swagger va antes del ruteo para no quedar bajo la politica de autenticacion global
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint(JsonConfig["swaggerUrl"], "API V1");
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health").AllowAnonymous();
            });

        }
    }
}
