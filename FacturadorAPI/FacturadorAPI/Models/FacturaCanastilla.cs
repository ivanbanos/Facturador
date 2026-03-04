
namespace FacturadorAPI.Models
{
    public class FacturaCanastilla
    {
        public int terceroId { get; set; }
        public int codigoFormaPago { get; set; }
        public int? codigoFormaPago2 { get; set; }
        public float? total1 { get; set; }
        public float? total2 { get; set; }
        public float descuento { get; set; }
        public string placa { get; set; }
        public List<CanastillaFactura> canastillas { get; set; }

    }
}
