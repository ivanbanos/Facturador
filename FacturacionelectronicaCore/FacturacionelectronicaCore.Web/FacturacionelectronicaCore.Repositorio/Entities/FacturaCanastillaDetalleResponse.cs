using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
   public  class FacturaCanastillaDetalleResponse
    {
        public double cantidad { get; set; }
        public double precio { get; set; }
        public double subtotal { get; set; }
        public double iva { get; set; }
        public double total { get; set; }
        public string descripcion { get; set; }
    }
}
