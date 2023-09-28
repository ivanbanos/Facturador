using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class ConvertirAOrdenCommand : IRequest
    {
        public ConvertirAOrdenCommand(int idFactura)
        {
            IdFactura = idFactura;
        }

        public int IdFactura { get; }
    }
}
