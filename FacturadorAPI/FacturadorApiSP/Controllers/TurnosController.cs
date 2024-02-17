using FacturadorAPI.Application.Commands;
using FacturadorAPI.Models;
using FacturadorApiSP.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FacturadorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TurnosController : ControllerBase
    {
        private readonly ILogger<TurnosController> _logger;
        private readonly IMediator _mediator;


        public TurnosController(ILogger<TurnosController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost]
        [Route("AbrirTurno/{isla}/{codigo}")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AbrirTurno(int isla, string codigo, CancellationToken cancellationToken)
        {
            await _mediator.Send(new AbrirTurnoCommand(isla, codigo), cancellationToken);
            return Ok();
        }

        [HttpPost]
        [Route("CerrarTurno/{isla}/{codigo}")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CerrarTurno(int isla, string codigo, CancellationToken cancellationToken)
        {
            await _mediator.Send(new CerrarTurnoCommand(isla, codigo), cancellationToken);
            return Ok();
        }


        [HttpPost]
        [Route("reimprimirTurno/{fecha}/{isla}/{posicion}")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ReimprimirTurno(DateTime fecha, int isla, int posicion, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ReimprimirTurnoCommand(fecha, isla, posicion), cancellationToken);
            return Ok();
        }


        [HttpGet]
        [Route("AgregarBolsa/{isla}/{codigo}/{cantidad}/{moneda}/{numero}")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AgregarBolsa(int isla, string codigo, string cantidad, string moneda, string numero, CancellationToken cancellationToken)
        {
            await _mediator.Send(new AgregarBolsaCommand(isla, codigo, cantidad, moneda, numero), cancellationToken);
            return Ok();
        }

    }
}
