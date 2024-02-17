using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerTurnoPorIslaQuery : IRequest<TurnoSiges>
    {
        public ObtenerTurnoPorIslaQuery(int idIsla)
        {
            IdIsla = idIsla;
        }

        public int IdIsla { get; }
    }
}
