using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad
{
	public class RequestContabilidad
	{
		public DatosTercero datosTercero { get; set; }
		public DatosFactura datosFactura { get; set; }
		public RequestContabilidad(FacturaSilog factura, string usuario, Alegra alegra, string estacion)
		{
			datosTercero = new DatosTercero(factura);
			datosFactura = new DatosFactura(factura, usuario, alegra, estacion);
		}

    }
}
