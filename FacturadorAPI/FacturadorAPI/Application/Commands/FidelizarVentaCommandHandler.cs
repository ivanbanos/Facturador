using Dominio.Entidades;
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
            
            var factura = await _databaseHandler.GetFacturaPorIdVenta(request.IdVenta);
            

            var ok = await _fidelizacion.SubirPuntops((float)puntos.ValorVenta, request.Identificacion, puntos.Factura);
            if (ok)
            {
                await _databaseHandler.ActualizarFacturaFidelizada(request.Identificacion, factura.ventaId);
                
                var fidelizados = await _fidelizacion.GetFidelizados(request.Identificacion);
                foreach (var fidelizado in fidelizados)
                {
                    await _databaseHandler.AddFidelizado(fidelizado.Documento, fidelizado.Puntos ?? 0);
                }
                await _databaseHandler.MandarImprimir(request.IdVenta);
                return Unit.Value;
            }
            else
            {
                throw new Exception("Venta fidelizada");
            }

        }
    }

}
