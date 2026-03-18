

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
                var fechaInicioBusqueda = DateTime.Now.Date.AddDays(-2);
                var fechaFinBusqueda = DateTime.Now.Date;
                var turnos = await _databaseHandler.GetTurnosByFechas(fechaInicioBusqueda, fechaFinBusqueda);
                var turnoFallback = turnos
                    .Where(x => string.Equals((x.Isla ?? string.Empty).Trim(), request.Isla.ToString(), StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.FechaApertura)
                    .FirstOrDefault();

                if (turnoFallback == null || turnoFallback.numero <= 0)
                {
                    _logger.LogWarning("No se encontro turno valido para imprimir cierre canastilla en isla {Isla}", request.Isla);
                    return "No se encontro turno valido para imprimir";
                }

                await _databaseHandler.MandarImprimirObjeto(request.Isla, turnoFallback.FechaApertura, turnoFallback.numero, "CierreCanastilla");
            } else
            {
                await _databaseHandler.MandarImprimirObjeto(request.Isla, turnoA.FechaApertura, turnoA.numero, "CierreCanastilla");
            }
            return "Ok";
        }
    }
}
