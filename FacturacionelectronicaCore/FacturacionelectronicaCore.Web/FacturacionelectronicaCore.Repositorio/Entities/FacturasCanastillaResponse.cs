using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
   public class FacturasCanastilla
    {
        public int FacturasCanastillaId { get; set; }
        public Guid Guid { get; set; }
        public DateTime fecha { get; set; }
        public int consecutivo { get; set; }
        public double subtotal { get; set; }
        public double descuento { get; set; }
        public double iva { get; set; }
        public double total { get; set; }
        public string Nombre { get; set; }
        public string Segundo{ get; set; }
        public string Apellidos { get; set; }
        public string identificacion { get; set; }
        public string TipoIdentificacion { get; set; }
        public string FormaDePago { get; set; }
        public IEnumerable<FacturaCanastillaDetalleResponse> facturaCanastillaDetalles { get; set; }
    }
}
