using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class MandarImprimirConsecutivoCommand : IRequest
    {
        public MandarImprimirConsecutivoCommand(string consecutivo)
        {
            Consecutivo = consecutivo;
        }

        public string Consecutivo { get; }
    }
}
