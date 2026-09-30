using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class Turno
    {
        public string Id { get; set; }
        public string Empleado { get; set; }
        public DateTime FechaApertura { get; set; }
        public DateTime? FechaCierre { get; set; }
        public int IdEstado { get; set; }
        public string Isla { get; set; }
        public int Numero { get; set; }

        public string EstacionGuid { get; set; }
        public List<TurnoSurtidor> turnoSurtidores { get; set; }
    }

    public class TurnoSurtidor
    {
        public string Manguera { get; set; }
        public string Surtidor { get; set; }
        public double Apertura { get; set; }
        public double? Cierre { get; set; }
        public string Combustible { get; set; }
        public float precioCombustible { get; set; }
    }
}
