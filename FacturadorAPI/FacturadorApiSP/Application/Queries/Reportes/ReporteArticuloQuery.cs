using FacturadorAPI.Application.Queries.Reportes.Objetos;
using MediatR;

namespace FacturadorAPI.Application.Queries.Reportes
{
    public class ReporteArticuloQuery : IRequest<ReporteArticuloResponse>
    {
        public ReporteArticuloQuery(ReporteGeneralRequest request)
        {
            Request = request;
        }

        public ReporteGeneralRequest Request { get; }
    }
}
