using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class OrdenDeDespacho
    {
        [BsonId]
        public string guid { get; set; }
        public int IdFactura { get; set; }
        public string Identificacion { get; set; }
        public string NombreTercero { get; set; }
        public string Combustible { get; set; }
        public double Cantidad { get; set; }
        public double Precio { get; set; }
        public double Total { get; set; }
        public decimal Descuento { get; set; }
        public string IdInterno { get; set; }
        public string Placa { get; set; }
        public string Kilometraje { get; set; }
        public int IdEstadoActual { get; set; }
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Manguera { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime Fecha { get; set; }
        
        public string Estado { get; set; }
        public string IdentificacionTercero { get; set; }
        public string FormaDePago { get; set; }
        public string FormaDePago2 { get; set; }
        public decimal? Total1 { get; set; }
        public decimal? Total2 { get; set; }
        public int IdLocal { get; set; }
        public int IdVentaLocal { get; set; }
        public int IdTerceroLocal { get; set; }
        public int IdEstacion { get; set; }
        public decimal SubTotal { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime FechaProximoMantenimiento { get; set; }
        
        public string Vendedor { get; set; }
        public string idFacturaElectronica { get; set; }
        
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime FechaReporte { get; set; }
    }
}
