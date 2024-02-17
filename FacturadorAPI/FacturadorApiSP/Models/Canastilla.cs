using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturadorAPI.Models
{
    public class Canastilla
    {
        internal bool deleted;

        public int CanastillaId { get; set; }
        public string descripcion { get; set; }
        public string unidad { get; set; }
        public float precio { get; set; }
        public Guid IdWeb { get; set; }
        public int iva { get; set; }
        public Guid guid { get; internal set; }
    }
}
