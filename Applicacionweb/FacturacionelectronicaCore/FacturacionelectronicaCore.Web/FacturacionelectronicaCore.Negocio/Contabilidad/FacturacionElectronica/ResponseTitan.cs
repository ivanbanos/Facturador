using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class MensajeTitan
    {
        public string msg { get; set; }
    }
    public class ResponseTitan
    {
        public string Code { get; set; }
        public List<MensajeTitan> Mensaje { get; set; }
        public string Form { get; set; }
    }
}
