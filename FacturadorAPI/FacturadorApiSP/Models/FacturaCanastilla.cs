
namespace FacturadorAPI.Models
{
    public class FacturaCanastilla
    {
        public int terceroId { get; set; }
        public int codigoFormaPago { get; set; }
        public float descuento { get; set; }
        public List<CanastillaFactura> canastillas { get; set; }

    }
}
