using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models
{
    public class FacturaSiges
    {
        public DateTime? fechaUltimaActualizacion;

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
        public Tercero Tercero { get; set; }
        public MangueraSiges Manguera { get; set; }
        public string Estado { get; set; }
        public bool habilitada { get; internal set; }
        public int codigoFormaPago { get; set; }

        public string CodigoInterno { get; set; }
        public string Empleado { get; set; }
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Mangueras { get; set; }
        public string Combustible { get; set; }

        public double Cantidad { get; set; }
        public double Precio { get; set; }
        public double Iva { get; set; }
        public double Subtotal { get; set; }
        public double Total { get; set; }
        public double Descuento { get; set; }
        public int IdCara { get; set; }
        public string IButton { get; set; }
        public DateTime? fechaProximoMantenimiento { get; set; }
    }
}
