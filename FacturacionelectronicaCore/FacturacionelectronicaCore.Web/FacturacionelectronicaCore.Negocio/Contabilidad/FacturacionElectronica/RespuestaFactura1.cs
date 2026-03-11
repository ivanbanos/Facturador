using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class DIAN
    {
        public string Mensaje { get; set; }
        public string Xml { get; set; }
        public string XmlDocumentKey { get; set; }
        public string Valido { get; set; }
        public string Descripcion { get; set; }
        public string StatusCode { get; set; }
        public List<Respuestum> Respuesta { get; set; }
    }

    public class Respuestum
    {
        public string descripcion { get; set; }
    }

    public class RespuestaFactura1
    {
        public string qrdata { get; set; }
        public List<DIAN> DIAN { get; set; }
        public string xml { get; set; }
        public string clavtec { get; set; }
        public object id { get; set; }
        public string error { get; set; }
        public string cufe { get; set; }
    }


}
