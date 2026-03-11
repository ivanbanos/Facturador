using EstacionesServicio.Modelo;
using FacturacionelectronicaCore.Negocio.Estacion;
using FacturacionelectronicaCore.Negocio.Modelo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstacionesController : Controller
    {
        private readonly IEstacionNegocio _estacionNegocio;

        public EstacionesController(IEstacionNegocio estacionNegocio)
        {
            _estacionNegocio = estacionNegocio;
        }

        [HttpGet]
        public async Task<IEnumerable<Estacion>> BuscarEstaciones()
        => await _estacionNegocio.GetEstaciones();

        [HttpPost]
        public async Task<ActionResult> CrearEstacion(IEnumerable<Estacion> estaciones)
        {

            if (!estaciones.Any())
            {
                return BadRequest();
            }

            await _estacionNegocio.AddEstacion(estaciones);

            return Ok();
        }

        [HttpGet("{guid}")]
        public async Task<ActionResult<Estacion>> GetEstacion(Guid guid)
        {
            if (guid == Guid.Empty) { return BadRequest(); }

            var result = await _estacionNegocio.GetEstacion(guid);

            if (result == null) { return NotFound(); }

            return Ok(result);
        }

        [HttpPost("BorrarEstaciones")]
        public async Task<ActionResult<int>> BorrarEstacion(IEnumerable<FacturasEntity> estaciones)
        {
            if (!estaciones.Any())
            {
                return BadRequest();
            }

            var response = await _estacionNegocio.BorrarEstacion(estaciones);

            return Ok(response);
        }
    }
}
