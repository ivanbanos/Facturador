using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class CupoCliente
    {
        public string Id { get; set; }
        public string Cliente { get; set; }
        public string COD_CLI { get; set; }
        public string Nit { get; set; }
        public double CupoAsignado { get; set; }
        public double CupoDisponible { get; set; }
        public string EstacionGuid { get; set; }
    }
}
