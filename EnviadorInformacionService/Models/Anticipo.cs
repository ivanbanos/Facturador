using System;

namespace EnviadorInformacionService.Models
{
    public class Anticipo
    {
        public int AnticipoId { get; set; }
        public string TurnoGuid { get; set; }
        public int IdIsla { get; set; }
        public int NumTurno { get; set; }
        public DateTime FechaTurno { get; set; }
        public string Nombre { get; set; }
        public string Placa { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
