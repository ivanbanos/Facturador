using FacturadorAPI.Application.Queries.Reportes.Objetos;
using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Queries.Reportes
{
    public class ReporteLecturasGeneralQuery : IRequest<ReporteLecturasGeneralResponse>
    {
        public ReporteLecturasGeneralQuery(ReporteGeneralRequest request)
        {
            Request = request;
        }

        public ReporteGeneralRequest Request { get; }
    }
}
