using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;

namespace FacturacionelectronicaCore.Web.Controllers
{
    public class RequestfacturasCanastilla
    {
        public IEnumerable<FacturaCanastilla> facturas { get; set; }
        public Guid estacion { get; set; }
    }
}