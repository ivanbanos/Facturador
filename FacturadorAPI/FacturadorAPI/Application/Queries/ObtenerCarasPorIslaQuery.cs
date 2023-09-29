using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerCarasPorIslaQuery : IRequest<IEnumerable<CaraSiges>>
    {
        public ObtenerCarasPorIslaQuery(int idIsla)
        {
            IdIsla = idIsla;
        }

        public int IdIsla { get; }
    }
}