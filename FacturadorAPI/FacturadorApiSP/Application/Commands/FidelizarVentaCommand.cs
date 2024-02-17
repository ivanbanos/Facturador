using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class FidelizarVentaCommand : IRequest
    {
        public FidelizarVentaCommand(string identificacion, int idCara)
        {
            Identificacion = identificacion;
            IdCara = idCara;
        }

        public string Identificacion { get; }
        public int IdCara { get; }
    }
}
