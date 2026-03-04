using FactoradorEstacionesModelo.Objetos;
using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

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
            try
            {
                var token = await _conexionEstacionRemota.GetToken(cancellationToken);
                var factura = await _databaseHandler.GetFacturaPorIdVenta(request.VentaId);
                
                if (!factura.enviada)
                {
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

                }

                var facturaSIGES = ConvertToFacturaSIGES(factura);
                try
                {
                    var formas = await _databaseHandler.ListarFormasPagoSP(cancellationToken);
                    await _conexionEstacionRemota.EnviarFacturas(new List<FacturaSiges>() { facturaSIGES }, formas, token);

                    await _databaseHandler.ActuralizarFacturasEnviados(new List<int>() { request.VentaId });

                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error {ex.Message}");
                    Console.WriteLine($"Error {ex.StackTrace}");

                }
                if (!factura.enviada)
                {
                    await _databaseHandler.MandarImprimir(request.VentaId, request.Impresiones);
                    return "NoChange";
                }
                else
                {
                    await _databaseHandler.MandarImprimir(request.VentaId, request.Impresiones);
                    return "Ok";
                }
            }
            catch (Exception ex)
            {
                var factura = await _databaseHandler.GetFacturaPorIdVenta(request.VentaId);

                if (!factura.enviada)
                {
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

                }
                Console.WriteLine($"Error {ex.Message}");
                Console.WriteLine($"Error {ex.StackTrace}");
                await _databaseHandler.MandarImprimir(request.VentaId, request.Impresiones); 
                return "Error";
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