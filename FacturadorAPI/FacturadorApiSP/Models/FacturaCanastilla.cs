
using FactoradorEstacionesModelo.Objetos;

namespace FacturadorAPI.Models
{
    public class FacturaCanastilla
    {
        public int FacturasCanastillaId { get; set; }
        public DateTime fecha { get; set; }
        public Resolucion resolucion { get; set; }
        public int consecutivo { get; set; }
        public string estado { get; set; }
        public int terceroId { get; set; }
        public Tercero Tercero { get; set; }
        public int impresa { get; set; }
        public bool enviada { get; set; }
        public int codigoFormaPago { get; set; }
        public string numeroTransaccion { get; set; }
        public int? codigoFormaPago2 { get; set; }
        public float? total1 { get; set; }
        public float? total2 { get; set; }
        public FormasPagos Forma { get; set; }
        public IEnumerable<CanastillaFactura> canastillas { get; set; }
        public float subtotal { get; set; }
        public float descuento { get; set; }
        public float iva { get; set; }
        public float total { get; set; }
        public string placa { get; set; }
        public Guid Guid { get; set; }
        public Guid IdEstacion { get; set; }

    }
}
