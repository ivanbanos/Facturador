using MediatR;

namespace FacturadorApiSP.Application.Commands
{
    public class ReimprimirTurnoCanastillaCommand : IRequest<string>
    {
        public ReimprimirTurnoCanastillaCommand(DateTime fecha, int idIsla, int posicion)
        {
            Fecha = fecha;
            IdIsla = idIsla;
            Posicion = posicion;
        }

        public DateTime Fecha { get; }
        public int IdIsla { get; }
        public int Posicion { get; }
    }
}
