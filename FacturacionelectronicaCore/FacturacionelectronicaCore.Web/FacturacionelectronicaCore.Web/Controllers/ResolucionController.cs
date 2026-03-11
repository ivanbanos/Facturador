using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Negocio.Resolucion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacturacionelectronicaCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResolucionController : ControllerBase
    {
        private readonly IResolucionNegocio _resolucionNegocio;

        public ResolucionController(IResolucionNegocio resolucionNegocio)
        {
            _resolucionNegocio = resolucionNegocio;
        }


        [HttpGet("{estacion}")]
        public async Task<ActionResult<IEnumerable<Resolucion>>> GetResolucionActiva(Guid estacion)
        {
            var result = await _resolucionNegocio.GetResolucionActiva(estacion);

            if (result == null) { return NotFound(); }

            return Ok(result);
        }

        [HttpGet("resoluciones/{estacion}")]
        public async Task<ActionResult<IEnumerable<Resolucion>>> GetResolucionesActiva(Guid estacion)
        {
            var result = await _resolucionNegocio.GetResolucionActiva(estacion);

            if (result == null) { return NotFound(); }

            return Ok(result);
        }


        [HttpPost("AddNuevaResolucion")]
        public async Task<ActionResult<int>> AddNuevaResolucion(CreacionResolucion resolucion)
        {

            return Ok(await _resolucionNegocio.AddNuevaResolucion(resolucion));
        }

        [HttpPost("HabilitarResolucion/{resolucion}")]
        public async Task<ActionResult<int>> HabilitarResolucion(Guid resolucion,[FromBody] DateTime fechaVencimiento)
        {

            return Ok(await _resolucionNegocio.HabilitarResolucion(resolucion, fechaVencimiento));
        }


        [HttpGet("AnularResolucion/{resolucion}")]
        public async Task<ActionResult<int>> AnularResolucion(Guid resolucion)
        {
            await _resolucionNegocio.AnularResolucion(resolucion);
            return Ok();
        }
    }
}
