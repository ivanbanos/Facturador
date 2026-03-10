using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnviadorInformacionService.Models
{
    public class CuposRequest
    {
        public IEnumerable<CupoAutomotor> cuposAutomotores { get; set; }
        public IEnumerable<CupoCliente> cuposClientes { get; set; }
        public string EstacionGuid { get; set; }
    }

    public class CupoAutomotor
    {
        public string Cliente { get; set; }
        public string COD_CLI { get; set; }
        public string Nit { get; set; }
        public string Placa { get; set; }
        public double CupoAsignado { get; set; }
        public double CupoDisponible { get; set; }
        public string EstacionGuid { get; set; }
    }
    public class CupoCliente
    {
        public string Cliente { get; set; }
        public string COD_CLI { get; set; }
        public string Nit { get; set; }
        public double CupoAsignado { get; set; }
        public double CupoDisponible { get; set; }
        public string EstacionGuid { get; set; }
    }
}
