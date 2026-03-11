using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class TurnoReporte
    {
        public string turno { get; set; }
        public string Manguera { get; set; }
        public string Surtidor { get; set; }
        public string Combustible { get; set; }
        public double Apertura { get; set; }
        public double Cierre { get; set; }
        public double Diferencia { get; set; }
        public double Precio { get; set; }
        public double Total { get; set; }
    }
}
