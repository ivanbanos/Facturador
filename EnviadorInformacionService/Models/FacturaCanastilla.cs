using FactoradorEstacionesModelo.Objetos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnviadorInformacionService.Models
{
    public class FacturaCanastilla
    {
		public int FacturasCanastillaId { get; set; }
		public DateTime fecha { get; set; }
		public Resolucion resolucion { get; set; }
		public int consecutivo { get; set; }
		public string estado { get; set; }
		public Tercero terceroId { get; set; }
		public int impresa { get; set; }
		public bool enviada { get; set; }
		public FormasPagos codigoFormaPago { get; set; }
		public int? codigoFormaPago2 { get; set; }
		public decimal? total1 { get; set; }
		public decimal? total2 { get; set; }
		public List<CanastillaFactura> canastillas { get; set; }
		public float subtotal { get; set; }
		public float descuento { get; set; }
		public float iva { get; set; }
		public float total { get; set; }
		public Guid Guid { get; set; }
		public Guid IdEstacion { get; set; }
        public string Isla { get; set; }
        public string Empleado { get; set; }
		public string Placa { get; set; }
    }
}
