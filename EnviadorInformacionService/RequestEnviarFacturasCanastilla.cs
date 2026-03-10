using EnviadorInformacionService.Models;
using System;
using System.Collections.Generic;

namespace EnviadorInformacionService
{
    internal class RequestEnviarFacturasCanastilla
    {
        internal IEnumerable<FacturaCanastilla> facturas;

        public Guid Estacion { get; internal set; }
    }
}