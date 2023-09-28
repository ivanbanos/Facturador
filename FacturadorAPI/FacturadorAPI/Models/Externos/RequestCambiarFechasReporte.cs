using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;

namespace FacturadorAPI.Models.Externos
{
    public class RequestCambiarFechasReporte
    {
        public IEnumerable<FacturaFechaReporte> facturas { get; set; }

        public Guid Estacion { get; set; }
    }
}