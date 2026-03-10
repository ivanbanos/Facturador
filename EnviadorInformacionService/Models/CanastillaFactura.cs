namespace EnviadorInformacionService.Models
{
    public class CanastillaFactura
    {
        public Canastilla Canastilla { get; set; }
        public float cantidad { get; set; }
        public float precioBruto { get { return precio - iva; }  }
        public float precio { get; set; }
        public float subtotal { get; set; }
        public float iva { get; set; }
        public float total { get; set; }
    }
}