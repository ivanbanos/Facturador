using FactoradorEstacionesModelo.Objetos;
using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Text;

namespace FacturadorAPI.Application.Commands
{
    public class MandarImprimirCommandHandler : IRequestHandler<MandarImprimirCommand, string>
    {
        private readonly ILogger<MandarImprimirCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly InfoEstacion _infoEstacion;

        public MandarImprimirCommandHandler(ILogger<MandarImprimirCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota,
            IOptions<InfoEstacion> infoEstacion)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
            _infoEstacion = infoEstacion.Value;
        }

        public async Task<string> Handle(MandarImprimirCommand request, CancellationToken cancellationToken)
        {
            await WriteDebugLogAsync($"INICIO MandarImprimir ventaId={request.VentaId}, facturaPOSId={request.FacturaPOSId}, terceroId={request.TerceroId}, formaPago={request.FormaPago}, formaPago2={request.FormaPago2}, total1={request.Total1}, total2={request.Total2}, impresiones={request.Impresiones}, placa={request.Placa}, kilometraje={request.Kilometraje}, numeroTransaccion={request.NumeroTransaccion}");
            try
            {
                var token = await _conexionEstacionRemota.GetToken(cancellationToken);
                var factura = await _databaseHandler.GetFacturaPorIdVenta(request.VentaId);
                await WriteDebugLogAsync($"Factura antes de actualizar: ventaId={factura.ventaId}, facturaPOSId={factura.facturaPOSId}, enviada={factura.enviada}, codigoFormaPago={factura.codigoFormaPago}, codigoFormaPago2={factura.codigoFormaPago2}, total1={factura.total1}, total2={factura.total2}");
                
                if (!factura.enviada)
                {
                    await WriteDebugLogAsync("Entra a ActualizarFactura porque enviada = false");
                    await _databaseHandler.ActualizarFactura(
                        factura.facturaPOSId,
                        request.TerceroId,
                        request.FormaPago,
                        request.VentaId,
                        request.Placa == "NP" ? "" : request.Placa,
                        request.Kilometraje == "NP" ? "" : request.Kilometraje,
                        request.NumeroTransaccion == "NP" ? "" : request.NumeroTransaccion,
                        request.FormaPago2,
                        request.Total1,
                        request.Total2);

                    factura = await _databaseHandler.GetFacturaPorIdVenta(request.VentaId);
                    await WriteDebugLogAsync($"Factura despues de actualizar: ventaId={factura.ventaId}, facturaPOSId={factura.facturaPOSId}, enviada={factura.enviada}, codigoFormaPago={factura.codigoFormaPago}, codigoFormaPago2={factura.codigoFormaPago2}, total1={factura.total1}, total2={factura.total2}");

                }
                else
                {
                    await WriteDebugLogAsync("No entra a ActualizarFactura porque enviada = true");
                }

                var facturaSIGES = ConvertToFacturaSIGES(factura);
                try
                {
                    var formas = await _databaseHandler.ListarFormasPagoSP(cancellationToken);
                    await _conexionEstacionRemota.EnviarFacturas(new List<FacturaSiges>() { facturaSIGES }, formas, token);

                    await _databaseHandler.ActuralizarFacturasEnviados(new List<int>() { request.VentaId });
                    await WriteDebugLogAsync($"Factura enviada a SIGES y marcada como enviada: ventaId={request.VentaId}");

                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error {ex.Message}");
                    Console.WriteLine($"Error {ex.StackTrace}");
                    await WriteDebugLogAsync($"Error enviando factura a SIGES: {ex.Message}");

                }
                if (!factura.enviada)
                {
                    await _databaseHandler.MandarImprimir(request.VentaId, request.Impresiones);
                    await WriteDebugLogAsync($"MandarImprimir ejecutado (factura no enviada previamente). ventaId={request.VentaId}");
                    return "NoChange";
                }
                else
                {
                    await _databaseHandler.MandarImprimir(request.VentaId, request.Impresiones);
                    await WriteDebugLogAsync($"MandarImprimir ejecutado (factura ya enviada previamente). ventaId={request.VentaId}");
                    return "Ok";
                }
            }
            catch (Exception ex)
            {
                await WriteDebugLogAsync($"Excepcion general en Handle: {ex.Message}");
                var factura = await _databaseHandler.GetFacturaPorIdVenta(request.VentaId);
                await WriteDebugLogAsync($"Factura en catch antes de actualizar: ventaId={factura.ventaId}, facturaPOSId={factura.facturaPOSId}, enviada={factura.enviada}, codigoFormaPago={factura.codigoFormaPago}, codigoFormaPago2={factura.codigoFormaPago2}, total1={factura.total1}, total2={factura.total2}");

                if (!factura.enviada)
                {
                    await WriteDebugLogAsync("Catch: entra a ActualizarFactura porque enviada = false");
                    await _databaseHandler.ActualizarFactura(
                        factura.facturaPOSId,
                        request.TerceroId,
                        request.FormaPago,
                        request.VentaId,
                        request.Placa == "NP" ? "" : request.Placa,
                        request.Kilometraje == "NP" ? "" : request.Kilometraje,
                        request.NumeroTransaccion == "NP" ? "" : request.NumeroTransaccion,
                        request.FormaPago2,
                        request.Total1,
                        request.Total2);

                    factura = await _databaseHandler.GetFacturaPorIdVenta(request.VentaId);
                    await WriteDebugLogAsync($"Catch: factura despues de actualizar: ventaId={factura.ventaId}, facturaPOSId={factura.facturaPOSId}, enviada={factura.enviada}, codigoFormaPago={factura.codigoFormaPago}, codigoFormaPago2={factura.codigoFormaPago2}, total1={factura.total1}, total2={factura.total2}");

                }
                else
                {
                    await WriteDebugLogAsync("Catch: no entra a ActualizarFactura porque enviada = true");
                }
                Console.WriteLine($"Error {ex.Message}");
                Console.WriteLine($"Error {ex.StackTrace}");
                await _databaseHandler.MandarImprimir(request.VentaId, request.Impresiones); 
                await WriteDebugLogAsync($"Catch: MandarImprimir ejecutado. ventaId={request.VentaId}");
                return "Error";
            }
        }

