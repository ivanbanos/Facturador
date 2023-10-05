using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class FidelizarVentaCommand : IRequest
    {
        public FidelizarVentaCommand(string identificacion, int idVenta)
        {
            Identificacion = identificacion;
            IdVenta = idVenta;
        }

        public string Identificacion { get; }
        public int IdVenta { get; }
    }
}
