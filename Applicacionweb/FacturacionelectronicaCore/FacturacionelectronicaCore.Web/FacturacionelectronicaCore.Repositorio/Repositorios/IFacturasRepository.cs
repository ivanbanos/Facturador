using EstacionesServicio.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface IFacturasRepository
    {
        /// <summary>
        /// Crea una factura a partir de las ordenes de despacho enviadas
        /// </summary>
        /// <param name="ordenesDeDespacho">Lista con los Guid de las ordenes de despacho</param>
        /// <returns></returns>
        Task<IEnumerable<Factura>> CrearFacturaOrdenesDeDespacho(IEnumerable<OrdenesDeDespachoGuids> ordenesDeDespacho);
        Task AddRange(IEnumerable<Factura> facturas, Guid estacion);

        /// <summary>
        /// Obtiene una lista de Facturas por Fecha inicial o Fecha final o Identiciación o Nombre de tercero
        /// </summary>
        /// <param name="fechaInicial">usuario</param>
        /// <param name="fechaFinal">fechaFinal</param>
        /// <param name="identificacionTercero">usuario</param>
        /// <param name="nombreTercero">fechaFinal</param>
        /// <returns>Coleccion de Facturas con Identificacion y Nombre de tercero</returns>
        Task<IEnumerable<Factura>> GetFacturas(DateTime? fechaInicial, DateTime? fechaFinal, string identificacionTercero, string nombreTercero, Guid estacion);
        Task<IEnumerable<Factura>> GetFacturaByEstado(string estado, Guid estacion);
        Task setConsecutivoFacturaPendiente(string facturaGuid, int consecutivo);

        /// <summary>
        /// Anula las facturas de la lista suministrada
        /// </summary>
        /// <param name="facturas">Lista de facturas a anular</param>
        /// <returns></returns>
        Task<int> AnularFacturas(IEnumerable<FacturasEntity> facturas);
        Task AddFacturasImprimir(IEnumerable<FacturasEntity> facturas);
        Task<IEnumerable<Factura>> GetFacturasImprimir(Guid estacion);
        Task AgregarFechaReporteFactura(IEnumerable<FacturaFechaReporte> facturas, Guid estacion);
        Task<IEnumerable<Factura>> ObtenerFacturaPorGuid(string facturaGuid);
        Task<IEnumerable<Factura>> ObtenerFacturaPorIdVentaLocal(int idVentaLocal, Guid estacion);
        Task SetIdFacturaElectronicaFactura(string idFacturaElectronica, string guid);
        Task AgregarTurnoAFactura(int idVentaLocal, string turnoGuid, Guid estacion);
        Task<IEnumerable<Factura>> ObtenerFacturasPorTurnoId(Guid turno);
    }
}