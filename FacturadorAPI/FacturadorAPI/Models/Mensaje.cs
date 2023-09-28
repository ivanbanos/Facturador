using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models
{
    public class Mensaje
    {
        public int SurtidorId { get; set; }
        public string Estado { get; set; }
        public string Ubicacion { get; set; }
        public string Turno { get; set; }
        public string Empleado { get; set; }
    }
}
