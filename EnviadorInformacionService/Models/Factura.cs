using System;
using System.Collections.Generic;
using System.Text;

namespace FactoradorEstacionesModelo.Objetos
{
    public class Factura
    {
        public int ventaId { get; set; }

        public int impresa { get; set; }

        public string DescripcionResolucion { get; set; }
        public string Autorizacion { get; set; }
        public string Placa { get; set; }
        public string Kilometraje { get; set; }
        public DateTime FechaInicioResolucion { get; set; }
        public DateTime FechaFinalResolucion { get; set; }
        public int Inicio { get; set; }
        public int Final { get; set; }
        public DateTime fecha { get; set; }

        public int facturaPOSId { get; set; }
        public int Consecutivo { get; set; }
        public int vecesImpresa { get; set; }
        public Venta Venta { get; set; }
        public Tercero Tercero { get; set; }
        public Manguera Manguera { get; set; }
        public string Estado { get; set; }
        public IEnumerable<Venta> Ventas { get; set; }
        public bool habilitada { get; set; }
        public int codigoFormaPago { get; set; }
        public int? codigoFormaPago2 { get; set; }
        public decimal? total1 { get; set; }
        public decimal? total2 { get; set; }
        public string numeroTransaccion { get; set; }
        public decimal total { get; set; }
        public decimal subtotal { get; set; }
        public decimal descuento { get; set; }
    }
}
