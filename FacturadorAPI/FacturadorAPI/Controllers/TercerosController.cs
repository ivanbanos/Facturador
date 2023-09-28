using FacturadorAPI.Models;
using FacturadorAPI.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using FacturadorAPI.Application.Commands;

namespace FacturadorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TercerosController : ControllerBase
    {
        private readonly ILogger<TercerosController> _logger;
        private readonly IMediator _mediator;


        public TercerosController(ILogger<TercerosController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [Route("TiposIdentificacion")]
        [ProducesResponseType(typeof(IEnumerable<TipoIdentificacion>), (int)HttpStatusCode.OK)]
        public async Task<IEnumerable<TipoIdentificacion>> GetListTiposIdentificacion(CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ListarTiposIdentificacionQuery(), cancellationToken);
        }


        [HttpGet]
        [Route("{identificacion}")]
        [ProducesResponseType(typeof(IEnumerable<Tercero>), (int)HttpStatusCode.OK)]
        public async Task<IEnumerable<Tercero>> ObtenerTerceroPorIDentificacion(string identificacion, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ObtenerTerceroPorIDentificacionQuery(identificacion), cancellationToken);
        }


        [HttpPost]
        [ProducesResponseType(typeof(Tercero), (int)HttpStatusCode.OK)]
        public async Task<Tercero> CrearTercero(Tercero tercero, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new CrearTerceroCommand(tercero), cancellationToken);
        }
    }
}
