using FactoradorEstacionesModelo.Objetos;
using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerUltimaFacturaPorCaraQueryHandler : IRequestHandler<ObtenerUltimaFacturaPorCaraQuery, FacturaSiges>
    {
        private readonly ILogger<ObtenerUltimaFacturaPorCaraQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;

        public ObtenerUltimaFacturaPorCaraQueryHandler(ILogger<ObtenerUltimaFacturaPorCaraQueryHandler> logger, IDataBaseHandler databaseHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
        }

        public async Task<FacturaSiges> Handle(ObtenerUltimaFacturaPorCaraQuery request, CancellationToken cancellationToken)
        {

            return ConvertToFacturaSIGES(await _databaseHandler.getUltimasFacturas((short)request.IdCara));

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
