using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class MandarImprimirConsecutivoCommandHandler : IRequestHandler<MandarImprimirConsecutivoCommand>
    {
        private readonly ILogger<MandarImprimirConsecutivoCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public MandarImprimirConsecutivoCommandHandler(ILogger<MandarImprimirConsecutivoCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<Unit> Handle(MandarImprimirConsecutivoCommand request, CancellationToken cancellationToken)
        {
            await _databaseHandler.MandarImprimirConsecutivo(request.Consecutivo);

            return Unit.Value;

        }

    }
}