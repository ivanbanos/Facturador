using FacturadorAPI.Application.Queries;
using FacturadorAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FacturadorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstacionController : ControllerBase
    {
        private readonly ILogger<EstacionController> _logger;
        private readonly IMediator _mediator;


        public EstacionController(ILogger<EstacionController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [Route("Islas")]
        [ProducesResponseType(typeof(IEnumerable<IslaSiges>), (int)HttpStatusCode.OK)]
        public async Task<IEnumerable<IslaSiges>> ListarIslasSiges(CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ListarIslasSigesQuery(), cancellationToken);
        }

        [HttpGet]
        [Route("CarasPorIsla")]
        [ProducesResponseType(typeof(IEnumerable<CaraSiges>), (int)HttpStatusCode.OK)]
        public async Task<IEnumerable<CaraSiges>> ObtenerCarasPorIsla(int idIsla, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ObtenerCarasPorIslaQuery(idIsla), cancellationToken);
        }

        [HttpGet]
        [Route("TurnoPorIsla")]
        [ProducesResponseType(typeof(TurnoSiges), (int)HttpStatusCode.OK)]
        public async Task<TurnoSiges> ObtenerTurnoPorIsla(int idIsla, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ObtenerTurnoPorIslaQuery(idIsla), cancellationToken);
        }


        [HttpGet]
        [Route("Surtidores")]
        [ProducesResponseType(typeof(IEnumerable<SurtidorSiges>), (int)HttpStatusCode.OK)]
        public async Task<IEnumerable<SurtidorSiges>> ObtenerSurtidores(CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ObtenerSurtidoresQuery(), cancellationToken);
        }
    }
}
