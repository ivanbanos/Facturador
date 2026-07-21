using MediatR;
using System;

namespace FacturadorAPI.Application.Commands
{
    public class CrearAnticipoCommand : IRequest
    {
        public int IdIsla { get; }
        public int NumTurno { get; }
        public DateTime FechaTurno { get; }
        public string Nombre { get; }
        public string Placa { get; }
        public decimal Monto { get; }
        public string TurnoGuid { get; }

        public CrearAnticipoCommand(int idIsla, int numTurno, DateTime fechaTurno, string nombre, string placa, decimal monto, string turnoGuid)
        {
            IdIsla = idIsla;
            NumTurno = numTurno;
            FechaTurno = fechaTurno;
            Nombre = nombre;
            Placa = placa;
            Monto = monto;
            TurnoGuid = turnoGuid ?? string.Empty;
        }
    }
}
