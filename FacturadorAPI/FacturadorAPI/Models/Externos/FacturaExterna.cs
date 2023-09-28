using FacturadorAPI.Models;

namespace FacturadorAPI.Models.Externos
{
    public class FacturaExterna
    {
        public FacturaExterna() { }
        public FacturaExterna(FacturaSiges x, string forma)
        {

            Guid = Guid.NewGuid();
            Consecutivo = x.Consecutivo == 0 ? x.ventaId : x.Consecutivo;
            Combustible = x.Combustible;
            Cantidad = x.Cantidad;
            Precio = x.Precio;
            Total = x.Total;
            IdInterno = "";
            Placa = x.Placa;
            Kilometraje = x.Kilometraje;
            Surtidor = x.Surtidor + "";
            Cara = x.Cara + "";
            Manguera = x.Manguera + "";
            FormaDePago = forma;
            Fecha = x.fecha;
            Tercero = new TerceroExterno(x.Tercero);
            Descuento = x.Descuento;
            IdLocal = x.facturaPOSId;
            IdVentaLocal = x.ventaId;
            IdTerceroLocal = x.Tercero.terceroId;
            FechaProximoMantenimiento = DateTime.Now;
            SubTotal = x.Subtotal;
            Vendedor = x.Empleado;
            Identificacion = x.Tercero.identificacion;
            Prefijo = x.DescripcionResolucion;
            Cedula = "";

        }
        public Guid Guid { get; set; }

        public int Consecutivo { get; set; }

        public Guid IdTercero { get; set; }
        public string Identificacion { get; set; }
        public string NombreTercero { get; set; }
        public string Combustible { get; set; }
        public double Cantidad { get; set; }
        public double Precio { get; set; }
        public double Total { get; set; }
        public double Descuento { get; set; }
        public string IdInterno { get; set; }
        public string Placa { get; set; }
        public string Kilometraje { get; set; }
        public Guid IdResolucion { get; set; }
        public int IdEstadoActual { get; set; }
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Manguera { get; set; }
        public string FormaDePago { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public TerceroExterno Tercero { get; set; }
        public int IdLocal { get; set; }
        public int IdVentaLocal { get; set; }
        public int IdTerceroLocal { get; set; }
        public DateTime FechaProximoMantenimiento { get; set; }
        public double SubTotal { get; set; }
        public string Vendedor { get; set; }
        public string DescripcionResolucion { get; set; }
        public IEnumerable<OrdenDeDespacho> Ordenes { get; set; }
        public string Prefijo { get; set; }
        public string Cedula { get; set; }
    }
}
