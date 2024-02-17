using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class ConvertirAOrdenCommandHandler : IRequestHandler<ConvertirAOrdenCommand>
    {
        private readonly ILogger<ConvertirAOrdenCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public ConvertirAOrdenCommandHandler(ILogger<ConvertirAOrdenCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<Unit> Handle(ConvertirAOrdenCommand request, CancellationToken cancellationToken)
        {

            await _databaseHandler.ConvertirAOrder(request.IdFactura);

            return Unit.Value;

        }

    }
}