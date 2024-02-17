
namespace FacturadorAPI.Models.Externos
{
    public class OrdenDeDespacho
    {

        public OrdenDeDespacho()
        {
        }
        public Guid Guid { get; set; }
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
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Manguera { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public TerceroExterno Tercero { get; set; }
        public string FormaDePago { get; set; }
        public int IdLocal { get; set; }
        public int IdVentaLocal { get; set; }
        public int IdTerceroLocal { get; set; }
        public DateTime FechaProximoMantenimiento { get; set; }
        public double SubTotal { get; set; }
        public string Vendedor { get; set; }
        public Guid estacion { get; set; }


        public OrdenDeDespacho(FacturaSiges x, string forma)
        {
            Guid = Guid.NewGuid();
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
        }
    }
}
