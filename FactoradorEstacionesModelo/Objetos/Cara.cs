using System;
using System.Collections.Generic;
using System.Text;

namespace FactoradorEstacionesModelo.Objetos
{
    public class Cara
    {
        public short COD_CAR { get; internal set; }
        public byte POS { get; internal set; }
        public string DESCRIPCION { get; internal set; }
        public short? NUM_POS { get; internal set; }
        public short COD_SUR { get; internal set; }
    }
}
