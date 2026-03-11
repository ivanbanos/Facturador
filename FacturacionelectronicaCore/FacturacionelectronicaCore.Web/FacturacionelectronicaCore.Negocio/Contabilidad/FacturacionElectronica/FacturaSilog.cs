using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Contabilidad.FacturacionElectronica
{
    public class FacturaSilog
    {
        public string numeroTransaccion { get; set; }

        public FacturaSilog() { }
        
        public Guid Guid { get; set; }

        public int Consecutivo { get; set; }

        public Guid IdTercero { get; set; }
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
        public Guid IdResolucion { get; set; }
        public int IdEstadoActual { get; set; }
        public string Surtidor { get; set; }
        public string Cara { get; set; }
        public string Manguera { get; set; }
        public string FormaDePago { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; }
        public TerceroSilog Tercero { get; set; }
        public int IdLocal { get; set; }
        public int IdVentaLocal { get; set; }
        public int IdTerceroLocal { get; set; }
        public DateTime FechaProximoMantenimiento { get; set; }
        public decimal SubTotal { get; set; }
        public string Vendedor { get; set; }
        public string DescripcionResolucion { get; set; }
        public string Prefijo { get; set; }
        public string Cedula { get; set; }
    }

    public class TerceroSilog
    {
        public int Id { get; set; }
        public Guid guid { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string DescripcionTipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public int IdLocal { get; set; }
    }
}
