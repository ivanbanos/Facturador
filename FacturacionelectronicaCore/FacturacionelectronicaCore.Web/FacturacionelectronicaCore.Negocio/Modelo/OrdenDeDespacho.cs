using System;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class OrdenDeDespacho
    {
        public  string numeroTransaccion { get; set; }

        public OrdenDeDespacho()
        {
        }
        public string guid { get; set; }
        public int IdFactura { get; set; }
        public string Identificacion { get; set; }
        public string NombreTercero { get; set; }
        public string Combustible { get; set; }
        public double Cantidad { get; set; }
        public double Precio { get; set; }
        public double Total { get; set; }
        public decimal Descuento { get; set; }
        public string IdInterno { get; set; }
        public string Placa { get; set; }
        public string Kilometraje { get; set; }
        public int IdEstadoActual { get; set; }
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Manguera { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public string IdentificacionTercero { get; set; }
        public string FormaDePago { get; set; }
        public string FormaDePago2 { get; set; }
        public decimal? Total1 { get; set; }
        public decimal? Total2 { get; set; }
        public int IdLocal { get; set; }
        public int IdVentaLocal { get; set; }
        public int IdTerceroLocal { get; set; }
        public int IdEstacion { get; set; }
        public decimal SubTotal { get; set; }
        public DateTime FechaProximoMantenimiento { get; set; }
        public string Vendedor { get; set; }

        public Tercero Tercero { get; set; }
        public string idFacturaElectronica { get; set; }
        public DateTime FechaReporte { get; set; }
        public decimal TotalPrice { get { return ((decimal)Cantidad * (decimal)Precio) - Descuento; } }

        public OrdenDeDespacho(string _guid, string identificacion, string nombreTercero, string combustible,
                                    double cantidad, double precio, double total, string idInterno, string placa, string kilometraje,
                                    string surtidor, string cara, string manguera, DateTime fecha, string estado, int idLocal, int idVentaLocal, DateTime fechaReporte)
        {
            guid = _guid;
            Identificacion = identificacion;
            NombreTercero = nombreTercero;
            Combustible = combustible;
            Cantidad = cantidad;
            Precio = precio;
            Total = total;
            IdInterno = idInterno;
            Placa = placa;
            Kilometraje = kilometraje;
            Surtidor = surtidor;
            Cara = cara;
            Manguera = manguera;
            Fecha = fecha;
            Estado = estado;
            IdLocal = idLocal;
            IdVentaLocal = idVentaLocal;
            FechaReporte = fechaReporte;
        }

    }
}
