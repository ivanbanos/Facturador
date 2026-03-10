using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
    public class Factura
    {
        public string numeroTransaccion { get; set; }

        public Factura() { }
        public Factura(FactoradorEstacionesModelo.Objetos.Factura x, string forma)
        {
            Guid = Guid.Parse(ConfigurationManager.AppSettings["estacionFuente"]);
            Consecutivo = x.Consecutivo == 0 || x.Consecutivo == -1? x.ventaId : x.Consecutivo;
            Combustible = x.Manguera.DESCRIPCION;
            Cantidad = x.Venta.CANTIDAD;
            Precio = x.Venta.PRECIO_UNI;
            Total = x.Venta.TOTAL;
            IdInterno = x.Venta.COD_INT;
            Placa = x.Placa;
            Kilometraje = x.Kilometraje;
            Surtidor = x.Venta.COD_SUR + "";
            Cara = x.Venta.COD_CAR + "";
            Manguera = x.Manguera.COD_MAN + "";
            FormaDePago = forma;
            Fecha = x.fecha;
            Tercero = new Tercero(x.Tercero);
            Descuento = x.Venta.Descuento;
            IdLocal = x.facturaPOSId;
            IdVentaLocal = x.Venta.CONSECUTIVO;
            IdTerceroLocal = x.Tercero.terceroId;
            FechaProximoMantenimiento = x.Venta.FECH_PRMA.HasValue ? x.Venta.FECH_PRMA.Value : DateTime.Now;
            SubTotal = x.Venta.VALORNETO;
            Vendedor = x.Venta.EMPLEADO;
            Identificacion = x.Tercero.identificacion;
            Prefijo = x.DescripcionResolucion;
            Cedula = x.Venta.CEDULA;
            numeroTransaccion = x.numeroTransaccion;
        }
        public Guid Guid { get; set; }

        public int Consecutivo { get; set; }

        public Guid IdTercero { get; set; }
        public string Identificacion { get; set; }
        public string NombreTercero { get; set; }
        public string Combustible { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Total { get; set; }
        public decimal Descuento { get; set; }
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
        public Tercero Tercero { get; set; }
        public int IdLocal { get; set; }
        public int IdVentaLocal { get; set; }
        public int IdTerceroLocal { get; set; }
        public DateTime FechaProximoMantenimiento { get; set; }
        public decimal SubTotal { get; set; }
        public string Vendedor { get; set; }
        public string DescripcionResolucion { get; set; }
        public IEnumerable<OrdenDeDespacho> Ordenes { get; set; }
        public string Prefijo { get; set; }
        public string Cedula { get; set; }
    }
}
