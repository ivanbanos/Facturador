using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public  class RequestEnviarFacturas
    {
        public IEnumerable<Factura> facturas { get; set; }
        public IEnumerable<Negocio.Modelo.OrdenDeDespacho> ordenDeDespachos { get; set; }
        public Guid Estacion { get; set; }
    }
}
