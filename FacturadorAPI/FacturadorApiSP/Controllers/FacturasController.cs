using FactoradorEstacionesModelo.Objetos;
using FacturadorAPI.Application.Commands;
using FacturadorAPI.Application.Queries;
using FacturadorAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Numerics;

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
        [Route("EnviarFacturaElectronica/{idVenta}/{TerceroId}/{FormaPago}/{VentaId}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> EnviarFacturaElectronica(int idVenta, int TerceroId, int FormaPago, int VentaId, string Kilometraje, string Placa, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EnviarFacturaElectronicaCommand(idVenta, TerceroId, FormaPago, VentaId, Placa, Kilometraje), cancellationToken); 
            return Ok();
        }

        [HttpPost]
        [Route("Imprimir/{FacturaPOSId}/{TerceroId}/{FormaPago}/{VentaId}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> MandarImprimir(int FacturaPOSId, int TerceroId, int FormaPago, int VentaId, string Kilometraje, string Placa,  CancellationToken cancellationToken)
        {
            await _mediator.Send(new MandarImprimirCommand(FacturaPOSId, TerceroId, FormaPago, VentaId, Placa, Kilometraje), cancellationToken);
            return Ok();
        }

        [HttpPost]
        [Route("ImprimirPorConsecutivo/{consecutivo}")]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> MandarImprimirConsecutivo(string consecutivo, CancellationToken cancellationToken)
        {
            await _mediator.Send(new MandarImprimirConsecutivoCommand(consecutivo), cancellationToken);
            return Ok();
        }


    }
}
