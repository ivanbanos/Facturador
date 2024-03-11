using FacturadorAPI.Models;
using FacturadorAPI.Repository.Repo;
using MachineUtilizationApi.Repository;
using MediatR;
using Newtonsoft.Json.Linq;

namespace FacturadorAPI.Application.Commands
{
    public class EnviarFacturaElectronicaCommandHandler : IRequestHandler<EnviarFacturaElectronicaCommand>
    {
        private readonly ILogger<EnviarFacturaElectronicaCommandHandler> _logger;
        private readonly IDataBaseHandler _databaseHandler;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;

        public EnviarFacturaElectronicaCommandHandler(ILogger<EnviarFacturaElectronicaCommandHandler> logger,
            IDataBaseHandler databaseHandler,
            IConexionEstacionRemota conexionEstacionRemota)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseHandler = databaseHandler ?? throw new ArgumentNullException(nameof(databaseHandler));
            _conexionEstacionRemota = conexionEstacionRemota ?? throw new ArgumentNullException(nameof(conexionEstacionRemota));
        }

        public async Task<Unit> Handle(EnviarFacturaElectronicaCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var factura = await _databaseHandler.GetFacturaPorIdVenta(request.IdFactura);
                await _databaseHandler.ActualizarFactura(factura.facturaPOSId, request.TerceroId, request.FormaPago, request.VentaId, request.Placa == "NP" ? "" : request.Placa, request.Kilometraje == "NP" ? "" : request.Kilometraje);
                factura = await _databaseHandler.GetFacturaPorIdVenta(request.IdFactura);


                try
                {
                    var token = await _conexionEstacionRemota.GetToken(cancellationToken);
                    var formas = await _databaseHandler.ListarFormasPagoSiges(cancellationToken);
                    await _conexionEstacionRemota.EnviarFacturas(new List<FacturaSiges>() { factura }, formas, token);

                    if (factura.Consecutivo == 0)
                    {
                        var guid = await _conexionEstacionRemota.ObtenerOrdenDespachoPorIdVentaLocal(factura.ventaId, token);
                        await _conexionEstacionRemota.CrearFacturaOrdenesDeDespacho(guid.ToString(), token);
                        await _databaseHandler.ActuralizarFacturasEnviados(new List<int>() { factura.ventaId });
                        
                    }
                    else
                    {
                        var guid = await _conexionEstacionRemota.ObtenerFacturaPorIdVentaLocal(factura.ventaId, token);
                        await _conexionEstacionRemota.CrearFacturaFacturas(guid.ToString(), token);
                        await _databaseHandler.ActuralizarFacturasEnviados(new List<int>() { factura.ventaId });
                        
                    }
                }
                catch (Exception)
                {
                }
            }
            catch (Exception e)
            {
            }

            return Unit.Value;

        }

    }
}