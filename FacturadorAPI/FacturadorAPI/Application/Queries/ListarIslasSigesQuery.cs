using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ListarIslasSigesQuery : IRequest<IEnumerable<IslaSiges>>
    {
    }
}
