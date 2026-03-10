using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnviadorInformacionService.Models
{
    public class Canastilla
    {
        public int CanastillaId { get; set; }
        public string descripcion { get; set; }
        public string unidad { get; set; }
        public float precio { get; set; }

        public bool deleted { get; set; }
        public Guid guid { get; set; }
        public int iva { get; set; }
    }
}
