using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
	public class RequestContabilidad
	{
		public DatosTercero datosTercero { get; set; }
		public DatosFactura datosFactura { get; set; }
		public RequestContabilidad(Factura factura)
		{
			datosTercero = new DatosTercero(factura);
			datosFactura = new DatosFactura(factura, new Guid(ConfigurationManager.AppSettings["estacionFuente"]));
		}

    }
}
