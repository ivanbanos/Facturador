using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class MandarImprimirConsecutivoCommand : IRequest<string>
    {
        public MandarImprimirConsecutivoCommand(string consecutivo)
        {
            Consecutivo = consecutivo;
        }

        public string Consecutivo { get; }
    }
}
