using System;

namespace EstacionesServicio.Repositorio.Entities
{
    public class Usuario
    {
        public Guid guid { get; set; }
        public string Nombre { get; set; }
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
    }
}
