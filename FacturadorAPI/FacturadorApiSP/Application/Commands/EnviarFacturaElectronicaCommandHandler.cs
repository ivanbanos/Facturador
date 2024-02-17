using FactoradorEstacionesModelo.Objetos;
using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class EnviarFacturaElectronicaCommandHandler : IRequestHandler<EnviarFacturaElectronicaCommand>
    {
        private readonly ILogger<EnviarFacturaElectronicaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public EnviarFacturaElectronicaCommandHandler(ILogger<EnviarFacturaElectronicaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<Unit> Handle(EnviarFacturaElectronicaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var factura = await _databaseHandler.GetFacturaPorIdVenta(request.IdFactura);
                var facturaSIGES = ConvertToFacturaSIGES(factura);
                await _databaseHandler.ActualizarFactura(factura.facturaPOSId, request.TerceroId, request.FormaPago, request.VentaId, request.Placa == "NP" ? "" : request.Placa, request.Kilometraje == "NP" ? "" : request.Kilometraje);
                factura = await _databaseHandler.GetFacturaPorIdVenta(request.IdFactura);

                facturaSIGES = ConvertToFacturaSIGES(factura);
                try
                {
                    var token = await _conexionEstacionRemota.GetToken(cancellationToken);
                    var formas = await _databaseHandler.ListarFormasPagoSP(cancellationToken);
                    await _conexionEstacionRemota.EnviarFacturas(new List<FacturaSiges>() { facturaSIGES }, formas, token);

                    if (factura.Consecutivo == 0)
                    {
                        var guid = await _conexionEstacionRemota.ObtenerOrdenDespachoPorIdVentaLocal(factura.ventaId, token);
                        await _conexionEstacionRemota.CrearFacturaOrdenesDeDespacho(guid.ToString(), token);
                        
                    }
                    else
                    {
                        var guid = await _conexionEstacionRemota.ObtenerFacturaPorIdVentaLocal(factura.ventaId, token);
                        await _conexionEstacionRemota.CrearFacturaFacturas(guid.ToString(), token);
                        
                    }
                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error {ex.Message}");
                    Console.WriteLine($"Error {ex.StackTrace}");

                }
                await _databaseHandler.MandarImprimir(request.VentaId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}");
                Console.WriteLine($"Error {ex.StackTrace}");
            }

            return Unit.Value;

        }


        private FacturaSiges ConvertToFacturaSIGES(Factura factura)
        {
            return new FacturaSiges()
            {
                Autorizacion = factura.Autorizacion,
                Cantidad = (double)factura.Venta.CANTIDAD,
                Cara = factura.Venta.COD_CAR.ToString(),
                codigoFormaPago = factura.codigoFormaPago,
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