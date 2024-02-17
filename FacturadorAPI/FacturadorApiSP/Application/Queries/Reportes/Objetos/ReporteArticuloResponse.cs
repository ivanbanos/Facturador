namespace FacturadorAPI.Application.Queries.Reportes.Objetos
{
    public class ReporteArticuloResponse
    {
        public ReporteArticuloResponse(string nombre, string nIT)
        {
            Nombre = nombre;
            NIT = nIT;
        }

        public string Nombre { get; }
        public string NIT { get; }
        public List<PorFormas> PorFormas { get; set; } = new List<PorFormas>();
        public List<PorArticulo> PorArticulo { get; set; } = new List<PorArticulo>();
    }

    public class PorFormas
    {
        public string Descripcion { get; set; }
        public int Facturas { get; set; }
        public double Cantidad { get; set; }
        public double Total { get; set; }
    }


    public class PorArticulo
    {
        public string Descripcion { get; set; }
        public int Facturas { get; set; }
        public double Cantidad { get; set; }
        public double Neto { get; set; }
        public double Subtotal { get; set; }
        public double Recaudo { get; set; }
        public double Total { get; set; }
        public double Descuento { get; internal set; }
    }
}
