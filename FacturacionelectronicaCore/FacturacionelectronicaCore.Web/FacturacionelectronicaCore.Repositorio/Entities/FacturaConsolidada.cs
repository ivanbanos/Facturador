using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class FacturaConsolidada
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public string Guid { get; set; } = System.Guid.NewGuid().ToString();
        
        /// <summary>
        /// Número consecutivo de la factura consolidada
        /// </summary>
        public int Consecutivo { get; set; }
        
        /// <summary>
        /// Fecha de creación de la factura consolidada
        /// </summary>
        public DateTime FechaCreacion { get; set; }
        
        /// <summary>
        /// Fecha desde la cual se consolidaron las órdenes
        /// </summary>
        public DateTime FechaDesde { get; set; }
        
        /// <summary>
        /// Fecha hasta la cual se consolidaron las órdenes
        /// </summary>
        public DateTime FechaHasta { get; set; }
        
        /// <summary>
        /// Identificación del cliente (tercero)
        /// </summary>
        public string IdentificacionCliente { get; set; }
        
        /// <summary>
        /// Nombre del cliente
        /// </summary>
        public string NombreCliente { get; set; }
        
        /// <summary>
        /// Información del tercero para facturación
        /// </summary>
        public Tercero Cliente { get; set; }
        
        /// <summary>
        /// GUID de la estación donde se creó
        /// </summary>
        public string IdEstacion { get; set; }
        
        /// <summary>
        /// Lista de GUIDs de las órdenes consolidadas
        /// </summary>
        public List<string> OrdenesConsolidadas { get; set; } = new List<string>();
        
        /// <summary>
        /// Estado de la factura: Creada, Enviada, Error, Exitosa
        /// </summary>
        public string Estado { get; set; } = "Creada";
        
        /// <summary>
        /// ID de la factura electrónica generada por el proveedor
        /// </summary>
        public string IdFacturaElectronica { get; set; }
        
        /// <summary>
        /// Información adicional del resultado de facturación electrónica
        /// </summary>
        public string InfoFacturacionElectronica { get; set; }
        
        /// <summary>
        /// Resumen de totales por combustible
        /// </summary>
        public List<ResumenCombustible> ResumenCombustibles { get; set; } = new List<ResumenCombustible>();
        
        /// <summary>
        /// Totales generales de la factura
        /// </summary>
        public TotalesFactura Totales { get; set; } = new TotalesFactura();
        
        /// <summary>
        /// Usuario que creó la factura consolidada
        /// </summary>
        public string UsuarioCreacion { get; set; }
        
        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime FechaActualizacion { get; set; }
        
        /// <summary>
        /// Observaciones o notas adicionales
        /// </summary>
        public string Observaciones { get; set; }
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
}