using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class Factura
    {
        public Factura() { }
        public Factura(int Consecutivo, string Combustible, decimal Cantidad, decimal Precio,
            decimal Total, string IdInterno, string Placa, string Kilometraje,
            string Surtidor, string Cara, string Manguera, DateTime Fecha, string IdentificacionTercero,
            string FormaDePago, int IdLocal, int IdVentaLocal
, DateTime fechaReporte)
        {
            this.Consecutivo = Consecutivo;
            this.Combustible = Combustible;
            this.Cantidad = Cantidad;
            this.Precio = Precio;
            this.Total = Total;
            this.IdInterno = IdInterno;
            this.Placa = Placa;
            this.Kilometraje = Kilometraje;
            this.Surtidor = Surtidor;
            this.Cara = Cara;
            this.Manguera = Manguera;
            this.Fecha = Fecha;
            this.IdentificacionTercero = IdentificacionTercero;
            this.FormaDePago = FormaDePago;
            this.IdLocal = IdLocal;
            this.IdVentaLocal = IdVentaLocal;
            FechaReporte = fechaReporte;
        }

        public int Id { get; set; }
        [BsonId]
        public string Guid { get; set; }
        public int Consecutivo { get; set; }
        public string IdTercero { get; set; }
        public string Identificacion { get; set; }
        public string NombreTercero { get; set; }
        public string Combustible { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Total { get; set; }
        public decimal Descuento { get; set; }
        public string IdInterno { get; set; }
        public string Placa { get; set; }
        public string Kilometraje { get; set; }
        public string IdResolucion { get; set; }
        public int IdEstadoActual { get; set; }
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Manguera { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public string IdentificacionTercero { get; set; }
        public string FormaDePago { get; set; }
        public int IdLocal { get; set; }
        public int IdVentaLocal { get; set; }
        public int IdTerceroLocal { get; set; }
        public int IdEstacion { get; set; }
        public decimal SubTotal { get; set; }
        public DateTime FechaProximoMantenimiento { get; set; }
        public string Vendedor { get; set; }
        public string DescripcionResolucion { get; set; }
        public string AutorizacionResolucion { get; set; }
        public string idFacturaElectronica { get; set; }
        public DateTime FechaReporte { get; set; }
    }
}
