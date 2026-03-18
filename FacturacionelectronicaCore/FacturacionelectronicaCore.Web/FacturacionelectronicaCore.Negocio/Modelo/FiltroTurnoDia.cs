using System;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class FiltroTurnoDia
    {
        public DateTime Fecha { get; set; }
        public Guid Estacion { get; set; }
        public string Empleado { get; set; }
        public string Isla { get; set; }
        public int? NumeroTurno { get; set; }
        public string Surtidor { get; set; }
        public string Manguera { get; set; }
        public string Combustible { get; set; }
    }
}
