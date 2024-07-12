using FactoradorEstacionesModelo.Objetos;
using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;

namespace FacturadorAPI.Application.Commands
{
    public class EnviarFacturaElectronicaCommandHandler : IRequestHandler<EnviarFacturaElectronicaCommand, string>
    {
        private readonly ILogger<EnviarFacturaElectronicaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly InfoEstacion _infoEstacion;

        public EnviarFacturaElectronicaCommandHandler(ILogger<EnviarFacturaElectronicaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota,
            IOptions<InfoEstacion> infoEstacion)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
            _infoEstacion = infoEstacion.Value;
        }

        public async Task<string> Handle(EnviarFacturaElectronicaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _conexionEstacionRemota.GetToken(cancellationToken); 
                var factura = await _databaseHandler.GetFacturaPorIdVenta(request.VentaId);
                if (factura.codigoFormaPago == 6)
                {
                    await _databaseHandler.ActualizarFactura(factura.facturaPOSId, factura.Tercero.terceroId, factura.codigoFormaPago, factura.ventaId, factura.Placa, request.Kilometraje == "NP" ? "" : request.Kilometraje, factura.numeroTransaccion == null ? "" : factura.numeroTransaccion);
                    await _databaseHandler.MandarImprimir(request.VentaId,1);
                    return "Ok";
                }
                var infoTemp = await _conexionEstacionRemota.GetInfoFacturaElectronica(request.VentaId, Guid.Parse(_infoEstacion.EstacionFuente), token);
                if (string.IsNullOrEmpty(infoTemp))
                {
                    await _databaseHandler.ActualizarFactura(factura.facturaPOSId, request.TerceroId, request.FormaPago, request.VentaId, request.Placa == "NP" ? "" : request.Placa, request.Kilometraje == "NP" ? "" : request.Kilometraje, request.NumeroTransaccion == "NP" ? "" : request.NumeroTransaccion);

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

                await _databaseHandler.MandarImprimir(request.VentaId, 1);
                if (infoTemp != null)
                {
                    return "NoChange";
                }
                else
                {
                    return "Ok";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}");
                Console.WriteLine($"Error {ex.StackTrace}");
            }

            return "NoChange";

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