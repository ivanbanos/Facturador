using System.Collections.Generic;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class RespuestaCelesteToken
    {
        public bool success { get; set; }
        public int code { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public RespuestaCelesteTokenData data { get; set; }
    }

    public class RespuestaCelesteTokenData
    {
        public string token { get; set; }
    }

    public class RespuestaCelesteFactura
    {
        public bool success { get; set; }
        public int status { get; set; }
        public string message { get; set; }
        public RespuestaCelesteFacturaData data { get; set; }
        // Error fields
        public string error { get; set; }
        public List<RespuestaCelesteDetalle> details { get; set; }
    }

    public class RespuestaCelesteFacturaData
    {
        public string gen_uuid { get; set; }
        public string fecha_factura { get; set; }
        public string numero_factura { get; set; }
    }

    public class RespuestaCelesteDetalle
    {
        public string field { get; set; }
        public string message { get; set; }
    }

    public class RespuestaCelesteConsulta
    {
        public bool success { get; set; }
        public int status { get; set; }
        public string message { get; set; }
        public RespuestaCelesteConsultaData data { get; set; }
    }

    public class RespuestaCelesteConsultaData
    {
        /// <summary>Numero de factura (consecutivo DIAN).</summary>
        public string numero_factura { get; set; }
        public string fecha_factura { get; set; }
        /// <summary>0 = aprobada, 1 = pendiente, 2 = rechazada. Also returned as string "aprobada" in older docs.</summary>
        public string estado { get; set; }
        /// <summary>CUFE del documento electrónico (si Celeste lo devuelve en este campo).</summary>
        public string cufe { get; set; }
        /// <summary>PDF en base64 (attached_document).</summary>
        public string attached_document { get; set; }
    }
}
