using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class CerrarTurnoCommandHandler : IRequestHandler<CerrarTurnoCommand>
    {
        private readonly ILogger<CerrarTurnoCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public CerrarTurnoCommandHandler(ILogger<CerrarTurnoCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<Unit> Handle(CerrarTurnoCommand request, CancellationToken cancellationToken)
        {

            await _databaseHandler.CerrarTurno(request.Isla, request.Codigo);

            return Unit.Value;

        }

    }
}