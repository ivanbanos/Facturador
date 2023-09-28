using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class MandarImprimirCommand : IRequest
    {
        public MandarImprimirCommand(int idVenta)
        {
            IdVenta = idVenta;
        }

        public int IdVenta { get; }
    }
}
