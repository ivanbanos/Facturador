using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacturadorAPI.Models;

namespace FacturadorAPI.Models.Externos
{
    public class RequestfacturasCanastilla
    {
        public IEnumerable<FacturaCanastilla> facturas { get; set; }
        public Guid estacion { get; set; }
    }
}
