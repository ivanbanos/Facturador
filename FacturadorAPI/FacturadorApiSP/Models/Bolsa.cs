namespace FacturadorApiSP.Models
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
