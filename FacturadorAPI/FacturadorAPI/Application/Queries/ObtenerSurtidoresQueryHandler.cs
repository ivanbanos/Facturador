using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerSurtidoresQueryHandler : IRequestHandler<ObtenerSurtidoresQuery, IEnumerable<SurtidorSiges>>
    {
        private readonly ILogger<ObtenerSurtidoresQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;

        public ObtenerSurtidoresQueryHandler(ILogger<ObtenerSurtidoresQueryHandler> logger, IDataBaseHandler databaseHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
        }

        public async Task<IEnumerable<SurtidorSiges>> Handle(ObtenerSurtidoresQuery request, CancellationToken cancellationToken)
        {
            return await _databaseHandler.ListarSurtidoresSigues(cancellationToken);
            
        }
    }
}
