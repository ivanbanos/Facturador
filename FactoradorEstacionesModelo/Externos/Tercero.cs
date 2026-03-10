using System;
using System.Collections.Generic;
using System.Text;

namespace FacturacionelectronicaCore.Negocio.Modelo
{
    public class Tercero
    {
        public Tercero()
        { }
        public Tercero(FactoradorEstacionesModelo.Objetos.Tercero x)
        {
            Nombre = ObtenerNombreCompleto(x.Nombre, x.Apellidos);
            Apellidos = string.IsNullOrWhiteSpace(x.Apellidos) ? "No informado" : x.Apellidos;
            Direccion = string.IsNullOrEmpty(x.Direccion)? "No informado":x.Direccion;
            Telefono = string.IsNullOrEmpty(x.Telefono) ? "No informado" : x.Telefono;
            Correo = string.IsNullOrEmpty(x.Correo) ? "No informado" : x.Correo;
            DescripcionTipoIdentificacion = string.IsNullOrEmpty(x.tipoIdentificacionS) ? "No especificada" : x.tipoIdentificacionS;
            Identificacion = string.IsNullOrEmpty(x.identificacion) ? "No informado" : x.identificacion;
            IdLocal = x.terceroId;
        }

        private static string ObtenerNombreCompleto(string nombre, string apellidos)
        {
            var nombreLimpio = string.IsNullOrWhiteSpace(nombre) ? string.Empty : nombre.Trim();
            var apellidosLimpios = string.IsNullOrWhiteSpace(apellidos) ? string.Empty : apellidos.Trim();
            var nombreCompleto = $"{nombreLimpio} {apellidosLimpios}".Trim();
            return string.IsNullOrWhiteSpace(nombreCompleto) ? "No informado" : nombreCompleto;
        }

        public int Id { get; set; }
        public Guid guid { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
        public string DescripcionTipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public int IdLocal { get; set; }
    }
}
