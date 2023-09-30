using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class MandarImprimirCommand : IRequest
    {
        public MandarImprimirCommand(FacturaSiges factura)
        {
            Factura = factura;
        }

        public FacturaSiges Factura { get; }
    }
}
