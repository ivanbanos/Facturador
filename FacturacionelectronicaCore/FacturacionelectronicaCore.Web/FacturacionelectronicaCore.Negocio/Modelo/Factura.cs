using System;
using System.Collections.Generic;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class Factura
    {
        public Factura() {
            Guid = "";
        }
        public string Guid { get; set; }

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
        public string AutorizacionResolucion { get; set; }
        public IEnumerable<OrdenDeDespacho> Ordenes { get; set; }
        public string idFacturaElectronica { get; set; }
        public string IdEstacion { get; set; }
        public DateTime FechaReporte { get; set; }
    }
}
