using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class AgregarFacturaCanastillaCommand : IRequest<string>
    {
        public AgregarFacturaCanastillaCommand(FacturaCanastilla facturaCanastilla)
        {
            FacturaCanastilla = facturaCanastilla;
        }

        public FacturaCanastilla FacturaCanastilla { get; }
    }
}
