using System.Net.Http.Json;
using ControladorEstacion.WebApi.Configuration;
using ControladorEstacion.WebApi.Contracts;
using Microsoft.Extensions.Options;

namespace ControladorEstacion.WebApi.Services;

public sealed class LegacyFacturadorClient : ILegacyFacturadorClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LegacyFacturadorClient> _logger;

    public LegacyFacturadorClient(
        HttpClient httpClient,
        IOptions<LegacyApiOptions> options,
        ILogger<LegacyFacturadorClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        var baseUrl = options.Value.BaseUrl.TrimEnd('/');
        _httpClient.BaseAddress = new Uri(baseUrl + "/");
    }

    public async Task<IReadOnlyList<SurtidorResponse>> GetSurtidoresAsync(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetFromJsonAsync<List<SurtidorResponse>>(
            "api/Estacion/Surtidores",
            cancellationToken);

        return result ?? new List<SurtidorResponse>();
    }

    public async Task<IReadOnlyList<ReporteLecturaDto>> GetReporteLecturasAsync(ReporteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync("api/Reportes/Lecturas", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Legacy API call failed. Route: {Route}, StatusCode: {StatusCode}",
                    "api/Reportes/Lecturas",
                    (int)response.StatusCode);

                return new List<ReporteLecturaDto>();
            }

            // Try to deserialize as a list
            var result = await response.Content.ReadFromJsonAsync<List<ReporteLecturaDto>>(cancellationToken: cancellationToken);
            return result ?? new List<ReporteLecturaDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing reporte lecturas from legacy API");
            return new List<ReporteLecturaDto>();
        }
    }

    public async Task<IReadOnlyList<ReporteVentaDto>> GetReporteVentasAsync(ReporteRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync("api/Reportes/Articulo", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Legacy API call failed. Route: {Route}, StatusCode: {StatusCode}",
                    "api/Reportes/Articulo",
                    (int)response.StatusCode);

                return new List<ReporteVentaDto>();
            }

            // Try to deserialize as a list
            var result = await response.Content.ReadFromJsonAsync<List<ReporteVentaDto>>(cancellationToken: cancellationToken);
            return result ?? new List<ReporteVentaDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing reporte ventas from legacy API");
            return new List<ReporteVentaDto>();
        }
    }
}
