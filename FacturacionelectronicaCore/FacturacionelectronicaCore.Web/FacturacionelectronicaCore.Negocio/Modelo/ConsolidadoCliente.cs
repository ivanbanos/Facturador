using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class ConsolidadoCliente
    {
        public string Cliente { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Total { get; set; }
    }
}
