
namespace FacturadorAPI.Models
{
    public class FacturaCanastilla
    {
        public int FacturasCanastillaId { get; set; }
        public DateTime fecha { get; set; }
        public Resolucion resolucion { get; set; }
        public int consecutivo { get; set; }
        public string estado { get; set; }
        public Tercero terceroId { get; set; }
        public int impresa { get; set; }
        public int enviada { get; set; }
        public FormaPagoSiges codigoFormaPago { get; set; }
        public List<CanastillaFactura> canastillas { get; set; }
        public float subtotal { get; set; }
        public float descuento { get; set; }
        public float iva { get; set; }
        public float total { get; set; }
    }
}
