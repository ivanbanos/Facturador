using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class MandarImprimirCommandHandler : IRequestHandler<MandarImprimirCommand>
    {
        private readonly ILogger<MandarImprimirCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public MandarImprimirCommandHandler(ILogger<MandarImprimirCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<Unit> Handle(MandarImprimirCommand request, CancellationToken cancellationToken)
        {

            await _databaseHandler.MandarImprimir(request.IdVenta);

            return Unit.Value;

        }

    }
}