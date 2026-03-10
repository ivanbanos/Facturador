using System;
using System.Collections.Generic;
using System.Text;

namespace ManejadorSurtidor.SICOM
{
    public class ChipResponse
    {
        public string idrom { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_fin { get; set; }
        public string placa { get; set; }
        public string vin { get; set; }
        public string servicio { get; set; }
        public int capacidad { get; set; }
        public string estado { get; set; }
        public object motivo { get; set; }
        public object motivo_texto { get; set; }
    }
}
