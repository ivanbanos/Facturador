using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class ProcesarYObtenerCanastillasCommand : IRequest<IEnumerable<Canastilla>>
    {
    }
}
