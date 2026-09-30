using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.FacturaConsolidada
{
    public interface IFacturaConsolidadaNegocio
    {
        /// <summary>
        /// Obtiene todas las facturas consolidadas con filtros opcionales
        /// </summary>
        Task<IEnumerable<Modelo.FacturaConsolidada>> GetFacturasConsolidadas(
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            string identificacionCliente = null,
            Guid? idEstacion = null,
            string estado = null);

        /// <summary>
        /// Obtiene una factura consolidada por GUID
        /// </summary>
        Task<Modelo.FacturaConsolidada> GetFacturaConsolidadaPorGuid(string guid);

        /// <summary>
        /// Obtiene órdenes pendientes de consolidación para un cliente específico
        /// </summary>
        Task<IEnumerable<Modelo.OrdenDeDespacho>> GetOrdenesPendientesConsolidacion(FiltroOrdenesConsolidacion filtro);

        /// <summary>
        /// Crea una nueva factura consolidada con las órdenes especificadas
        /// </summary>
        Task<Modelo.FacturaConsolidada> CrearFacturaConsolidada(CrearFacturaConsolidadaRequest request);

        /// <summary>
        /// Envía una factura consolidada a facturación electrónica
        /// </summary>
        Task<string> EnviarFacturaConsolidadaAFacturacion(string guidFactura);

        /// <summary>
        /// Obtiene el detalle completo de una factura consolidada (incluye órdenes)
        /// </summary>
        Task<Modelo.FacturaConsolidada> GetDetalleFacturaConsolidada(string guid);

        /// <summary>
        /// Valida si las órdenes pueden ser consolidadas
        /// </summary>
        Task<string> ValidarOrdenesParaConsolidacion(List<string> guidsOrdenes, string identificacionCliente);
    }
}