using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ListarFormasPagoSigesQuery : IRequest<IEnumerable<FormaPagoSiges>>
    {
    }
}
