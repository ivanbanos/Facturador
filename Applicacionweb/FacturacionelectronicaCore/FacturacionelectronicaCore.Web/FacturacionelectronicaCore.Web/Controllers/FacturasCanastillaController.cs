using EstacionesServicio.Modelo;
using FacturacionelectronicaCore.Negocio.FacturaCanastillaNegocio;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Web.Authtentication;
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
    public class FacturasCanastillaController : ControllerBase
    {
        private readonly IFacturaCanastillaNegocio _facturaNegocio;

        public FacturasCanastillaController(IFacturaCanastillaNegocio facturaNegocio)
        {
            _facturaNegocio = facturaNegocio;
        }

        [HttpPost("GetFactura")]
        public async Task<IEnumerable<FacturaCanastilla>> BuscarFacturas(FiltroBusqueda filtroFactura)
        {
            filtroFactura = User.RestringirFiltroACliente(filtroFactura);
            return await _facturaNegocio.GetFacturas(filtroFactura.FechaInicial, filtroFactura.FechaFinal, filtroFactura.Identificacion, filtroFactura.NombreTercero, filtroFactura.Estacion);
        }

        [HttpPost("GetFacturasReporte")]
        public async Task<FacturaCanastillaReporte> GetFacturasReporte(FiltroBusqueda filtroFactura)
        {
            filtroFactura = User.RestringirFiltroACliente(filtroFactura);
            return await _facturaNegocio.GetFacturasReporte(filtroFactura.FechaInicial, filtroFactura.FechaFinal, filtroFactura.Identificacion, filtroFactura.NombreTercero, filtroFactura.Estacion);
        }


        [HttpGet("{idFactura}")]
        public async Task<ActionResult<FacturaCanastilla>> Get(string idFactura)
        {
            var factura = await _facturaNegocio.GetFactura(idFactura);
            if (factura != null && !PerteneceAlCliente(factura))
            {
                return Forbid();
            }
            return factura;
        }


        [HttpGet("detalle/{idFactura}")]
        public async Task<ActionResult<IEnumerable<FacturaCanastilla>>> GetDEtalle(string idFactura)
        {
            var detalle = await _facturaNegocio.GetDetalleFactura(idFactura);
            if (detalle != null && !detalle.All(PerteneceAlCliente))
            {
                return Forbid();
            }
            return Ok(detalle);
        }

        private bool PerteneceAlCliente(FacturaCanastilla factura)
            => !User.EsCliente() || User.EsDelCliente(factura?.terceroId?.Identificacion);



        [HttpGet("ColocarEspera/{idFactura}/Estacion/{idEstacion}")]
        public async Task<int> ColocarEspera(string idFactura, Guid idEstacion)
        { _facturaNegocio.ColocarEspera(idFactura, idEstacion);return 0; }



        [HttpGet("ObtenerParaImprimir/Estacion/{idEstacion}")]
        public async Task<int> ObtenerParaImprimir(Guid idEstacion)
            => await _facturaNegocio.ObtenerParaImprimir(idEstacion);
    }
}
