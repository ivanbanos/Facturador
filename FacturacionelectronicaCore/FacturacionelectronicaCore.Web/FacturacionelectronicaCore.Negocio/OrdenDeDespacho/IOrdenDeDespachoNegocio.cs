using EstacionesServicio.Modelo;
using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.OrdenDeDespacho
{
    public interface IOrdenDeDespachoNegocio
    {
        Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesDeDespacho(FiltroBusqueda filtroOrdenDeDespacho);
        Task<int> AddOrdenesImprimir(IEnumerable<FacturasEntity> ordenDeDespachos);
        Task AnularOrdenes(IEnumerable<FacturasEntity> ordenes);
        Task<string> CrearFacturaOrdenesDeDespacho(IEnumerable<OrdenesDeDespachoGuids> ordenesDeDespacho);
        Task<Modelo.OrdenDeDespacho> ObtenerOrdenDespachoPorIdVentaLocal(int idVentaLocal, Guid estacion);
        Task<IEnumerable<Modelo.OrdenDeDespacho>> ObtenerOrdenesPorTurno(Guid turno);
        Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesSinFacturaElectronicaCreditoDirecto(FiltroBusqueda filtroOrdenDeDespacho);
        Task<List<string>> ReenviarOrdenesDespachoPorIdVentaLocal(List<int> idVentaLocalList, Guid estacion);
        Task<ReporteFiscal> GetReporteFiscal(FiltroBusqueda filtroFactura);
    }
}