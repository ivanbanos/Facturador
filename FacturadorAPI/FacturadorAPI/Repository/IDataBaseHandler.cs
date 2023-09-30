using FacturadorAPI.Models;

namespace MachineUtilizationApi.Repository
{
    public interface IDataBaseHandler
    {
        Task ActualizarCanastilla(IEnumerable<Canastilla> canastillas);
        Task ActualizarFactura(int facturaPOSId, int terceroId, int codigoFormaPago, int idVenta, string placa, string kilometraje);
        Task ActuralizarFacturasEnviados(List<int> list);
        Task ConvertirAFactura(int idFactura);
        Task ConvertirAOrder(int idFactura);
        Task<Tercero> CrearTercero(Tercero tercero);
        Task GenerarFacturaCanastilla(FacturaCanastilla facturaCanastilla, bool imprimir);
        Task<IEnumerable<Canastilla>> GetCanastillas();
        Task<FacturaSiges> GetFacturaPorIdVenta(int idFactura);
        Task<IEnumerable<FacturaSiges>> GetFacturasPorFechas(DateTime fechaInicio, DateTime fechaFin);
        Task<IEnumerable<TurnoSiges>> GetTurnosByFechas(DateTime fechaInicio, DateTime fechaFin);
        Task<IEnumerable<TurnoSurtidor>> GetTurnoSurtidorInfo(int id);
        Task<IEnumerable<CaraSiges>> ListarCarasSigues(CancellationToken cancellationToken);
        Task<IEnumerable<FormaPagoSiges>> ListarFormasPagoSiges(CancellationToken cancellationToken);
        Task<IEnumerable<SurtidorSiges>> ListarSurtidoresSigues(CancellationToken cancellationToken);
        Task<IEnumerable<TipoIdentificacion>> ListarTiposIdentificacion(CancellationToken cancellationToken);
        Task MandarImprimir(int idVenta);
        Task<IEnumerable<Tercero>> ObtenerTerceroPorIDentificacion(string identificacion, CancellationToken cancellationToken);
        Task<TurnoSiges> ObtenerTurnoPorIsla(int idIsla, CancellationToken cancellationToken);
        Task<FacturaSiges> ObtenerUltimaFacturaPorCara(int idCara, CancellationToken cancellationToken);
    }
}
