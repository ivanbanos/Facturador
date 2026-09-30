using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ComprobanteTitan
    {
        public string TipoComprobante { get; set; }
        public string Fecha { get; set; }
        public string Prefijo { get; set; }
        public string Numero { get; set; }
        public string Observaciones { get; set; }
        public List<MetodoPagoTitan> MetodoPago { get; set; }
    }

    public class DetalleTitan
    {
        public string Item { get; set; }
        public string Nrodoc { get; set; }
        public string codigo { get; set; }
        public string Nombre { get; set; }
        public string Cantidad { get; set; }
        public string ValorUnitario { get; set; }
        public string Descuento { get; set; }
        public string SubTotal { get; set; }
        public string Total { get; set; }
        public List<ImpuestoTitan> Impuestos { get; set; }
    }

    public class EmisorTitan
    {
        public string Identificacion { get; set; }
    }

    public class ImpuestoTitan
    {
        public string Base { get; set; }
        public string Nombre { get; set; }
        public string Porcentaje { get; set; }
        public string Impuesto { get; set; }
    }

    public class MetodoPagoTitan
    {
        public string FormaPago { get; set; }
        public string MedioPago { get; set; }
        public string Fechavence { get; set; }
        public string Diasplazo { get; set; }
    }

    public class ReceptorTitan
    {
        public string Identificacion { get; set; }
        public string Dv { get; set; }
        public string Apellido1 { get; set; }
        public string Apellido2 { get; set; }
        public string Nombres { get; set; }
        public string Razonsocial { get; set; }
        public string Direccion { get; set; }
        public string CodCiudad { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
    }

    public class FacturaTitan
    {
        public ComprobanteTitan Comprobante { get; set; }
        public EmisorTitan Emisor { get; set; }
        public ReceptorTitan Receptor { get; set; }
        public List<DetalleTitan> Detalles { get; set; }
    }
}
