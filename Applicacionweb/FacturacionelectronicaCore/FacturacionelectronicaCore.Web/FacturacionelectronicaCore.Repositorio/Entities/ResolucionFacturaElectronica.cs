using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class ResolucionFacturaElectronica
    {
        public int numeroActual { get; set; }
        public string resolucion { get; set; }
        public string prefijo { get; set; }
        public string correo { get; set; }
        public string token { get; set; }
        public string idNumeracion { get; set; }
    }
}
