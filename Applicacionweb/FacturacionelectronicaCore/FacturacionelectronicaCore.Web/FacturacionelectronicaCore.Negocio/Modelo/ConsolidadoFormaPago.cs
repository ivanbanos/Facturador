using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class ConsolidadoFormaPago
    {
        public string FormaPago { get; set; }
        public int CantidadFacturas { get; set; }          // Número de facturas/órdenes
        public decimal CantidadCombustible { get; set; }   // Cantidad total en litros/galones
        public decimal Total { get; set; }
    }
}
