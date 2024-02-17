using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class ConvertirAFacturaCommand : IRequest
    {
        public ConvertirAFacturaCommand(int idFactura)
        {
            IdFactura = idFactura;
        }

        public int IdFactura { get; }
    }
}
