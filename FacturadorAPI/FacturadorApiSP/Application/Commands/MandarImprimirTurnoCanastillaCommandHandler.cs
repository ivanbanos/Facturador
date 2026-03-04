

using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;
using Microsoft.Extensions.Options;

namespace FacturadorApiSP.Application.Commands
{
    public class MandarImprimirTurnoCanastillaCommandHandler : IRequestHandler<MandarImprimirTurnoCanastillaCommand, string>
    {
        private readonly ILogger<MandarImprimirTurnoCanastillaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly InfoEstacion _infoEstacion;

        public MandarImprimirTurnoCanastillaCommandHandler(ILogger<MandarImprimirTurnoCanastillaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota,
            IOptions<InfoEstacion> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
            _infoEstacion = options.Value;
        }

        public async Task<string> Handle(MandarImprimirTurnoCanastillaCommand request, CancellationToken cancellationToken)
        {
            var turnoA = await _databaseHandler.ObtenerTurnoPorIsla(request.Isla, cancellationToken);
            if(turnoA == null)
            {
                await _databaseHandler.MandarImprimirObjeto(request.Isla, DateTime.Now.Date, 0, "CierreCanastilla");
            } else
            {
                await _databaseHandler.MandarImprimirObjeto(request.Isla, turnoA.FechaApertura, turnoA.numero, "CierreCanastilla");
            }
            return "Ok";
        }
    }
}
