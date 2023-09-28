using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models.Externos
{
    public class RequestEnvioResolucion
    {
        public IEnumerable<string> guidsFacturasPendientes;

        public ResolucionExterno Resolucion { get; internal set; }

        public Guid Estacion { get; set; }
    }
}
