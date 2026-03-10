using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnviadorInformacionService.Models
{
    public class Bolsa
    {
        public DateTime Fecha { get; set; }
        public string Isla { get; set; }
        public int NumeroTurno { get; set; }
        public string Empleado { get; set; }
        public int Consecutivo { get; set; }
        public double Moneda { get; set; }
        public double Billete { get; set; }
    }
}
