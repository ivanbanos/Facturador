using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerSurtidoresQuery : IRequest<IEnumerable<SurtidorSiges>>
    {
    }
}
