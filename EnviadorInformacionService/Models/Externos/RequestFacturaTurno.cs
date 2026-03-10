using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnviadorInformacionService.Models.Externos
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
