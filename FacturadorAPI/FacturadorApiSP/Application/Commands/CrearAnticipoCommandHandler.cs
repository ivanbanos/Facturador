using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FacturadorApiSP.Application.Commands
{
    public class CrearAnticipoCommandHandler : IRequestHandler<CrearAnticipoCommand>
    {
        private readonly ILogger<CrearAnticipoCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;

        public CrearAnticipoCommandHandler(ILogger<CrearAnticipoCommandHandler> logger, IDataBaseHandler databaseHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
        }

        public async Task<Unit> Handle(CrearAnticipoCommand request, CancellationToken cancellationToken)
        {
            await _databaseHandler.CrearAnticipo(
                request.IdIsla,
                request.NumTurno,
                request.FechaTurno,
                request.Nombre,
                request.Placa,
                request.Monto,
                request.TurnoGuid);

            return Unit.Value;
        }
    }
}
