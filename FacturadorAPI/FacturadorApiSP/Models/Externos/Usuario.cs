using System;

namespace FacturadorAPI.Models.Externos
{
    public class Usuario
    {
        public Guid guid { get; set; }
        public string Nombre { get; set; }
        public string NombreUsuario { get; set; }
        public string Contrasena { get; set; }

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
