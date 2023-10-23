using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class ReimprimirTurnoCommand : IRequest
    {
        public ReimprimirTurnoCommand(DateTime fecha, int idIsla, int posicion)
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
