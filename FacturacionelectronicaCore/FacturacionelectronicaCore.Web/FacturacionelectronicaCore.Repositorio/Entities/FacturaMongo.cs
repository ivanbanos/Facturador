using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class FacturaMongo : Factura
    {
        public FacturaMongo() { }

        public FacturaMongo(Factura factura)
        {
            Id = factura.Id;
            Consecutivo = factura.Consecutivo;
            IdTercero = factura.IdTercero;
            Identificacion = factura.Identificacion;
            NombreTercero = factura.NombreTercero;
            Combustible = factura.Combustible;
            Cantidad = factura.Cantidad;
            Precio = factura.Precio;
            Total = factura.Total;
            Descuento = factura.Descuento;
            IdInterno = factura.IdInterno;
            Placa = factura.Placa;
            Kilometraje = factura.Kilometraje;
            IdResolucion = factura.IdResolucion;
            IdEstadoActual = factura.IdEstadoActual;
            Surtidor = factura.Surtidor;
            Cara = factura.Cara;
            Manguera = factura.Manguera;
            Fecha = factura.Fecha;
            FechaReporte = factura.FechaReporte;
            Estado = factura.Estado;
            IdentificacionTercero = factura.IdentificacionTercero;
            FormaDePago = factura.FormaDePago;
            IdLocal = factura.IdLocal;
            IdVentaLocal = factura.IdVentaLocal;
            IdTerceroLocal = factura.IdTerceroLocal;
            IdEstacion = factura.IdEstacion;
            SubTotal = factura.SubTotal;
            FechaProximoMantenimiento = factura.FechaProximoMantenimiento;
            Vendedor = factura.Vendedor;
            DescripcionResolucion = factura.DescripcionResolucion;
            AutorizacionResolucion = factura.AutorizacionResolucion;
            idFacturaElectronica = factura.idFacturaElectronica;
        }

        public string EstacionGuid { get; set; }
        public string TurnoGuid { get; set; }
    }
}
