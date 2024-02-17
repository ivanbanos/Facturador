using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ListarTiposIdentificacionQuery : IRequest<IEnumerable<TipoIdentificacion>>
    {
        public ListarTiposIdentificacionQuery()
        {
        }

    }
}
