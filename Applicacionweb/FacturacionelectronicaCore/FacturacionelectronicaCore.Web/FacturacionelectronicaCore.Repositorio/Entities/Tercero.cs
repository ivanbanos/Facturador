using EstacionesServicio.Repositorio.Entities;
using System;

namespace FacturacionelectronicaCore.Repositorio.Entities
{
    public class Tercero : Entity
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Nombre { get; set; }
        public string Segundo { get; set; }
        public string Apellidos { get; set; }
        public string Municipio { get; set; }
        public string Departamento { get; set; }
        public string Direccion { get; set; }

        public int TipoPersona { get; set; }
        public int ResponsabilidadTributaria { get; set; }
        public string Pais { get; set; }
        public string CodigoPostal { get; set; }
        public string Celular { get; set; }
        public string Telefono { get; set; }
        public string Telefono2 { get; set; }
        public string Correo { get; set; }
        public string Correo2 { get; set; }
        public string Vendedor { get; set; }
        public string Comentarios { get; set; }
        public int TipoIdentificacion { get; set; }
        public string Identificacion { get; set; }
        public string DescripcionTipoIdentificacion { get; set; }
        public int IdLocal { get; set; }
        public int IdContable { get; set; }
        public string idFacturacion { get; set; }

    }
}
