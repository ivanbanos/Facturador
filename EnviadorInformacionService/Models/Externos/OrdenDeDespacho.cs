using System;
using System.Configuration;
using System.Linq;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class OrdenDeDespacho
    {

        public OrdenDeDespacho()
        {
        }
        public Guid Guid { get; set; }
        public string Identificacion { get; set; }
        public DateTime FechaReporte { get; set; }
        public string NombreTercero { get; set; }
        public string Combustible { get; set; }
        public double Cantidad { get; set; }
        public double Precio { get; set; }
        public double Total { get; set; }
        public decimal Descuento { get; set; }
        public string IdInterno { get; set; }
        public string Placa { get; set; }
        public string Kilometraje { get; set; }
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Manguera { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public Tercero Tercero { get; set; }
        public string FormaDePago { get; set; }
        public string FormaDePago2 { get; set; }
        public decimal? Total1 { get; set; }
        public decimal? Total2 { get; set; }
        public int IdLocal { get; set; }
        public int IdVentaLocal { get; set; }
        public int IdTerceroLocal { get; set; }
        public DateTime FechaProximoMantenimiento { get; set; }
        public decimal SubTotal { get; set; }
        public string Vendedor { get; set; }
        public Guid estacion { get; set; }


        public OrdenDeDespacho(FactoradorEstacionesModelo.Objetos.Factura x, string forma, string forma2 = null)
        {
            var MultiplicarPor10 = bool.Parse(ConfigurationManager.AppSettings["MultiplicarPor10"]);
            if (MultiplicarPor10)
            {
                x.Venta.PRECIO_UNI = x.Venta.PRECIO_UNI * 10;
                x.Venta.VALORNETO = x.Venta.VALORNETO * 10;
                x.Venta.VALORNETO = x.Venta.TOTAL * 10;
                x.Venta.Descuento = x.Venta.Descuento * 10;
            }
            Guid = Guid.NewGuid();
            Combustible = x.Venta.Combustible;
            Cantidad = (double)x.Venta.CANTIDAD;
            Precio = (double)x.Venta.PRECIO_UNI;
            Total = (double)x.Venta.TOTAL;
            IdInterno = x.Venta.COD_INT;
            Placa = x.Placa;
            Kilometraje = x.Kilometraje;
            Surtidor = x.Venta.COD_SUR + "";
            Cara = x.Venta.COD_CAR + "";
            Manguera = x.Manguera.COD_MAN + "";
            FormaDePago = forma;
            FormaDePago2 = forma2;
            Total1 = x.total1;
            Total2 = x.total2;
            Fecha = x.fecha;
            Descuento  = x.Venta.Descuento;
            var nombres = x.Tercero?.Nombre?.Trim();
            var apellidos = x.Tercero?.Apellidos?.Trim();
            NombreTercero = string.Join(" ", new[] { nombres, apellidos }.Where(v => !string.IsNullOrWhiteSpace(v))).Trim();
            Tercero = new Tercero(x.Tercero);
            IdLocal = x.facturaPOSId;
            IdVentaLocal = x.Venta.CONSECUTIVO;
            IdTerceroLocal = x.Tercero.terceroId;
            FechaProximoMantenimiento = x.Venta.FECH_PRMA.HasValue ? x.Venta.FECH_PRMA.Value : DateTime.Now;
            SubTotal = x.Venta.VALORNETO;
            Vendedor = x.Venta.EMPLEADO;
            Identificacion = x.Tercero.identificacion;
            FechaReporte = x.Venta.FECHA_REAL.Value;
        }
    }
}
