using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class FacturaCanastillaReporte
    {
        public IEnumerable<FacturacionelectronicaCore.Repositorio.Entities.FacturaCanastilla> Facturas { get; set; }
        public IEnumerable<DetalleFormaPago> DetalleFormaPago { get; set; }
        public List<DetalleArticulo> DetalleArticulo { get; internal set; }
    }
    public class DetalleFormaPago
    {
        public string FormaDePago { get; set; }
        public int Cantidad { get; set; }
        public double Precio { get; set; }
    }
}
