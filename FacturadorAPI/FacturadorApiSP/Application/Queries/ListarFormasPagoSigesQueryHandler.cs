using FacturadorAPI.Models;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Queries
{
    public class ListarFormasPagoSigesQueryHandler : IRequestHandler<ListarFormasPagoSigesQuery, IEnumerable<FormaPagoSiges>>
    {
        private readonly ILogger<ListarFormasPagoSigesQueryHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;

        public ListarFormasPagoSigesQueryHandler(ILogger<ListarFormasPagoSigesQueryHandler> logger, IDataBaseHandler databaseHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
        }

        public async Task<IEnumerable<FormaPagoSiges>> Handle(ListarFormasPagoSigesQuery request, CancellationToken cancellationToken)
        {

            return await _databaseHandler.ListarFormasPagoSP(cancellationToken);

        }
    }
}
