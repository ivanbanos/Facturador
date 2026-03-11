using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class RequestFacturaTurno
    {
        public int idVentaLocal { get; set; }
        public DateTime fecha { get; set; }
        public string isla { get; set; }
        public int numero { get; set; }
        public Guid estacion { get; set; }
    }
}
