using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;

namespace FacturacionelectronicaCore.Web.Controllers
{
    public class RequestCambiarFechasReporte
    {
        public IEnumerable<FacturaFechaReporte> facturas { get; set; }

        public Guid Estacion { get;  set; }
    }
}