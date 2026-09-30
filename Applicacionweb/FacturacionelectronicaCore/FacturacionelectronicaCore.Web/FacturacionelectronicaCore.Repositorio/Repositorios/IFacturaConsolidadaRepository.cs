using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface IFacturaConsolidadaRepository
    {
        /// <summary>
        /// Obtiene todas las facturas consolidadas con filtros opcionales
        /// </summary>
        Task<IEnumerable<FacturaConsolidada>> GetFacturasConsolidadas(
            DateTime? fechaDesde = null, 
            DateTime? fechaHasta = null, 
            string identificacionCliente = null, 
            Guid? idEstacion = null,
            string estado = null);
        
        /// <summary>
        /// Obtiene una factura consolidada por ID
        /// </summary>
        Task<FacturaConsolidada> GetFacturaConsolidada(string id);
        
        /// <summary>
        /// Obtiene una factura consolidada por GUID
        /// </summary>
        Task<FacturaConsolidada> GetFacturaConsolidadaPorGuid(string guid);
        
        /// <summary>
        /// Crea una nueva factura consolidada
        /// </summary>
        Task<FacturaConsolidada> CrearFacturaConsolidada(FacturaConsolidada factura);
        
        /// <summary>
        /// Actualiza una factura consolidada existente
        /// </summary>
        Task<FacturaConsolidada> ActualizarFacturaConsolidada(FacturaConsolidada factura);
        
        /// <summary>
        /// Obtiene el siguiente número consecutivo para facturas consolidadas
        /// </summary>
        Task<int> ObtenerSiguienteConsecutivo(Guid idEstacion);
        
        /// <summary>
        /// Verifica si una orden ya está incluida en alguna factura consolidada
        /// </summary>
        Task<bool> OrdenYaConsolidada(string guidOrden);
        
        /// <summary>
        /// Obtiene facturas consolidadas por rango de fechas y estación
        /// </summary>
        Task<IEnumerable<FacturaConsolidada>> GetFacturasConsolidadasPorPeriodo(
            DateTime fechaDesde, 
            DateTime fechaHasta, 
            Guid idEstacion);
    }
}