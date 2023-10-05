﻿using Dominio.Entidades;
using FactoradorEstacionesModelo.Fidelizacion;
using FacturadorAPI.Repository.Repo;
using FacturadorEstacionesRepositorio;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class FidelizarVentaCommandHandler : IRequestHandler<FidelizarVentaCommand>
    {
        private readonly ILogger<FidelizarVentaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IFidelizacion _fidelizacion;

        public FidelizarVentaCommandHandler(ILogger<FidelizarVentaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IFidelizacion fidelizacion)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _fidelizacion = fidelizacion ?? throw new ArgumentNullException(nameof(fidelizacion));
        }

        public async Task<Unit> Handle(FidelizarVentaCommand request, CancellationToken cancellationToken)
        {
            var puntos = await _databaseHandler.GetVentaFidelizarAutomaticaPorVenta(request.IdVenta);
            var tercero = await _databaseHandler.GetTerceroByQuery(request.Identificacion);
            await _fidelizacion.SubirPuntops((float)puntos.ValorVenta, tercero.identificacion, puntos.Factura);
            var fidelizados = await _fidelizacion.GetFidelizados();
            foreach (var fidelizado in fidelizados)
            {
               await _databaseHandler.AddFidelizado(fidelizado.Documento, fidelizado.Puntos ?? 0);
            }
            return Unit.Value;

        }
    }

}
