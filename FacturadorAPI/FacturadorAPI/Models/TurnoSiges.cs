using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models
{
    public class TurnoSiges
    {
        public int impresa { get; set; } = 0;

        public int Id { get; set; }
        public string Empleado { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int IdEstado { get; set; }
        public string Isla { get; set; }

        public List<TurnoSurtidor> turnoSurtidores {get;set;}
    }
}
