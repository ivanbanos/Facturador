using FacturadorAPI.Application.Commands;
using FacturadorAPI.Application.Queries;
using FacturadorAPI.Models;
using FacturadorApiSP.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FacturadorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CanastillaController : ControllerBase
    {
        private readonly ILogger<CanastillaController> _logger;
        private readonly IMediator _mediator;


        public CanastillaController(ILogger<CanastillaController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Canastilla>), (int)HttpStatusCode.OK)]
        public async Task<IEnumerable<Canastilla>> ListarCanastillas(CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ProcesarYObtenerCanastillasCommand(), cancellationToken);
        }


        [HttpPost]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> AgregarCanastilla(FacturaCanastillaRequest facturaCanastilla, CancellationToken cancellationToken)
        {
            
            return Ok(await _mediator.Send(new AgregarFacturaCanastillaCommand(facturaCanastilla), cancellationToken));
        }


        [HttpPost]
        [Route("CerrarTurno/{isla}")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CerrarTurno(int isla, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new MandarImprimirTurnoCanastillaCommand(isla), cancellationToken));
        }

        [HttpPost]
        [Route("ReimprimirFacturaCanastilla/{consecutivo}")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ReimprimirFacturaCanastilla(int consecutivo, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ReimprimirFacturaCanastillaCommand(consecutivo), cancellationToken);
            return Ok();
        }

        [HttpPost]
        [Route("ReimprimirTurnoCanastilla/{fecha}/{isla}/{posicion}")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ReimprimirTurnoCanastilla(DateTime fecha, int isla, int posicion, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new ReimprimirTurnoCanastillaCommand(fecha, isla, posicion), cancellationToken));
        }
    }
}
