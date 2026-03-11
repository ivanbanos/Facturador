using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class OrdenesMongo : OrdenDeDespacho
    {
        public OrdenesMongo() { }

        public OrdenesMongo(OrdenDeDespacho orden)
        {
            guid = orden.guid;
            IdFactura = orden.IdFactura;
            Identificacion = orden.Identificacion;
            NombreTercero = orden.NombreTercero;
            Combustible = orden.Combustible;
            Cantidad = orden.Cantidad;
            Precio = orden.Precio;
            Total = orden.Total;
            Descuento = orden.Descuento;
            IdInterno = orden.IdInterno;
            Placa = orden.Placa;
            Kilometraje = orden.Kilometraje;
            IdEstadoActual = orden.IdEstadoActual;
            Surtidor = orden.Surtidor;
            Cara = orden.Cara;
            Manguera = orden.Manguera;
            Fecha = orden.Fecha;
            FechaReporte = orden.FechaReporte;
            Estado = orden.Estado;
            IdentificacionTercero = orden.IdentificacionTercero;
            FormaDePago = orden.FormaDePago;
            FormaDePago2 = orden.FormaDePago2;
            Total1 = orden.Total1;
            Total2 = orden.Total2;
            IdLocal = orden.IdLocal;
            IdVentaLocal = orden.IdVentaLocal;
            IdTerceroLocal = orden.IdTerceroLocal;
            IdEstacion = orden.IdEstacion;
            SubTotal = orden.SubTotal;
            FechaProximoMantenimiento = orden.FechaProximoMantenimiento;
            Vendedor = orden.Vendedor;
            idFacturaElectronica = orden.idFacturaElectronica;
        }

        public string EstacionGuid { get; set; }
        public string TurnoGuid { get; set; }
    }
}
