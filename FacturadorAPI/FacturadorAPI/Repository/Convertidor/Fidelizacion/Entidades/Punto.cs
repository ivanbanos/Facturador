namespace Dominio.Entidades
{
    public class Punto
    {
        public Punto() { }

        public Punto(float valorVenta, string factura, int fidelizadoId, int centroVentaId)
        {
            ValorVenta = valorVenta;
            Factura = factura;
            FidelizadoId = fidelizadoId;
            CentroVentaId = centroVentaId;
        }
                
        public int Id { get; set; }
        public float ValorVenta { get; set; }
        public string Factura { get; set; }
        public int FidelizadoId { get; set; }
        public virtual Fidelizado Fidelizado { get; set; }
        public int CentroVentaId { get; set; }
        public virtual CentroVenta CentroVenta { get; set; }
    }
}
