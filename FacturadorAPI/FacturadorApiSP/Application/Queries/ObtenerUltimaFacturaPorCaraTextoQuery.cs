using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerUltimaFacturaPorCaraTextoQuery : IRequest<string>
    {
        public ObtenerUltimaFacturaPorCaraTextoQuery(int idCara)
        {
            IdCara = idCara;
        }

        public int IdCara { get; }
    }
}
