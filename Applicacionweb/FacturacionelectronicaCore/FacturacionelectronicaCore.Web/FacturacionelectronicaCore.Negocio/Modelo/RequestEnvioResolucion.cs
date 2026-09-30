using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class RequestEnvioResolucion
    {
        public List<string> guidsFacturasPendientes { get; set; }

        public Resolucion Resolucion { get;  set; }

        public Guid Estacion { get; set; }
    }
}
