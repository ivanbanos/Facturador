using FacturadorAPI.Application.Commands;
using FacturadorAPI.Models;
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

        [HttpPost]
        [Route("CrearAnticipo")]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CrearAnticipo([FromBody] CrearAnticipoRequest request, CancellationToken cancellationToken)
        {
            if (request.Monto <= 0)
                return BadRequest("El monto debe ser mayor a cero.");

            if (string.IsNullOrWhiteSpace(request.Nombre) && string.IsNullOrWhiteSpace(request.Placa))
                return BadRequest("Debe indicar al menos un nombre o una placa.");

            await _mediator.Send(
                new CrearAnticipoCommand(
                    request.IdIsla,
                    request.NumTurno,
                    request.FechaTurno,
                    request.Nombre,
                    request.Placa,
                    request.Monto,
                    request.TurnoGuid),
                cancellationToken);

            return Ok();
        }
    }

    public class CrearAnticipoRequest
    {
        public int IdIsla { get; set; }
        public int NumTurno { get; set; }
        public DateTime FechaTurno { get; set; }
        public string Nombre { get; set; }
        public string Placa { get; set; }
        public decimal Monto { get; set; }
        public string TurnoGuid { get; set; }
    }
}
