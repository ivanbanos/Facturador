using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class AbrirTurnoCommandHandler : IRequestHandler<AbrirTurnoCommand>
    {
        private readonly ILogger<AbrirTurnoCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public AbrirTurnoCommandHandler(ILogger<AbrirTurnoCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<Unit> Handle(AbrirTurnoCommand request, CancellationToken cancellationToken)
        {

            await _databaseHandler.AbrirTurno(request.Isla, request.Codigo);

            return Unit.Value;

        }

    }
}