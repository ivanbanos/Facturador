using FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class ResolucionElectronica
    {

        public ResolucionElectronica(Numbering numbering)
        {
            invoiceText = $"Resolución electronica {numbering.prefix} - {numbering.dian_resolutions.First().number} desde {numbering.dian_resolutions.First().start} hasta {numbering.dian_resolutions.First().end}. Valido hasta {numbering.dian_resolutions.First().end_date}";
        }

        public ResolucionElectronica() { }


        public string id { get; set; }
        public string name { get; set; }
        public string prefix { get; set; }
        public string invoiceText { get; set; }
        public bool isDefault { get; set; }
    }
}
