using FacturacionelectronicaCore.Negocio.Modelo;
using FacturacionelectronicaCore.Negocio.Tercero;
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
    public class TercerosController : ControllerBase
    {
        private readonly ITerceroNegocio _terceroNegocio;

        public TercerosController(ITerceroNegocio terceroNegocio)
        {
            _terceroNegocio = terceroNegocio;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tercero>>> Get()
        => Ok(await _terceroNegocio.GetTerceros());

        [HttpGet("{guid}")]
        public async Task<ActionResult<Tercero>> Get(Guid guid)
        {
            if (guid == Guid.Empty) { return BadRequest(); }

            var result = await _terceroNegocio.GetTercero(guid);

            if (result == null) { return NotFound(); }

            return  Ok(result);
        }
       

        [HttpPost] 
        public async Task<ActionResult<int>> AddOrUpdate(IEnumerable<Tercero> terceros)
        {
            if (!terceros.Any())
            {
                return BadRequest();
            }

            return Ok(await _terceroNegocio.AddOrUpdate(terceros));
        }

        [HttpGet("GetIsTerceroValidoPorIdentificacion/{identificacion}")]
        public async Task<ActionResult<bool>> GetIsTerceroValidoPorIdentificacion(string identificacion)
        {
            if (string.IsNullOrEmpty(identificacion)) { return BadRequest(); }

            var result = await _terceroNegocio.GetIsTerceroValidoPorIdentificacion(identificacion);


            return Ok(result);
        }

        [HttpPost("SincronizarTerceros")]
        public async Task<ActionResult> SincronizarTerceros()
        {

            await _terceroNegocio.SincronizarTerceros();


            return Ok();
        }
        
    }
}
