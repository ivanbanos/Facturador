using System;
using System.Collections.Generic;
using System.Text;

namespace FactoradorEstacionesModelo.Siges
{
    public class TurnoSurtidor
    {
        public MangueraSiges Manguera { get; set; }
        public double Apertura { get; set; }
        public double? Cierre { get; set; }
        public Combustible Combustible { get; set; }
    }
}
