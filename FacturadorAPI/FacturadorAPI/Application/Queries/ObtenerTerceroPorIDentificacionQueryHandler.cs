using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerTerceroPorIDentificacionQueryHandler : IRequestHandler<ObtenerTerceroPorIDentificacionQuery, IEnumerable<Tercero>>
    {
        private readonly ILogger<ObtenerTerceroPorIDentificacionQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;

        public ObtenerTerceroPorIDentificacionQueryHandler(ILogger<ObtenerTerceroPorIDentificacionQueryHandler> logger, IDataBaseHandler databaseHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
        }

        public async Task<IEnumerable<Tercero>> Handle(ObtenerTerceroPorIDentificacionQuery request, CancellationToken cancellationToken)
        {

            return await _databaseHandler.ObtenerTerceroPorIDentificacion(request.Identificacion, cancellationToken);

        }
    }
}
