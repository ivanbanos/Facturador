
using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ListarTiposIdentificacionQueryHandler : IRequestHandler<ListarTiposIdentificacionQuery, IEnumerable<TipoIdentificacion>>
    {
        private readonly ILogger<ListarTiposIdentificacionQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;

        public ListarTiposIdentificacionQueryHandler(ILogger<ListarTiposIdentificacionQueryHandler> logger, IDataBaseHandler databaseHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
        }

        public async Task<IEnumerable<TipoIdentificacion>> Handle(ListarTiposIdentificacionQuery request, CancellationToken cancellationToken)
        {

            return await _databaseHandler.ListarTiposIdentificacion(cancellationToken);

        }
    }
}
