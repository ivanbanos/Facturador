using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class ReimprimirFacturaCanastillaCommandHandler : IRequestHandler<ReimprimirFacturaCanastillaCommand>
    {
        private readonly ILogger<ReimprimirFacturaCanastillaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;

        public ReimprimirFacturaCanastillaCommandHandler(ILogger<ReimprimirFacturaCanastillaCommandHandler> logger,
            IDataBaseHandler databaseHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
        }

        public async Task<Unit> Handle(ReimprimirFacturaCanastillaCommand request, CancellationToken cancellationToken)
        {
            await _databaseHandler.ReimprimirFacturaCanastilla(request.Consecutivo);
            return Unit.Value;
        }
    }
}
