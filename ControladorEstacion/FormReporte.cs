using FactoradorEstacionesModelo.Siges;
using FacturadorEstacionesRepositorio;
using Modelo;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ControladorEstacion
{
    public partial class FormReporte : Form
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly InfoEstacion _infoEstacion;
        string tipoReporte;

        private sealed class LecturaRow
        {
            public DateTime Fecha { get; set; }
            public int TurnoId { get; set; }
            public string Isla { get; set; } = string.Empty;
            public string Manguera { get; set; } = string.Empty;
            public string Articulo { get; set; } = string.Empty;
            public double Precio { get; set; }
            public double LecturaInicial { get; set; }
            public double LecturaFinal { get; set; }
            public double Diferencia { get; set; }
            public double Ventas { get; set; }
        }

        private sealed class ArticuloRow
        {
            public string Articulo { get; set; } = string.Empty;
            public int Ventas { get; set; }
            public double Cantidad { get; set; }
            public double ValorNeto { get; set; }
            public double Subtotal { get; set; }
            public double Descuento { get; set; }
            public double Recaudo { get; set; }
            public double Total { get; set; }
        }

        private sealed class FormaPagoRow
        {
            public int Codigo { get; set; }
            public string Descripcion { get; set; } = string.Empty;
            public int Ventas { get; set; }
            public double Cantidad { get; set; }
            public double Total { get; set; }
        }

        public FormReporte(string tipo, IEstacionesRepositorio estacionesRepositorio, InfoEstacion infoEstacion)
        {
            InitializeComponent();
            this.Text += " " + tipo;
            tipoReporte = tipo;
            _estacionesRepositorio = estacionesRepositorio;
            _infoEstacion = infoEstacion;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var fechaInicio = this.dateTimePicker1.Value.Date;
                var fechaFin = this.dateTimePicker2.Value.Date;

                var facturas = _estacionesRepositorio
                    .GetFacturasPorFechas(fechaInicio, fechaFin.AddDays(1))
                    .ToList();

                var turnos = _estacionesRepositorio
                    .GetTurnosByFechas(fechaInicio, fechaFin.AddDays(1))
                    .ToList();

                var outputPath = $"{_infoEstacion.Reportes}/reporte-{tipoReporte}-{fechaInicio:dd-MM-yyyy}-{fechaFin:dd-MM-yyyy}.pdf";

                if (tipoReporte == "lecturas")
                {
                    var rows = new List<LecturaRow>();
                    var cantidadTotal = 0d;
                    var ventaTotal = 0d;

                    foreach (var turno in turnos)
                    {
                        var turnoinfo = _estacionesRepositorio.ObtenerTurnoInfo(turno.Id);
                        foreach (var turnosurtidor in turnoinfo)
                        {
                            if (!turnosurtidor.Cierre.HasValue)
                            {

                                continue;
                            }

                            var diferencia = turnosurtidor.Cierre.Value - turnosurtidor.Apertura;
                            var venta = diferencia * turnosurtidor.Combustible.Precio;

                            cantidadTotal += diferencia;
                            ventaTotal += venta;

                            rows.Add(new LecturaRow
                            {
                                Fecha = turno.FechaApertura,
                                TurnoId = turno.Id,
                                Isla = turno.Isla,
                                Manguera = turnosurtidor.Manguera.Descripcion,
                                Articulo = turnosurtidor.Combustible.Descripcion,
                                Precio = turnosurtidor.Combustible.Precio,
                                LecturaInicial = turnosurtidor.Apertura,
                                LecturaFinal = turnosurtidor.Cierre.Value,
                                Diferencia = diferencia,
                                Ventas = venta,
                            });
                        }

                    }

                    GenerateLecturasPdf(outputPath, fechaInicio, fechaFin, rows, cantidadTotal, ventaTotal);
                }
                else
                {
                    var formasPago = _estacionesRepositorio.BuscarFormasPagosSiges();
                    var formasPagoPorId = formasPago
                        .GroupBy(x => x.Id)
                        .ToDictionary(x => x.Key, x => x.First().Descripcion);


                    var groupArticulo = facturas.GroupBy(x => string.IsNullOrWhiteSpace(x.Combustible) ? "Sin articulo" : x.Combustible.Trim());

                    var articuloRows = new List<ArticuloRow>();

                    foreach (var articulo in groupArticulo)
                    {
                        var cantidadArticulo = articulo.Sum(x => x.Cantidad);
                        var valorNetoArticulo = articulo.Sum(x => x.Subtotal);
                        var descuentoArticulo = articulo.Sum(x => x.Descuento);
                        var totalArticulo = articulo.Sum(x => x.Total);

                        articuloRows.Add(new ArticuloRow
                        {
                            Articulo = articulo.Key,
                            Ventas = articulo.Count(),
                            Cantidad = cantidadArticulo,
                            ValorNeto = valorNetoArticulo,
                            Subtotal = valorNetoArticulo,
                            Descuento = descuentoArticulo,
                            Recaudo = totalArticulo,
                            Total = totalArticulo,
                        });
                    }

                    var cantidadTotal = facturas.Sum(x => x.Cantidad);
                    var valorNetoTotal = facturas.Sum(x => x.Subtotal);
                    var descuentoTotal = facturas.Sum(x => x.Descuento);
                    var recaudoTotal = facturas.Sum(x => x.Total);


                    var resumenFormasPago = new List<(int Codigo, int Ventas, double Cantidad, double Total)>();

                    foreach (var factura in facturas)
                    {
                        var totalFactura = factura.Total <= 0 ? 0d : factura.Total;
                        var totalPrimario = factura.codigoFormaPago2.HasValue
                            ? Math.Max(0d, factura.total1 ?? 0d)
                            : totalFactura;
                        var totalSecundario = factura.codigoFormaPago2.HasValue
                            ? Math.Max(0d, factura.total2 ?? 0d)
                            : 0d;

                        var cantidadPrimaria = totalFactura > 0 ? factura.Cantidad * (totalPrimario / totalFactura) : factura.Cantidad;
                        var cantidadSecundaria = totalFactura > 0 ? factura.Cantidad * (totalSecundario / totalFactura) : 0d;

                        if (totalPrimario > 0)
                        {
                            resumenFormasPago.Add((factura.codigoFormaPago, 1, cantidadPrimaria, totalPrimario));
                        }

                        if (factura.codigoFormaPago2.HasValue && totalSecundario > 0)
                        {
                            resumenFormasPago.Add((factura.codigoFormaPago2.Value, 1, cantidadSecundaria, totalSecundario));
                        }
                    }

                    var formaPagoRows = resumenFormasPago
                        .GroupBy(x => x.Codigo)
                        .OrderBy(x => x.Key)
                        .Select(forma => new FormaPagoRow
                        {
                            Codigo = forma.Key,
                            Descripcion = formasPagoPorId.ContainsKey(forma.Key) ? formasPagoPorId[forma.Key] : "No configurada",
                            Ventas = forma.Sum(x => x.Ventas),
                            Cantidad = forma.Sum(x => x.Cantidad),
                            Total = forma.Sum(x => x.Total),
                        })
                        .ToList();

                    GenerateVentasPdf(
                        outputPath,
                        fechaInicio,
                        fechaFin,
                        articuloRows,
                        formaPagoRows,
                        facturas.Count(),
                        cantidadTotal,
                        valorNetoTotal,
                        descuentoTotal,
                        recaudoTotal);
                }

                MessageBox.Show("Reporte generado con exito");
                this.Close();
            }
            catch (Exception)
            {
                MessageBox.Show($"Error generando reporte. Pruebe cerrando el archivo ya generado. Si persiste el error, por favor, comunicarse con asistencia.");
                this.Close();

            }

        }

        private void GenerateLecturasPdf(string outputPath, DateTime fechaInicio, DateTime fechaFin, List<LecturaRow> rows, double cantidadTotal, double ventaTotal)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontFamily("Calibri").FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().Text(_infoEstacion.Razon).FontSize(18).SemiBold();
                        column.Item().Text($"NIT {_infoEstacion.NIT}").FontSize(11).FontColor(Colors.Grey.Darken2);
                        column.Item().PaddingTop(4).Text("Reporte de Control de Lecturas de Turnos").FontSize(13).SemiBold().FontColor(Colors.Blue.Darken3);
                        column.Item().Text($"Desde {fechaInicio:dd/MM/yyyy} hasta {fechaFin:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(12).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(36);
                            columns.ConstantColumn(42);
                            columns.RelativeColumn(1.2f);
                            columns.RelativeColumn(1.2f);
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(58);
                            columns.ConstantColumn(58);
                            columns.ConstantColumn(58);
                            columns.ConstantColumn(65);
                        });

                        static IContainer HeaderCell(IContainer container)
                        {
                            return container
                                .Background(Colors.Blue.Darken2)
                                .Border(1)
                                .BorderColor(Colors.White)
                                .PaddingVertical(4)
                                .PaddingHorizontal(3)
                                .DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold().FontSize(9));
                        }

                        static IContainer BodyCell(IContainer container)
                        {
                            return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(3);
                        }

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Fecha");
                            header.Cell().Element(HeaderCell).AlignCenter().Text("Turno");
                            header.Cell().Element(HeaderCell).AlignCenter().Text("Isla");
                            header.Cell().Element(HeaderCell).Text("Manguera");
                            header.Cell().Element(HeaderCell).Text("Articulo");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Precio");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Lec. Inicial");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Lec. Final");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Diferencia");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Ventas");
                        });

                        foreach (var row in rows)
                        {
                            table.Cell().Element(BodyCell).Text(row.Fecha.ToString("dd/MM/yyyy"));
                            table.Cell().Element(BodyCell).AlignCenter().Text(row.TurnoId.ToString());
                            table.Cell().Element(BodyCell).AlignCenter().Text(row.Isla);
                            table.Cell().Element(BodyCell).Text(row.Manguera);
                            table.Cell().Element(BodyCell).Text(row.Articulo);
                            table.Cell().Element(BodyCell).AlignRight().Text(FormatCurrency(row.Precio));
                            table.Cell().Element(BodyCell).AlignRight().Text(FormatNumber(row.LecturaInicial));
                            table.Cell().Element(BodyCell).AlignRight().Text(FormatNumber(row.LecturaFinal));
                            table.Cell().Element(BodyCell).AlignRight().Text(FormatNumber(row.Diferencia));
                            table.Cell().Element(BodyCell).AlignRight().Text(FormatCurrency(row.Ventas));
                        }

                        table.Cell().ColumnSpan(8).Element(cell => cell.Background(Colors.Grey.Lighten3).Padding(4)).Text("Total").SemiBold();
                        table.Cell().Element(cell => cell.Background(Colors.Grey.Lighten3).Padding(4).AlignRight()).Text(FormatNumber(cantidadTotal)).SemiBold();
                        table.Cell().Element(cell => cell.Background(Colors.Grey.Lighten3).Padding(4).AlignRight()).Text(FormatCurrency(ventaTotal)).SemiBold();
                    });

                    page.Footer().AlignRight().Text($"Reporte generado en {DateTime.Now:dd/MM/yyyy HH:mm}, por SIGES SOLUCIONES SAS").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            }).GeneratePdf(outputPath);
        }

        private void GenerateVentasPdf(
            string outputPath,
            DateTime fechaInicio,
            DateTime fechaFin,
            List<ArticuloRow> articuloRows,
            List<FormaPagoRow> formaPagoRows,
            int totalVentas,
            double cantidadTotal,
            double valorNetoTotal,
            double descuentoTotal,
            double recaudoTotal)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontFamily("Calibri").FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().Text(_infoEstacion.Razon).FontSize(18).SemiBold();
                        column.Item().Text($"NIT {_infoEstacion.NIT}").FontSize(11).FontColor(Colors.Grey.Darken2);
                        column.Item().PaddingTop(4).Text("Reporte de Ventas por Articulo Resumido").FontSize(13).SemiBold().FontColor(Colors.Blue.Darken3);
                        column.Item().Text($"Desde {fechaInicio:dd/MM/yyyy} hasta {fechaFin:dd/MM/yyyy}").FontSize(10).FontColor(Colors.Grey.Darken2);
                    });

                    page.Content().PaddingTop(12).Column(column =>
                    {
                        column.Spacing(12);
                        column.Item().Element(c => ComposeArticulosTable(c, articuloRows, totalVentas, cantidadTotal, valorNetoTotal, descuentoTotal, recaudoTotal));
                        column.Item().Text("Reporte de Formas de Pago").FontSize(12).SemiBold().FontColor(Colors.Blue.Darken3);
                        column.Item().Element(c => ComposeFormaPagoTable(c, formaPagoRows));
                    });

                    page.Footer().AlignRight().Text($"Reporte generado en {DateTime.Now:dd/MM/yyyy HH:mm}, por SIGES SOLUCIONES SAS").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            }).GeneratePdf(outputPath);
        }

        private void ComposeArticulosTable(
            IContainer container,
            List<ArticuloRow> rows,
            int totalVentas,
            double cantidadTotal,
            double valorNetoTotal,
            double descuentoTotal,
            double recaudoTotal)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.4f);
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(70);
                });

                static IContainer HeaderCell(IContainer c) => c.Background(Colors.Blue.Darken2).Border(1).BorderColor(Colors.White).Padding(4).DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold().FontSize(9));
                static IContainer BodyCell(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Articulo");
                    header.Cell().Element(HeaderCell).AlignCenter().Text("No Ventas");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Cantidad");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Valor Neto");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Subtotal");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Descuento");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Recaudo");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Total");
                });

                foreach (var row in rows)
                {
                    table.Cell().Element(BodyCell).Text(row.Articulo);
                    table.Cell().Element(BodyCell).AlignCenter().Text(row.Ventas.ToString());
                    table.Cell().Element(BodyCell).AlignRight().Text(FormatNumber(row.Cantidad));
                    table.Cell().Element(BodyCell).AlignRight().Text(FormatCurrency(row.ValorNeto));
                    table.Cell().Element(BodyCell).AlignRight().Text(FormatCurrency(row.Subtotal));
                    table.Cell().Element(BodyCell).AlignRight().Text(FormatCurrency(row.Descuento));
                    table.Cell().Element(BodyCell).AlignRight().Text(FormatCurrency(row.Recaudo));
                    table.Cell().Element(BodyCell).AlignRight().Text(FormatCurrency(row.Total));
                }

                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4)).Text("Total").SemiBold();
                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4).AlignCenter()).Text(totalVentas.ToString()).SemiBold();
                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4).AlignRight()).Text(FormatNumber(cantidadTotal)).SemiBold();
                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4).AlignRight()).Text(FormatCurrency(valorNetoTotal)).SemiBold();
                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4).AlignRight()).Text(FormatCurrency(valorNetoTotal)).SemiBold();
                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4).AlignRight()).Text(FormatCurrency(descuentoTotal)).SemiBold();
                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4).AlignRight()).Text(FormatCurrency(recaudoTotal)).SemiBold();
                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4).AlignRight()).Text(FormatCurrency(recaudoTotal)).SemiBold();
            });
        }

        private void ComposeFormaPagoTable(IContainer container, List<FormaPagoRow> rows)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1.4f);
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(90);
                });

                static IContainer HeaderCell(IContainer c) => c.Background(Colors.Blue.Darken2).Border(1).BorderColor(Colors.White).Padding(4).DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold().FontSize(9));
                static IContainer BodyCell(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(4);

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Código forma de pago");
                    header.Cell().Element(HeaderCell).AlignCenter().Text("No Ventas");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Cantidad");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Total");
                });

                foreach (var row in rows)
                {
                    table.Cell().Element(BodyCell).Text($"{row.Codigo} {row.Descripcion}");
                    table.Cell().Element(BodyCell).AlignCenter().Text(row.Ventas.ToString());
                    table.Cell().Element(BodyCell).AlignRight().Text(FormatNumber(row.Cantidad));
                    table.Cell().Element(BodyCell).AlignRight().Text(FormatCurrency(row.Total));
                }

                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4)).Text("Total").SemiBold();
                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4).AlignCenter()).Text(rows.Sum(x => x.Ventas).ToString()).SemiBold();
                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4).AlignRight()).Text(FormatNumber(rows.Sum(x => x.Cantidad))).SemiBold();
                table.Cell().Element(c => c.Background(Colors.Grey.Lighten3).Padding(4).AlignRight()).Text(FormatCurrency(rows.Sum(x => x.Total))).SemiBold();
            });
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("N2");
        }

        private static string FormatCurrency(double value)
        {
            return $"${value:N2}";
        }
    }
}
