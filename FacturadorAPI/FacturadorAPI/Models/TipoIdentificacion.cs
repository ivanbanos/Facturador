using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models
{
    public class TipoIdentificacion
    {
        public int TipoIdentificacionId { get; set; }
        public short CodigoDian { get; set; }
        public string Descripcion { get; set; }
    }
}