        private async Task WriteDebugLogAsync(string message)
        {
            try
            {
                var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
                Directory.CreateDirectory(logDirectory);
                var logFile = Path.Combine(logDirectory, "mandar-imprimir-debug.log");
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}";
                await File.AppendAllTextAsync(logFile, line, Encoding.UTF8);
            }
            catch
            {
                // No interrumpir el flujo funcional por errores de logging de diagnostico.
            }
        }
        private FacturaSiges ConvertToFacturaSIGES(Factura factura)
        {
            return new FacturaSiges()
            {
                Autorizacion = factura.Autorizacion,
                Cantidad = (double)factura.Venta.CANTIDAD,
                Cara = factura.Venta.COD_CAR.ToString(),
                codigoFormaPago = factura.codigoFormaPago,
                codigoFormaPago2 = factura.codigoFormaPago2,
                total1 = factura.total1,
                total2 = factura.total2,
                CodigoInterno = factura.Venta.COD_INT,
                Combustible = factura.Venta.Combustible,
                Consecutivo = factura.Consecutivo,
                Manguera = ConvertirMangueraSiges(factura.Manguera),
                DescripcionResolucion = factura.DescripcionResolucion,
                facturaPOSId = factura.facturaPOSId,
                fecha = factura.fecha,
                FechaFinalResolucion = factura.FechaFinalResolucion,
                FechaInicioResolucion = factura.FechaInicioResolucion,
                Final = factura.Final,
                Inicio = factura.Inicio,
                habilitada = factura.habilitada,
                Estado = factura.Estado,
                impresa = factura.impresa,
                Kilometraje = factura.Kilometraje,
                Placa = factura.Placa,
                ventaId = factura.ventaId,
                vecesImpresa = factura.vecesImpresa,

                Surtidor = factura.Venta.COD_SUR.ToString(),
                Mangueras = factura.Manguera.COD_MAN.ToString(),
                Precio = (double)factura.Venta.PRECIO_UNI,
                Total = (double)factura.Venta.TOTAL,
                Subtotal = (double)factura.Venta.SUBTOTAL,
                Descuento = (double)factura.Venta.Descuento,
                Empleado = factura.Venta.EMPLEADO,
                fechaProximoMantenimiento = factura.Venta.FECH_PRMA,

                Tercero = factura.Tercero
            };

        }

        private MangueraSiges ConvertirMangueraSiges(Manguera manguera)
        {
            return new MangueraSiges() { Id = manguera.COD_MAN, Descripcion = manguera.DESCRIPCION };
        }
    }
}