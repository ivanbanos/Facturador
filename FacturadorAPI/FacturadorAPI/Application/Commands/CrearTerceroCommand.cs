using FacturadorAPI.Models;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class CrearTerceroCommand : IRequest<Tercero>
    {
        public CrearTerceroCommand(Tercero tercero)
        {
            Tercero = tercero;
        }

        public Tercero Tercero { get; }
    }
}
