using EstacionesServicio.Modelo;
using FacturacionelectronicaCore.Negocio.FacturaCanastillaNegocio;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FacturaCanastilla = FacturacionelectronicaCore.Repositorio.Entities.FacturaCanastilla;

namespace FacturacionelectronicaCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturasCanastillaController
    {
        private readonly IFacturaCanastillaNegocio _facturaNegocio;

        public FacturasCanastillaController(IFacturaCanastillaNegocio facturaNegocio)
        {
            _facturaNegocio = facturaNegocio;
        }

        [HttpPost("GetFactura")]
        public async Task<IEnumerable<FacturaCanastilla>> BuscarFacturas(FiltroBusqueda filtroFactura)
        => await _facturaNegocio.GetFacturas(filtroFactura.FechaInicial, filtroFactura.FechaFinal, filtroFactura.Identificacion, filtroFactura.NombreTercero, filtroFactura.Estacion);
       
        [HttpPost("GetFacturasReporte")]
        public async Task<FacturaCanastillaReporte> GetFacturasReporte(FiltroBusqueda filtroFactura)
                => await _facturaNegocio.GetFacturasReporte(filtroFactura.FechaInicial, filtroFactura.FechaFinal, filtroFactura.Identificacion, filtroFactura.NombreTercero, filtroFactura.Estacion);


        [HttpGet("{idFactura}")]
        public async Task<FacturaCanastilla> Get(string idFactura)
            => await _facturaNegocio.GetFactura(idFactura);


        [HttpGet("detalle/{idFactura}")]
        public async Task<IEnumerable<FacturaCanastilla>> GetDEtalle(string idFactura)
            => await _facturaNegocio.GetDetalleFactura(idFactura);



        [HttpGet("ColocarEspera/{idFactura}/Estacion/{idEstacion}")]
        public async Task<int> ColocarEspera(string idFactura, Guid idEstacion)
        { _facturaNegocio.ColocarEspera(idFactura, idEstacion);return 0; }



        [HttpGet("ObtenerParaImprimir/Estacion/{idEstacion}")]
        public async Task<int> ObtenerParaImprimir(Guid idEstacion)
            => await _facturaNegocio.ObtenerParaImprimir(idEstacion);
    }
}
