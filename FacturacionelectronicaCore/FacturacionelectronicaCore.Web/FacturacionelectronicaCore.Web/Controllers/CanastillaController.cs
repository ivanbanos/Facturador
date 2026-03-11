using FacturacionelectronicaCore.Negocio.Canastilla;
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
    public class CanastillaController : ControllerBase
    {
        private readonly ICanastillaNegocio _canastillaNegocio;

        public CanastillaController(ICanastillaNegocio canastillaNegocio)
        {
            _canastillaNegocio = canastillaNegocio;
        }

        [HttpGet("{guid}")]
        public async Task<ActionResult<Canastilla>> Get(Guid guid)
        {
            if (guid == Guid.Empty) { return BadRequest(); }

            var result = await _canastillaNegocio.GetCanastilla(guid);

            if (result == null) { return NotFound(); }

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Canastilla>>> Get([FromQuery] Guid? estacion = null)
        => Ok(await _canastillaNegocio.GetCanastillas(estacion));

        [HttpPost]
        public async Task<ActionResult<int>> AddOrUpdate(IEnumerable<Canastilla> canastillas)
        {
            if (!canastillas.Any())
            {
                return BadRequest();
            }

            return Ok(await _canastillaNegocio.AddOrUpdate(canastillas));
        }
    }
}
