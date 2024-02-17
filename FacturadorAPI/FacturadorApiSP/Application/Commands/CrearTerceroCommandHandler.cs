using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class CrearTerceroCommandHandler : IRequestHandler<CrearTerceroCommand, Tercero>
    {
        private readonly ILogger<CrearTerceroCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public CrearTerceroCommandHandler(ILogger<CrearTerceroCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<Tercero> Handle(CrearTerceroCommand request, CancellationToken cancellationToken)
        {
           

            return await _databaseHandler.CrearTercero(request.Tercero);

        }
    }
}