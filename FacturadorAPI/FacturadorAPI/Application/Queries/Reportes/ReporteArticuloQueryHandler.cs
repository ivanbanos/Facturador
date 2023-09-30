using FacturadorAPI.Application.Queries.Reportes.Objetos;
using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.Server;

namespace FacturadorAPI.Application.Queries.Reportes
{
    public class ReporteArticuloQueryHandler : IRequestHandler<ReporteArticuloQuery, ReporteArticuloResponse>
    {
        private readonly ILogger<ReporteArticuloQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly InfoEstacion _infoEstacion;

        public ReporteArticuloQueryHandler(ILogger<ReporteArticuloQueryHandler> logger, IDataBaseHandler databaseHandler, IOptions<InfoEstacion> infoEstacion)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _infoEstacion = infoEstacion.Value;
        }

        public async Task<ReporteArticuloResponse> Handle(ReporteArticuloQuery request, CancellationToken cancellationToken)
        {
            var reporteArticuloResponse = new ReporteArticuloResponse(_infoEstacion.Nombre, _infoEstacion.NIT);
            var facturas = await _databaseHandler.GetFacturasPorFechas(request.Request.FechaInicio, request.Request.FechaFin);
            var formasPago = await _databaseHandler.ListarFormasPagoSiges(cancellationToken);
            var groupForma = facturas.GroupBy(x => x.codigoFormaPago);
            foreach (var forma in groupForma)
            {
                if (formasPago.Any(x => x.Id == forma.Key))
                {
                    reporteArticuloResponse.PorFormas.Add(new PorFormas()
                    {
                        Cantidad = forma.Sum(x=>x.Cantidad),
                        Descripcion = formasPago.First(x => x.Id == forma.Key).Descripcion,
                        Facturas = forma.Count(),
                        Total = forma.Sum(x => x.Total)
                    });
                }
            }

            var groupArticulo = facturas.GroupBy(x => x.Combustible);
            foreach (var articulo in groupArticulo)
            {
                    reporteArticuloResponse.PorArticulo.Add(new PorArticulo()
                    {
                        Cantidad = articulo.Sum(x => x.Cantidad),
                        Descripcion = articulo.Key,
                        Facturas = articulo.Count(),
                        Neto = articulo.Sum(x => x.Subtotal),
                        Recaudo = 0,
                        Descuento = articulo.Sum(x => x.Descuento),
                        Subtotal = articulo.Sum(x => x.Subtotal),
                        Total = articulo.Sum(x => x.Total)
                    });
                
            }
            return reporteArticuloResponse;
        }
    }
}
