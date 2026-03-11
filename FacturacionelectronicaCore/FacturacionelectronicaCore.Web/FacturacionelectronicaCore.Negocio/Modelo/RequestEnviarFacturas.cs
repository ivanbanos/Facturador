using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public  class RequestEnviarFacturas
    {
        public List<Factura> facturas { get; set; }
        public List<Negocio.Modelo.OrdenDeDespacho> ordenDeDespachos { get; set; }
        public Guid Estacion { get; set; }
    }
}
