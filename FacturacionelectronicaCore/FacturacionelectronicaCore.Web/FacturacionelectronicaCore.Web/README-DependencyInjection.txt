// Agregar estas líneas en el archivo Startup.cs o Program.cs para configurar la inyección de dependencias

// En el método ConfigureServices o donde se configuran las dependencias:

// Registrar el repositorio de facturas consolidadas
services.AddScoped<IFacturaConsolidadaRepository, FacturaConsolidadaRepository>();

// Registrar la lógica de negocio de facturas consolidadas
services.AddScoped<IFacturaConsolidadaNegocio, FacturaConsolidadaNegocio>();

// Agregar el perfil de AutoMapper para facturas consolidadas
services.AddAutoMapper(typeof(FacturaConsolidadaProfile));

// O si ya tienes configurado AutoMapper, agregar el perfil:
// services.AddAutoMapper(typeof(FacturaConsolidadaProfile), typeof(OtroProfile), ...);

// Las dependencias existentes como IMongoHelper, IOrdenDeDespachoRepository, 
// ITerceroRepositorio, IFacturacionElectronicaFacade ya deben estar registradas