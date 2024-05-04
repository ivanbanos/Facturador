using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class AgregarFacturaCanastillaCommand : IRequest<string>
    {
        public AgregarFacturaCanastillaCommand(FacturaCanastillaRequest facturaCanastilla)
        {
            FacturaCanastilla = facturaCanastilla;
        }

        public FacturaCanastillaRequest FacturaCanastilla { get; }
    }
}
