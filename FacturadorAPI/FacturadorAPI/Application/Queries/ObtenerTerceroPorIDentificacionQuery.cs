using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerTerceroPorIDentificacionQuery : IRequest<IEnumerable<Tercero>>
    {

        public ObtenerTerceroPorIDentificacionQuery(string identificacion)
        {
            Identificacion = identificacion;
        }

        public string Identificacion { get; }
    }
}
