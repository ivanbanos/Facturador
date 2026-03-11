
using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class RequestCambiarFechasReporte
    {
        public IEnumerable<FacturaFechaReporte> facturas { get; set; }

        public Guid Estacion { get;  set; }
    }
}