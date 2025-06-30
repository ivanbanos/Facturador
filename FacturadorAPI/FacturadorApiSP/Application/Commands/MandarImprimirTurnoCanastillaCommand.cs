using MediatR;

namespace FacturadorApiSP.Application.Commands
{
    public class MandarImprimirTurnoCanastillaCommand : IRequest<string>
    {
        public MandarImprimirTurnoCanastillaCommand(int isla)
        {
            Isla = isla;
        }

        public int Isla { get; }
    }
}