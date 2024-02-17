using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class AgregarFacturaCanastillaCommand : IRequest
    {
        public AgregarFacturaCanastillaCommand(FacturaCanastilla facturaCanastilla)
        {
            FacturaCanastilla = facturaCanastilla;
        }

        public FacturaCanastilla FacturaCanastilla { get; }
    }
}
