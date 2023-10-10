using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class AgregarFacturaCanastillaCommandHandler : IRequestHandler<AgregarFacturaCanastillaCommand>
    {
        private readonly ILogger<AgregarFacturaCanastillaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public AgregarFacturaCanastillaCommandHandler(ILogger<AgregarFacturaCanastillaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<Unit> Handle(AgregarFacturaCanastillaCommand request, CancellationToken cancellationToken)
        {

            try
            {

                await _databaseHandler.GenerarFacturaCanastilla(request.FacturaCanastilla, true);

            }
            catch (Exception ex)
            {
            }

            return Unit.Value;

        }

    }
}