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

                if (request.FacturaCanastilla.terceroId.terceroId == -1)
                {

                    //request.FacturaCanastilla.terceroId = _databaseHandler.crearTercero(0, new TipoIdentificacion() { TipoIdentificacionId = 1 }, _tercero.identificacion, "No identificado", "No identificado", "No identificado", "No identificado", "");

                }
                await _databaseHandler.GenerarFacturaCanastilla(request.FacturaCanastilla, true);

            }
            catch (Exception ex)
            {
            }

            return Unit.Value;

        }

    }
}