using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ControladorEstacion.WebApi.Contracts;

namespace ControladorEstacion.WebApi.Services;

/// <summary>
/// Service to generate PDF reports for surtidor readings and sales.
/// Uses QuestPDF for professional document generation.
/// </summary>
public interface IReportePdfService
{
    Task<byte[]> GenerarPdfLecturasAsync(ReporteRequest request, List<ReporteLecturaDto> datos);
    Task<byte[]> GenerarPdfVentasAsync(ReporteRequest request, List<ReporteVentaDto> datos);
}

public class ReportePdfService : IReportePdfService
{
    private readonly ILogger<ReportePdfService> _logger;

    public ReportePdfService(ILogger<ReportePdfService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerarPdfLecturasAsync(ReporteRequest request, List<ReporteLecturaDto> datos)
    {
        _logger.LogInformation(
            "Generando PDF de lecturas: {FechaInicio} a {FechaFin}",
            request.FechaInicio.ToShortDateString(),
            request.FechaFin.ToShortDateString()
        );

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);

                page.Header().Text("REPORTE DE LECTURAS").FontSize(14).Bold();

                page.Content().Column(column =>
                {
                    // Report info
                    column.Item().Text(t =>
                    {
                        t.Span("Período: ").Bold();
                        t.Span($"{request.FechaInicio:dd/MM/yyyy} a {request.FechaFin:dd/MM/yyyy}");
                    });

                    column.Item().Text(t =>
                    {
                        t.Span("Generado: ").Bold();
                        t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    });

                    column.Item().PaddingVertical(10).Text("");

                    // Table
                    if (datos.Count == 0)
                    {
                        column.Item().Text("No hay datos disponibles.").Italic();
                    }
                    else
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn((float)1);
                                columns.RelativeColumn((float)1);
                                columns.RelativeColumn((float)1.5);
                                columns.RelativeColumn((float)1.5);
                                columns.RelativeColumn((float)1.5);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#051D38").Text("Surtidor").FontColor(Colors.White).Bold();
                                header.Cell().Background("#051D38").Text("Turno").FontColor(Colors.White).Bold();
                                header.Cell().Background("#051D38").Text("Km Inicial").FontColor(Colors.White).Bold();
                                header.Cell().Background("#051D38").Text("Km Final").FontColor(Colors.White).Bold();
                                header.Cell().Background("#051D38").Text("Fecha").FontColor(Colors.White).Bold();
                            });

                            foreach (var row in datos)
                            {
                                table.Cell().Text(row.NumeroSurtidor?.ToString() ?? "-");
                                table.Cell().Text(row.NumeroTurno?.ToString() ?? "-");
                                table.Cell().Text(row.KmInicial ?? "-");
                                table.Cell().Text(row.KmFinal ?? "-");
                                table.Cell().Text(row.FechaHora?.ToString("dd/MM/yyyy HH:mm") ?? "-");
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text($"Generado automáticamente - {DateTime.Now:dd/MM/yyyy}").Italic();
            });
        });

        return await Task.FromResult(documento.GeneratePdf());
    }

    public async Task<byte[]> GenerarPdfVentasAsync(ReporteRequest request, List<ReporteVentaDto> datos)
    {
        _logger.LogInformation(
            "Generando PDF de ventas: {FechaInicio} a {FechaFin}",
            request.FechaInicio.ToShortDateString(),
            request.FechaFin.ToShortDateString()
        );

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);

                page.Header().Text("REPORTE DE VENTAS").FontSize(14).Bold();

                page.Content().Column(column =>
                {
                    // Report info
                    column.Item().Text(t =>
                    {
                        t.Span("Período: ").Bold();
                        t.Span($"{request.FechaInicio:dd/MM/yyyy} a {request.FechaFin:dd/MM/yyyy}");
                    });

                    column.Item().Text(t =>
                    {
                        t.Span("Generado: ").Bold();
                        t.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    });

                    column.Item().PaddingVertical(10).Text("");

                    // Table
                    if (datos.Count == 0)
                    {
                        column.Item().Text("No hay datos disponibles.").Italic();
                    }
                    else
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn((float)1);
                                columns.RelativeColumn((float)1);
                                columns.RelativeColumn((float)1.5);
                                columns.RelativeColumn((float)1.5);
                                columns.RelativeColumn((float)1.5);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#051D38").Text("Surtidor").FontColor(Colors.White).Bold();
                                header.Cell().Background("#051D38").Text("Turno").FontColor(Colors.White).Bold();
                                header.Cell().Background("#051D38").Text("Venta Total").FontColor(Colors.White).Bold();
                                header.Cell().Background("#051D38").Text("Venta Contado").FontColor(Colors.White).Bold();
                                header.Cell().Background("#051D38").Text("Fecha").FontColor(Colors.White).Bold();
                            });

                            decimal totalVentas = 0;
                            decimal totalContado = 0;

                            foreach (var row in datos)
                            {
                                table.Cell().Text(row.NumeroSurtidor?.ToString() ?? "-");
                                table.Cell().Text(row.NumeroTurno?.ToString() ?? "-");
                                table.Cell().Text(row.VentaTotal?.ToString("C") ?? "-");
                                table.Cell().Text(row.VentaContado?.ToString("C") ?? "-");
                                table.Cell().Text(row.FechaHora?.ToString("dd/MM/yyyy HH:mm") ?? "-");

                                totalVentas += row.VentaTotal ?? 0;
                                totalContado += row.VentaContado ?? 0;
                            }

                            // Totals row
                            table.Cell().Background("#051D38").Text("TOTAL").Bold().FontColor(Colors.White);
                            table.Cell().Background("#051D38").Text("").FontColor(Colors.White);
                            table.Cell().Background("#051D38").Text(totalVentas.ToString("C")).Bold().FontColor(Colors.White);
                            table.Cell().Background("#051D38").Text(totalContado.ToString("C")).Bold().FontColor(Colors.White);
                            table.Cell().Background("#051D38").Text("").FontColor(Colors.White);
                        });
                    }
                });

                page.Footer().AlignCenter().Text($"Generado automáticamente - {DateTime.Now:dd/MM/yyyy}").Italic();
            });
        });

        return await Task.FromResult(documento.GeneratePdf());
    }
}

/// <summary>
/// DTOs for PDF report data.
/// </summary>
public class ReporteLecturaDto
{
    public int? NumeroSurtidor { get; set; }
    public int? NumeroTurno { get; set; }
    public string? KmInicial { get; set; }
    public string? KmFinal { get; set; }
    public DateTime? FechaHora { get; set; }
}

public class ReporteVentaDto
{
    public int? NumeroSurtidor { get; set; }
    public int? NumeroTurno { get; set; }
    public decimal? VentaTotal { get; set; }
    public decimal? VentaContado { get; set; }
    public DateTime? FechaHora { get; set; }
}
