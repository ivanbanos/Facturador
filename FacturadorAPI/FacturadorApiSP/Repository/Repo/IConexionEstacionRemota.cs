using FacturadorAPI.Models;

namespace FacturadorAPI.Repository.Repo
{
    public interface IConexionEstacionRemota
    {
        Task CrearFacturaFacturas(string guid, string token);
        Task CrearFacturaOrdenesDeDespacho(string guid, string token);
        Task EnviarFacturas(List<FacturaSiges> facturaSiges, IEnumerable<FormaPagoSiges> formas, string token);
        Task<string> GetToken(CancellationToken cancellationToken);
        Task<string> ObtenerFacturaPorIdVentaLocal(int ventaId, string token);
        Task<string> ObtenerOrdenDespachoPorIdVentaLocal(int ventaId, string token);
        Task<IEnumerable<Canastilla>> RecibirCanastilla(string token, CancellationToken cancellationToken);

        Task<string> GetInfoFacturaElectronica(int idVentaLocal, Guid estacionGuid, string token);
    }
}
