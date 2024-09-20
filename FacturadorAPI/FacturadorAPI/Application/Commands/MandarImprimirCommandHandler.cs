using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;

namespace FacturadorAPI.Application.Commands
{
    public class MandarImprimirCommandHandler : IRequestHandler<MandarImprimirCommand, string>
    {
        private readonly ILogger<MandarImprimirCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public MandarImprimirCommandHandler(ILogger<MandarImprimirCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<string> Handle(MandarImprimirCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _conexionEstacionRemota.GetToken(cancellationToken);
                var factura = await _databaseHandler.GetFacturaPorIdVenta(request.FacturaPOSId);

                if (factura.codigoFormaPago == 2)
                {
                    await _databaseHandler.ActualizarFactura(factura.facturaPOSId, factura.Tercero.terceroId, factura.codigoFormaPago, factura.ventaId, factura.Placa, request.Kilometraje == "NP" ? "" : request.Kilometraje);

                    return "Ok";
                }
                else
                {
                    await _databaseHandler.ActualizarFactura(factura.facturaPOSId, request.TerceroId, request.FormaPago, request.VentaId, request.Placa == "NP" ? "" : request.Placa, request.Kilometraje == "NP" ? "" : request.Kilometraje);

                    factura = await _databaseHandler.GetFacturaPorIdVenta(request.VentaId);

                }

                try
                {

                    var formas = await _databaseHandler.ListarFormasPagoSiges(cancellationToken);
                    await _databaseHandler.ActuralizarFacturasEnviados(new List<int>() { factura.ventaId });
                    await _conexionEstacionRemota.EnviarFacturas(new List<FacturaSiges>() { factura }, formas, token);

                }
                catch (Exception ex)
                {

                    Console.WriteLine($"Error {ex.Message}");
                    Console.WriteLine($"Error {ex.StackTrace}");

                }

                await _databaseHandler.MandarImprimir(request.VentaId);
                return "Ok";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}");
                Console.WriteLine($"Error {ex.StackTrace}");
            }
            await _databaseHandler.MandarImprimir(request.VentaId);

            return "NoChange";

        }

    }
}