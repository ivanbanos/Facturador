using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models
{
    public class CaraSiges
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
        public string Impresora { get; set; }
        
        public string Isla { get; set; }
        public int IdIsla { get; set; }
    }
}
