using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;

namespace FacturadorApiSP.Application.Commands
{
    public class ReimprimirTurnoCanastillaCommandHandler : IRequestHandler<ReimprimirTurnoCanastillaCommand, string>
    {
        private readonly ILogger<ReimprimirTurnoCanastillaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly InfoEstacion _infoEstacion;

        public ReimprimirTurnoCanastillaCommandHandler(ILogger<ReimprimirTurnoCanastillaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IOptions<InfoEstacion> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _infoEstacion = options.Value;
        }

        public async Task<string> Handle(ReimprimirTurnoCanastillaCommand request, CancellationToken cancellationToken)
        {
            await _databaseHandler.MandarImprimirObjeto(request.IdIsla, request.Fecha, request.Posicion, "ReimprimirCierreCanastilla");
            return "Ok";
        }
    }
}
