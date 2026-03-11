using FacturacionelectronicaCore.Negocio.Tercero;
using FacturacionelectronicaCore.Negocio.Turno;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using FacturacionelectronicaCore.Negocio.Modelo;
using Microsoft.AspNetCore.Authorization;

namespace FacturacionelectronicaCore.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuposInfoController : ControllerBase
    {
        private readonly ICupoNegocio _cupoNegocio;

        public CuposInfoController(ICupoNegocio cupoNegocio)
        {
            _cupoNegocio = cupoNegocio;
        }
        [HttpPost]
        public async Task<ActionResult<int>> AddOrUpdate(CuposRequest request)
        {
            await _cupoNegocio.Add(request);
            return Ok();
        }
        [HttpGet("Automotores/{estacion}")]
        public async Task<ActionResult<int>> Automotores(string estacion)
        {
            return Ok(await _cupoNegocio.GetCupoAutomotor(estacion));
        }
        [HttpGet("Clientes/{estacion}")]
        public async Task<ActionResult<int>> Clientes(string estacion)
        {
            return Ok(await _cupoNegocio.GetCupoCliente(estacion));
        }
    }
}
