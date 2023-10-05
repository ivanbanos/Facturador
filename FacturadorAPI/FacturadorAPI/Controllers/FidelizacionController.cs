﻿using FacturadorAPI.Application.Commands;
using FacturadorAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FacturadorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FidelizacionController : ControllerBase
    {
        private readonly ILogger<FidelizacionController> _logger;
        private readonly IMediator _mediator;


        public FidelizacionController(ILogger<FidelizacionController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost]
        [Route("FidelizarVenta/{identificacion}/{idVenta}")]
        [ProducesResponseType(typeof(IEnumerable<Canastilla>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ListarCanastillas(string identificacion, int idVenta, CancellationToken cancellationToken)
        {
            await _mediator.Send(new FidelizarVentaCommand(identificacion, idVenta), cancellationToken);
            return Ok();
        }
    }
}
