using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface IOrdenDeDespachoRepositorio
    {
        /// <summary>
        /// Obtiene una lista de ordenes de despacho por Fecha inicial o Fecha final o Identiciación o Nombre de tercero
        /// </summary>
        /// <param name="fechaInicial">usuario</param>
        /// <param name="fechaFinal">fechaFinal</param>
        /// <param name="identificacionTercero">usuario</param>
        /// <param name="nombreTercero">fechaFinal</param>
        /// <param name="estacion">Estacion</param>
        /// <returns>Coleccion de Ordenes de desapacho con Identificacion y Nombre de tercero</returns>
        Task<IEnumerable<OrdenDeDespacho>> GetOrdenesDeDespacho(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid? estacion);
        Task AddRange(IEnumerable<OrdenDeDespacho> lists, Guid estacion);

        Task<int> AddOrdenesImprimir(IEnumerable<FacturasEntity> lists);
        Task<IEnumerable<OrdenDeDespacho>> GetOrdenesImprimir(Guid estacion);
        Task<int> AnularOrdenes(IEnumerable<FacturasEntity> facturasList);
        Task SetIdFacturaElectronicaOrdenesdeDespacho(string idFacturaElectronica, string guid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="facturaGuid"></param>
        /// <returns></returns>
        Task<IEnumerable<OrdenDeDespacho>> GetOrdenesDeDespachoByFactura(string facturaGuid);
        Task<IEnumerable<OrdenDeDespacho>> ObtenerOrdenDespachoPorGuid(string guid);
        Task<IEnumerable<OrdenDeDespacho>> ObtenerOrdenDespachoPorIdVentaLocal(int idVentaLocal, Guid estacion);
        Task<IEnumerable<OrdenDeDespacho>> ObtenerOrdenesPorTurno(Guid turno);
        Task AgregarFechaReporteFactura(IEnumerable<FacturaFechaReporte> facturas, Guid estacion);
    }
}