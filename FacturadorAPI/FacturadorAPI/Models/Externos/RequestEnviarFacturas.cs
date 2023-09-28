using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models.Externos
{
    public class RequestEnviarFacturas
    {
        public IEnumerable<FacturaExterna> facturas { get; set; }
        public IEnumerable<OrdenDeDespacho> ordenDeDespachos { get; set; }
        public Guid Estacion { get; set; }
    }
}
