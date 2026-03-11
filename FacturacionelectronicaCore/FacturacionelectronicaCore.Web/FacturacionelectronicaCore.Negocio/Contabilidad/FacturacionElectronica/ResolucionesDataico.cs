using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class DianResolution
    {
        public string code { get; set; }
        public string code_msg { get; set; }
        public string number { get; set; }
        public int start { get; set; }
        public int end { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
    }

    public class Numbering
    {
        public string prefix { get; set; }
        public string numbering_type { get; set; }
        public string subtype { get; set; }
        public List<DianResolution> dian_resolutions { get; set; }
    }

    public class ResolucionesDataico
    {
        public List<Numbering> numberings { get; set; }
    }


}
