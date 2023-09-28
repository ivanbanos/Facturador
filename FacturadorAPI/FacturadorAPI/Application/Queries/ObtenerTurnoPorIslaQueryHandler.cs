using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerTurnoPorIslaQueryHandler : IRequestHandler<ObtenerTurnoPorIslaQuery, TurnoSiges>
    {
        private readonly ILogger<ObtenerTurnoPorIslaQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;

        public ObtenerTurnoPorIslaQueryHandler(ILogger<ObtenerTurnoPorIslaQueryHandler> logger, IDataBaseHandler databaseHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
        }

        public async Task<TurnoSiges> Handle(ObtenerTurnoPorIslaQuery request, CancellationToken cancellationToken)
        {

            return await _databaseHandler.ObtenerTurnoPorIsla(request.IdIsla, cancellationToken);

        }
    }
}
