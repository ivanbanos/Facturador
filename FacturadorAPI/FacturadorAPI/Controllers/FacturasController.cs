using FacturadorAPI.Application.Commands;
using FacturadorAPI.Application.Queries;
using FacturadorAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FacturadorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturasController : ControllerBase
    {
        private readonly ILogger<FacturasController> _logger;
        private readonly IMediator _mediator;


        public FacturasController(ILogger<FacturasController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpGet]
        [Route("FormasDePago")]
        [ProducesResponseType(typeof(IEnumerable<FormaPagoSiges>), (int)HttpStatusCode.OK)]
        public async Task<IEnumerable<FormaPagoSiges>> ListarFormasPagoSigesQuery(CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ListarFormasPagoSigesQuery(), cancellationToken);
        }


        [HttpGet]
        [Route("UltimaFacturaPorCara/{idCara}")]
        [ProducesResponseType(typeof(FacturaSiges), (int)HttpStatusCode.OK)]
        public async Task<FacturaSiges> UltimaFacturaPorCara(int idCara, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ObtenerUltimaFacturaPorCaraQuery(idCara), cancellationToken);
        }


        [HttpGet]
        [Route("UltimaFacturaPorCara/{idCara}/Texto")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<string> UltimaFacturaPorCaraText(int idCara, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ObtenerUltimaFacturaPorCaraTextoQuery(idCara), cancellationToken);
        }


        [HttpGet]
        [Route("ConvertirAFactura/{idVenta}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ConvertirAFactura(int idVenta, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ConvertirAFacturaCommand(idVenta), cancellationToken);
            return Ok();
        }

        [HttpGet]
        [Route("ConvertirAOrden/{idVenta}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> ConvertirAOrden(int idVenta, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ConvertirAOrdenCommand(idVenta), cancellationToken); 
            return Ok();
        }

        [HttpPost]
        [Route("EnviarFacturaElectronica/{idVenta}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> EnviarFacturaElectronica(int idVenta, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EnviarFacturaElectronicaCommand(idVenta), cancellationToken); 
            return Ok();
        }

        [HttpPost]
        [Route("Imprimir/{FacturaPOSId}/{TerceroId}/{FormaPago}/{VentaId}/{Placa}/{Kilometraje}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> MandarImprimir(int FacturaPOSId, int TerceroId, int FormaPago, int VentaId, string Placa, string Kilometraje, CancellationToken cancellationToken)
        {
            await _mediator.Send(new MandarImprimirCommand(FacturaPOSId, TerceroId, FormaPago, VentaId, Placa, Kilometraje), cancellationToken);
            return Ok();
        }
        


    }
}
