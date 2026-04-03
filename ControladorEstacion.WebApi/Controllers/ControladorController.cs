using ControladorEstacion.WebApi.Contracts;
using ControladorEstacion.WebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Linq;

namespace ControladorEstacion.WebApi.Controllers;

[ApiController]
[Route("api/v1/controlador")]
public sealed class ControladorController : ControllerBase
{
    private readonly ILegacyFacturadorClient _legacyClient;
    private readonly IReportePdfService _pdfService;

    public ControladorController(ILegacyFacturadorClient legacyClient, IReportePdfService pdfService)
    {
        _legacyClient = legacyClient;
        _pdfService = pdfService;
    }

    [HttpGet("surtidores")]
    [ProducesResponseType(typeof(IEnumerable<SurtidorResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSurtidores(CancellationToken cancellationToken)
    {
        var surtidores = await _legacyClient.GetSurtidoresAsync(cancellationToken);
        return Ok(surtidores);
    }

    [HttpPost("reportes/lecturas")]
    [EnableRateLimiting("reportes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReporteLecturas([FromBody] ReporteRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidateReporteRequest(request);
        if (validation is not null)
        {
            return validation;
        }

        var data = await _legacyClient.GetReporteLecturasAsync(request, cancellationToken);
        return Ok(data);
    }

    [HttpPost("reportes/ventas")]
    [EnableRateLimiting("reportes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReporteVentas([FromBody] ReporteRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidateReporteRequest(request);
        if (validation is not null)
        {
            return validation;
        }

        var data = await _legacyClient.GetReporteVentasAsync(request, cancellationToken);
        return Ok(data);
    }

    [HttpPost("reportes/lecturas.pdf")]
    [EnableRateLimiting("reportes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReporteLecturasPdf([FromBody] ReporteRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidateReporteRequest(request);
        if (validation is not null)
        {
            return validation;
        }

        // Fetch data from legacy API
        var data = await _legacyClient.GetReporteLecturasAsync(request, cancellationToken);
        
        // Generate PDF
        var pdfBytes = await _pdfService.GenerarPdfLecturasAsync(request, data.ToList());

        var fileName = $"Reporte_Lecturas_{request.FechaInicio:yyyyMMdd}_{request.FechaFin:yyyyMMdd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpPost("reportes/ventas.pdf")]
    [EnableRateLimiting("reportes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetReporteVentasPdf([FromBody] ReporteRequest request, CancellationToken cancellationToken)
    {
        var validation = ValidateReporteRequest(request);
        if (validation is not null)
        {
            return validation;
        }

        // Fetch data from legacy API
        var data = await _legacyClient.GetReporteVentasAsync(request, cancellationToken);
        
        // Generate PDF
        var pdfBytes = await _pdfService.GenerarPdfVentasAsync(request, data.ToList());

        var fileName = $"Reporte_Ventas_{request.FechaInicio:yyyyMMdd}_{request.FechaFin:yyyyMMdd}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "ok" });
    }

    private BadRequestObjectResult? ValidateReporteRequest(ReporteRequest request)
    {
        if (request.FechaInicio > request.FechaFin)
        {
            return BadRequest("FechaInicio no puede ser mayor que FechaFin");
        }

        if ((request.FechaFin - request.FechaInicio).TotalDays > 365)
        {
            return BadRequest("El rango maximo permitido es 365 dias");
        }

        var now = DateTime.UtcNow.Date;
        if (request.FechaInicio.Date > now || request.FechaFin.Date > now)
        {
            return BadRequest("No se permiten fechas futuras");
        }

        return null;
    }
}
