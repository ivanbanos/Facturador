using System;
using System.Collections.Generic;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class FacturaConsolidada
    {
        public string Id { get; set; }
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        public int Consecutivo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public string IdentificacionCliente { get; set; }
        public string NombreCliente { get; set; }
        public Tercero Cliente { get; set; }
        public Guid IdEstacion { get; set; }
        public List<string> OrdenesConsolidadas { get; set; } = new List<string>();
        public string Estado { get; set; } = "Creada";
        public string IdFacturaElectronica { get; set; }
        public string InfoFacturacionElectronica { get; set; }
        public List<ResumenCombustible> ResumenCombustibles { get; set; } = new List<ResumenCombustible>();
        public TotalesFactura Totales { get; set; } = new TotalesFactura();
        public string UsuarioCreacion { get; set; }
        public DateTime FechaActualizacion { get; set; }
        public string Observaciones { get; set; }
        
        /// <summary>
        /// Lista de órdenes detalladas (para mostrar en el detalle)
        /// </summary>
        public List<OrdenDeDespacho> OrdenesDetalle { get; set; } = new List<OrdenDeDespacho>();
    }
    
    public class ResumenCombustible
    {
        public string TipoCombustible { get; set; }
        public double CantidadTotal { get; set; }
        public double SubTotal { get; set; }
        public double Descuento { get; set; }
        public double Iva { get; set; }
        public double Total { get; set; }
        public int NumeroOrdenes { get; set; }
    }
    
    public class TotalesFactura
    {
        public double SubTotal { get; set; }
        public double DescuentoTotal { get; set; }
        public double IvaTotal { get; set; }
        public double Total { get; set; }
        public int TotalOrdenes { get; set; }
        public double CantidadTotalCombustible { get; set; }
    }
    
    /// <summary>
    /// DTO para crear una nueva factura consolidada
    /// </summary>
    public class CrearFacturaConsolidadaRequest
    {
        public string IdentificacionCliente { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public Guid IdEstacion { get; set; }
        public List<string> GuidsOrdenes { get; set; } = new List<string>();
        public string UsuarioCreacion { get; set; }
        public string Observaciones { get; set; }
    }
    
    /// <summary>
    /// DTO para filtrar órdenes pendientes de consolidación
    /// </summary>
    public class FiltroOrdenesConsolidacion
    {
        public string IdentificacionCliente { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public Guid IdEstacion { get; set; }
    }
}