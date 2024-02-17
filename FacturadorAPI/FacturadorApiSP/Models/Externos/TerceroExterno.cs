using System;
using System.Collections.Generic;
using System.Text;

namespace FacturadorAPI.Models.Externos
{
    public class TerceroExterno
    {
        public TerceroExterno()
        { }
        public TerceroExterno(Models.Tercero x)
        {
            Nombre = string.IsNullOrEmpty(x.Nombre) ? "No informado" : x.Nombre;
            Direccion = string.IsNullOrEmpty(x.Direccion) ? "No informado" : x.Direccion;
            Telefono = string.IsNullOrEmpty(x.Telefono) ? "No informado" : x.Telefono;
            Correo = string.IsNullOrEmpty(x.Correo) ? "No informado" : x.Correo;
            DescripcionTipoIdentificacion = string.IsNullOrEmpty(x.tipoIdentificacionS) ? "No especificada" : x.tipoIdentificacionS;
            Identificacion = string.IsNullOrEmpty(x.identificacion) ? "No informado" : x.identificacion;
            IdLocal = x.terceroId;
        }
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
