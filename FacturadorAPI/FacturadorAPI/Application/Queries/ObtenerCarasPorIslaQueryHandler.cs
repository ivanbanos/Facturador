using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ObtenerCarasPorIslaQueryHandler : IRequestHandler<ObtenerCarasPorIslaQuery, IEnumerable<CaraSiges>>
    {
        private readonly ILogger<ObtenerCarasPorIslaQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;

        public ObtenerCarasPorIslaQueryHandler(ILogger<ObtenerCarasPorIslaQueryHandler> logger, IDataBaseHandler databaseHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
        }

        public async Task<IEnumerable<CaraSiges>> Handle(ObtenerCarasPorIslaQuery request, CancellationToken cancellationToken)
        {
            var caras = await _databaseHandler.ListarCarasSigues(cancellationToken);
            return caras.Where(x => x.IdIsla == request.IdIsla);


        }
    }
}
