namespace FacturadorAPI.Models
{
    public class FacturaCanastillaRequest
    {
        public int terceroId { get; set; }
        public int codigoFormaPago { get; set; }
        public int isla { get; set; }
        public string empleado { get; set; }
        public int vendedor { get; set; }
        public IEnumerable<CanastillaFacturaRequest> canastillas { get; set; }
        public float descuento { get; set; }

    }

    public class CanastillaFacturaRequest
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
