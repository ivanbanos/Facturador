using FactoradorEstacionesModelo.Siges;
using FacturacionelectronicaCore.Negocio.Modelo;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class Factura
    {
        public Factura() { }
        public Factura(FacturaSiges x, string forma)
        { 
            
            Guid = Guid.NewGuid();
            Consecutivo = x.Consecutivo==0?x.ventaId: x.Consecutivo;
            Combustible = x.Combustible;
            Cantidad = x.Cantidad;
            Precio = x.Precio;
            Total = x.Total;
            IdInterno = "";
            Placa = x.Placa;
            Kilometraje = x.Kilometraje;
            Surtidor = x.Surtidor + "";
            Cara = x.Cara + "";
            Manguera = x.Manguera + "";
            FormaDePago = forma;
            FormaDePago2 = string.Empty;
            Fecha = x.fecha;
            Tercero = new Tercero(x.Tercero);
            Descuento = x.Descuento;
            IdLocal = x.facturaPOSId;
            IdVentaLocal = x.ventaId;
            IdTerceroLocal = x.Tercero.terceroId;
            FechaProximoMantenimiento = DateTime.Now;
            SubTotal = x.Subtotal;
            Total1 = x.total1;
            Total2 = x.total2;
            Vendedor = x.Empleado;
            Identificacion = x.Tercero.identificacion;
            NombreTercero = Tercero?.Nombre;
            Prefijo = x.DescripcionResolucion;
            Cedula = "";
            
        }
        public Guid Guid { get; set; }

        public int Consecutivo { get; set; }

        public Guid IdTercero { get; set; }
        public string Identificacion { get; set; }
        public string NombreTercero { get; set; }
        public string Combustible { get; set; }
        public double Cantidad { get; set; }
        public double Precio { get; set; }
        public double Total { get; set; }
        public double Descuento { get; set; }
        public string IdInterno { get; set; }
        public string Placa { get; set; }
        public string Kilometraje { get; set; }
        public Guid IdResolucion { get; set; }
        public int IdEstadoActual { get; set; }
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Manguera { get; set; }
        public string FormaDePago { get; set; }
        public string FormaDePago2 { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public Tercero Tercero { get; set; }
        public int IdLocal { get; set; }
        public int IdVentaLocal { get; set; }
        public int IdTerceroLocal { get; set; }
        public DateTime FechaProximoMantenimiento { get; set; }
        public double SubTotal { get; set; }
        public double? Total1 { get; set; }
        public double? Total2 { get; set; }
        public string Vendedor { get; set; }
        public string DescripcionResolucion { get; set; }
        public IEnumerable<OrdenDeDespacho> Ordenes { get; set; }
        public string Prefijo { get; set; }
        public string Cedula { get; set; }
    }
}
