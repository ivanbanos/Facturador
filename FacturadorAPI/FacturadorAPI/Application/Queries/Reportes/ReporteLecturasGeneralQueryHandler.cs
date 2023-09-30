using FacturadorAPI.Application.Queries.Reportes.Objetos;
using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;

namespace FacturadorAPI.Application.Queries.Reportes
{
    public class ReporteLecturasGeneralQueryHandler : IRequestHandler<ReporteLecturasGeneralQuery, ReporteLecturasGeneralResponse>
    {
        private readonly ILogger<ReporteLecturasGeneralQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly InfoEstacion _infoEstacion;

        public ReporteLecturasGeneralQueryHandler(ILogger<ReporteLecturasGeneralQueryHandler> logger, IDataBaseHandler databaseHandler, IOptions<InfoEstacion> infoEstacion)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _infoEstacion = infoEstacion.Value;
        }

        public async Task<ReporteLecturasGeneralResponse> Handle(ReporteLecturasGeneralQuery request, CancellationToken cancellationToken)
        {
            var reporteLecturasGeneralResponse = new ReporteLecturasGeneralResponse(_infoEstacion.Nombre, _infoEstacion.NIT);
            var turnos = await _databaseHandler.GetTurnosByFechas(request.Request.FechaInicio, request.Request.FechaFin);
            foreach(var turno in turnos){
                var turnosSurtidores = await _databaseHandler.GetTurnoSurtidorInfo(turno.Id);
                foreach (var turnosurtidor in turnosSurtidores) {
                    reporteLecturasGeneralResponse.reporteTurnoItem.Add(
                        new ReporteTurnoItem()
                        {
                            Combustible = turnosurtidor.Combustible.Descripcion,
                            Fecha = turno.FechaApertura,
                            IdTurno = turno.Id,
                            Isla = turno.Isla,
                            LecturaInicial = turnosurtidor.Apertura.ToString(),
                            LecturaFinal = turnosurtidor.Cierre.HasValue?turnosurtidor.Cierre.Value.ToString():"",
                            Manguera = turnosurtidor.Manguera.Descripcion
                        }); ;
                }
            }
            return reporteLecturasGeneralResponse;
        }
    }
}
