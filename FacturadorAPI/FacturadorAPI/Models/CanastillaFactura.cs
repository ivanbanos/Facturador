namespace FacturadorAPI.Models
{
    public class CanastillaFactura
    {
        public int canastillaId { get; set; }
        public Guid canastillaGuid { get; set; }
        public string descripcion { get; set; }
        public string unidad { get; set; }
        public float precio { get; set; }
        public string deleted { get; set; }
        public float cantidad { get; set; }
        public float iva { get; set; }
    }
}