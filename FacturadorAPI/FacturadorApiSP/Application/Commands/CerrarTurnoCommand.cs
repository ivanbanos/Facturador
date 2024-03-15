using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class CerrarTurnoCommand : IRequest<string>
    {
        public CerrarTurnoCommand(int isla, string codigo)
        {
            Isla = isla;
            Codigo = codigo;
        }

        public int Isla { get; }
        public string Codigo { get; }
    }
}
