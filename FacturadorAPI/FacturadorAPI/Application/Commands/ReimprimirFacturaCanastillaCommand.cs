using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class ReimprimirFacturaCanastillaCommand : IRequest
    {
        public ReimprimirFacturaCanastillaCommand(int consecutivo)
        {
            Consecutivo = consecutivo;
        }

        public int Consecutivo { get; }
    }
}
