using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.ManejadorInformacionLocal
{
    public interface IManejadorInformacionLocalNegocio
    {
        Task EnviarTerceros(IEnumerable<Modelo.Tercero> terceros);
        Task EnviarOrdenesDespacho(IEnumerable<Modelo.OrdenDeDespacho> ordenDeDespachos, Guid estacion);
        Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesDeDespacho(Guid estacion);
        Task<IEnumerable<Modelo.Tercero>> GetTercerosActualizados(Guid estacion);
        Task<IEnumerable<Modelo.Tercero>> GetTercerosActualizados();
        Task<IEnumerable<string>> GetTipos();
        Task<string> GetInfoFacturaElectronica(int idVentaLocal, Guid estacion);
        Task<int> AddFacturaCanastilla(IEnumerable<FacturaCanastilla> facturas, Guid estacion);
        Task<ResolucionElectronica> GetResolucionElectronica();
        Task<string> JsonFacturaElectronica(int idVentaLocal, Guid estacion);
        Task<string> JsonFacturaElectronicaCanastilla(int idVentaLocal, Guid estacion);
        Task<string> GetInfoFacturaElectronicaCanastilla(int consecutivo, Guid estacion);
        Task AgregarFechaReporteFactura(IEnumerable<FacturaFechaReporte> facturas, Guid estacion);
    }
}
