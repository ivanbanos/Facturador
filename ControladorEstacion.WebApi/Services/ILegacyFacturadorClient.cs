using ControladorEstacion.WebApi.Contracts;
using ControladorEstacion.WebApi.Services;

namespace ControladorEstacion.WebApi.Services;

public interface ILegacyFacturadorClient
{
    Task<IReadOnlyList<SurtidorResponse>> GetSurtidoresAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ReporteLecturaDto>> GetReporteLecturasAsync(ReporteRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<ReporteVentaDto>> GetReporteVentasAsync(ReporteRequest request, CancellationToken cancellationToken);
}
