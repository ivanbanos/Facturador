using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class EnviarFacturaElectronicaCommand : IRequest
    {
        public EnviarFacturaElectronicaCommand(int idFactura)
        {
            IdFactura = idFactura;
        }

        public int IdFactura { get; }
    }
}
