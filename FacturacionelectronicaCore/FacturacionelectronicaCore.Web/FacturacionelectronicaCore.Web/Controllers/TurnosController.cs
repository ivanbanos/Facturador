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
    public class TurnosController : ControllerBase
    {
        private readonly ITurnoNegocio _turnoNegocio;

        public TurnosController(ITurnoNegocio turnoNegocio)
        {
            _turnoNegocio = turnoNegocio;
        }

        [HttpPost("GetTurnoReporte")]
        public async Task<ActionResult<IEnumerable<TurnoReporte>>> Get(FiltroBusqueda filtroFactura)
        {

            var result = await _turnoNegocio.Get(filtroFactura.FechaInicial.Value, filtroFactura.FechaFinal.Value, filtroFactura.Estacion.ToString());

            if (result == null) { return NotFound(); }

            return Ok(result);
        }


        [HttpPost]
        public async Task<ActionResult<int>> AddOrUpdate(Turno turno)
        {
            await _turnoNegocio.Add(turno);
            return Ok();
        }
    }
}
