using EstacionesServicio.Modelo;
using FacturacionelectronicaCore.Negocio.Estacion;
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
        private readonly IEstacionNegocio _estacionNegocio;

        public FacturasCanastillaController(IFacturaCanastillaNegocio facturaNegocio, IEstacionNegocio estacionNegocio)
        {
            _facturaNegocio = facturaNegocio;
            _estacionNegocio = estacionNegocio;
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

        /// <summary>
        /// Reporte de facturas de canastilla de un cliente en todas las estaciones, con el nombre de la estación.
        /// Para usuarios cliente la identificación se toma del token; para los demás es obligatoria.
        /// </summary>
        [HttpPost("TodasLasEstaciones")]
        public async Task<ActionResult<FacturaCanastillaReporte>> GetFacturasReporteTodasLasEstaciones(FiltroBusqueda filtroFactura)
        {
            var filtro = User.RestringirFiltroACliente(filtroFactura ?? new FiltroBusqueda());
            if (string.IsNullOrWhiteSpace(filtro.Identificacion))
            {
                return BadRequest(new { message = NombresEstaciones.MensajeIdentificacionRequerida });
            }
            var errorRango = NombresEstaciones.ValidarRango(filtro.FechaInicial, filtro.FechaFinal);
            if (errorRango != null)
            {
                return BadRequest(new { message = errorRango });
            }

            var reporte = await _facturaNegocio.GetFacturasReporte(filtro.FechaInicial, filtro.FechaFinal, filtro.Identificacion, filtro.NombreTercero, null);
            var nombreEstacion = await NombresEstaciones.Obtener(_estacionNegocio);
            var facturas = (reporte.Facturas ?? Enumerable.Empty<FacturaCanastilla>()).ToList();
            foreach (var factura in facturas)
            {
                factura.EstacionNombre = nombreEstacion(factura.IdEstacion);
            }
            reporte.Facturas = facturas;
            return Ok(reporte);
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
