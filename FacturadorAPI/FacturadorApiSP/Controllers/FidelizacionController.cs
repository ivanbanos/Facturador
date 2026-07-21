using FacturadorAPI.Application.Commands;
using FacturadorAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

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
        [Route("FidelizarVenta/{identificacion}/{ventaId}")]
        [ProducesResponseType(typeof(IEnumerable<Canastilla>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> FidelizarVenta(string identificacion, int ventaId, [FromBody] FidelizarVentaRequestDto dto, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(new FidelizarVentaCommand(
                    identificacion,
                    ventaId,
                    dto.FacturaPOSId,
                    dto.TerceroId,
                    dto.CodigoFormaPago,
                    dto.Placa ?? "NP",
                    dto.Kilometraje ?? "NP",
                    dto.NumeroTransaccion ?? "NP",
                    dto.CodigoFormaPago2,
                    dto.Total1,
                    dto.Total2), cancellationToken);
                return Ok();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Forbidden)
            {
                _logger.LogWarning(ex, "Fidelizacion service returned 403 for identificacion {Identificacion} and ventaId {VentaId}", identificacion, ventaId);
                return StatusCode((int)HttpStatusCode.Forbidden,
                    "Fidelizacion service rejected the request (403). Verify credentials and permissions in InfoEstacion configuration.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error calling fidelizacion service for identificacion {Identificacion} and ventaId {VentaId}", identificacion, ventaId);
                return StatusCode((int)HttpStatusCode.BadGateway,
                    "Error calling fidelizacion service.");
            }
            catch(Exception ex)
            {
                if(ex.Message == "Venta fidelizada" || ex.Message == "Tercero no existe")
                {
                    return BadRequest(ex.Message);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
