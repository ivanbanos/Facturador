using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class ConvertirAFacturaCommandHandler : IRequestHandler<ConvertirAFacturaCommand>
    {
        private readonly ILogger<ConvertirAFacturaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public ConvertirAFacturaCommandHandler(ILogger<ConvertirAFacturaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<Unit> Handle(ConvertirAFacturaCommand request, CancellationToken cancellationToken)
        {

            await _databaseHandler.ConvertirAFactura(request.IdFactura);

            return Unit.Value;

        }

    }
}