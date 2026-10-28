using System;

namespace EstacionesServicio.Modelo
{
    public class Usuario
    {
        public Guid guid { get; set; }
        public string Nombre { get; set; }
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }
        /// <summary>
        /// Identificacion del tercero (cliente) asociado. Null para usuarios administradores.
        /// </summary>
        public string IdentificacionTercero { get; set; }
        /// <summary>
        /// false = inhabilitado (no puede iniciar sesion)
        /// </summary>
        public bool Activo { get; set; } = true;
        /// <summary>
        /// Perfil de los usuarios sin cliente: null = Administrador, "Operador" = sin gestion de usuarios.
        /// </summary>
        public string Rol { get; set; }

        public Usuario(Guid _guid, string nombre, string b, string c)
        {
            guid = _guid;
            Nombre = nombre;
            NombreUsuario = b;
            Contrasena = c;
        }

        public Usuario()
        {
        }

    }
}
