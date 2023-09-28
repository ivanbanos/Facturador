using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerUltimaFacturaPorCaraQuery : IRequest<FacturaSiges>
    {
        public ObtenerUltimaFacturaPorCaraQuery(int idCara)
        {
            IdCara = idCara;
        }

        public int IdCara { get; }
    }
}
