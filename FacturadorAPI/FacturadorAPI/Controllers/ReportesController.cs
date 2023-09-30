using FacturadorAPI.Application.Queries;
using FacturadorAPI.Application.Queries.Reportes;
using FacturadorAPI.Application.Queries.Reportes.Objetos;
using FacturadorAPI.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FacturadorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly ILogger<ReportesController> _logger;
        private readonly IMediator _mediator;


        public ReportesController(ILogger<ReportesController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost]
        [Route("Articulo")]
        [ProducesResponseType(typeof(ReporteArticuloResponse), (int)HttpStatusCode.OK)]
        public async Task<ReporteArticuloResponse> REporteArticulos(ReporteGeneralRequest request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ReporteArticuloQuery(request), cancellationToken);
        }

        [HttpPost]
        [Route("Lecturas")]
        [ProducesResponseType(typeof(ReporteLecturasGeneralResponse), (int)HttpStatusCode.OK)]
        public async Task<ReporteLecturasGeneralResponse> ReporteLecturas(ReporteGeneralRequest request, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ReporteLecturasGeneralQuery(request), cancellationToken);
        }
    }
}
