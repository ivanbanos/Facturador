using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class DetalleArticulo
    {
        public string Descripcion { get; set; }
        public int Cantidad { get; set; }
        public double Subtotal { get; set; }
        public double Iva { get; internal set; }
        public double Total { get; internal set; }
    }
}
