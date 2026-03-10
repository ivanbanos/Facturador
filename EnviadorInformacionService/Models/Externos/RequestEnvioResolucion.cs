using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class RequestEnvioResolucion
    {
        public IEnumerable<string> guidsFacturasPendientes;

        public Resolucion Resolucion { get; internal set; }

        public Guid Estacion { get; set; }
    }
}
