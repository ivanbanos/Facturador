using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class SpecificError
    {
        public List<string> path { get; set; }
        public string error { get; set; }
    }

    public class ErrorDataico
    {
        public List<SpecificError> errors { get; set; }
    }
}
