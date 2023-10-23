using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class ReimprimirTurnoCommandHandler : IRequestHandler<ReimprimirTurnoCommand>
    {
        private readonly ILogger<ReimprimirTurnoCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public ReimprimirTurnoCommandHandler(ILogger<ReimprimirTurnoCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<Unit> Handle(ReimprimirTurnoCommand request, CancellationToken cancellationToken)
        {
            await _databaseHandler.ReimprimirTurno(request.Fecha, request.IdIsla, request.Posicion);
            return Unit.Value;

        }

    }
}