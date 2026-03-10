using System;
using System.Configuration;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
    public class DatosFactura
    {

        public DatosFactura(Factura factura, Guid estacion)
		{
			Consecutivo = factura.Consecutivo+"";
			Prefijo = factura.Prefijo;
            Detalles = $"Placa: {factura.Placa}, Kilometraje : {factura.Kilometraje}, Nro Transaccion : {factura.numeroTransaccion}";
            Placa = factura.Placa;
			Kilometraje = string.IsNullOrEmpty(factura.Kilometraje)?"0.0":factura.Kilometraje.Replace(",", ".");
			Surtidor = factura.Surtidor;
			Cara = factura.Cara;
			Manguera = factura.Manguera;
			FechaFacturacion = factura.Fecha.ToString("yyyy-MM-dd");
			Tercero = factura.Tercero.Identificacion;
			Cantidad = factura.Cantidad + "";
			Cantidad = Cantidad.Replace(",", ".");
			Precio = factura.Precio + "";
			Precio = Precio.Replace(",", ".");
			Descuento = "0";
			Iva = "0";
			Total = Decimal.ToInt32(Decimal.Round(factura.Total)).ToString();
			Subtotal = Decimal.ToInt32(Decimal.Round(factura.SubTotal)).ToString();
			fechaUltimoMantenimiento = factura.FechaProximoMantenimiento.ToString("yyyy-MM-dd");
			Usuario = factura.Cedula.Trim();
			Guid = estacion.ToString();
			if (factura.FormaDePago.ToLower().Contains("efec"))
			{
				FormaPago = ConfigurationManager.AppSettings["codigoefectivo"];
			}
			else if (factura.FormaDePago.ToLower().Contains("dito"))
            {
                FormaPago = ConfigurationManager.AppSettings["codigocredito"];
            }
            else if (factura.FormaDePago.ToLower().Contains("trans"))
            {
                FormaPago = ConfigurationManager.AppSettings["codigotransferencia"];
                NroTransaccion = factura.numeroTransaccion;
            }
            else
            {
                FormaPago = ConfigurationManager.AppSettings["codigovoucher"];
				TipoTarjeta = ConfigurationManager.AppSettings[factura.FormaDePago.Trim()];
				NroTransaccion = factura.numeroTransaccion ?? factura.Kilometraje ?? "NoEspecificado";
            }
			if (factura.Combustible.ToLower().Contains("corriente"))
			{
				Combustible = ConfigurationManager.AppSettings["codigocorriente"];
			}
			else if (factura.Combustible.ToLower().Contains("acpm") || factura.Combustible.ToLower().Contains("bio") || factura.Combustible.ToLower().Contains("diesel") || factura.Combustible.ToLower().Contains("a.c.p.m"))
			{
				Combustible = ConfigurationManager.AppSettings["codigoacpm"];
			}
			else if (factura.Combustible.ToLower().Contains("extra"))
			{
				Combustible = ConfigurationManager.AppSettings["codigoextra"];
			}
			else 
			{
				Combustible = ConfigurationManager.AppSettings["codigogas"];
			}
		}

		public string Consecutivo { get; set; }
        public string Prefijo { get; set; }
        public string Detalles { get; set; }
		public string Placa { get; set; }
		public string Kilometraje { get; set; }
		public string fechaUltimoMantenimiento { get; set; }
		public string Surtidor { get; set; }
		public string Cara { get; set; }
		public string Manguera { get; set; }
		public string FechaFacturacion { get; set; }
		public string Tercero { get; set; }
		public string FormaPago { get; set; }
        public string TipoTarjeta { get; set; }
        public string NroTransaccion { get; set; }
        public string Combustible { get; set; }
		public string Cantidad { get; set; }
		public string Precio { get; set; }
		public string Descuento { get; set; }
		public string Iva { get; set; }
		public string Total { get; set; }
		//public DateTime? FechaProximoMantenimiento { get; set; }
		public string Subtotal { get; set; }
		public string Guid { get; set; }
		public string Usuario { get; set; }

	}
}
