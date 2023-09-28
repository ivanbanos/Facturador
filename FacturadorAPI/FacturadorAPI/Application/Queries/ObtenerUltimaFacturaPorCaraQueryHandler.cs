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

            return await _databaseHandler.ObtenerUltimaFacturaPorCara(request.IdCara, cancellationToken);

        }
    }
}
