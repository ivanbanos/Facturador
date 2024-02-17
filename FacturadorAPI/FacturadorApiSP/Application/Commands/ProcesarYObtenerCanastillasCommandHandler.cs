using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class ProcesarYObtenerCanastillasCommandHandler : IRequestHandler<ProcesarYObtenerCanastillasCommand, IEnumerable<Canastilla>>
    {
        private readonly ILogger<ProcesarYObtenerCanastillasCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public ProcesarYObtenerCanastillasCommandHandler(ILogger<ProcesarYObtenerCanastillasCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<IEnumerable<Canastilla>> Handle(ProcesarYObtenerCanastillasCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _conexionEstacionRemota.GetToken(cancellationToken);
                var canastillas = await _conexionEstacionRemota.RecibirCanastilla(token, cancellationToken);

                _databaseHandler.ActualizarCanastilla(canastillas);
            }
            catch (Exception ex)
            {
            }

            return await _databaseHandler.GetCanastillas();

        }
    }
}
