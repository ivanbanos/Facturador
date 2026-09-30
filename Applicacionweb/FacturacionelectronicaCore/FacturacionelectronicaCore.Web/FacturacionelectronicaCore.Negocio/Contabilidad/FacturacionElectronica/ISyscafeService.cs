using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public interface ISyscafeService
    {
        Task<bool> EnviarFacturaCanastilla(Modelo.FacturaCanastilla factura);
        Task<bool> EnviarFacturaCombustible(Modelo.OrdenDeDespacho orden);
    }
}
