using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EstacionesServicio.Negocio.Usuario
{
    public interface IUsuarioNegocio
    {
        /// <summary>
        /// Obtiene una lista de todos los usuarios
        /// </summary>
        /// <returns>Coleccion de usuarios</returns>
        Task<IEnumerable<Modelo.Usuario>> GetUsuarios();
        /// <summary>
        /// Obtiene un usuario por usuario y constrasena
        /// </summary>
        /// <param name="usuario"></param>
        /// <param name="contrasena">Contrasena</param>
        /// <returns>Identificador unico</returns>
        Task<Modelo.Usuario> GetUsuario(string usuario, string contrasena);
        /// <summary>
        /// Obtiene el usuario por Guid
        /// </summary>
        /// <param name="guid">Identificador unico</param>
        /// <returns>Usuario</returns>
        Task<Modelo.Usuario> GetUsuario(Guid guid);
        /// <summary>
        /// Crea un usuario
        /// </summary>
        /// <param name="usuario">Usuario a crear</param>
        /// <returns>Usuario</returns>
        Task<int> SaveUsuario(IEnumerable<Modelo.Usuario> usuarios);
        /// <summary>
        /// Crea un usuario nuevo. Si trae identificacion de tercero, el usuario queda como cliente.
        /// </summary>
        /// <returns>true si se creo; false si el nombre de usuario ya existe</returns>
        Task<bool> CrearUsuario(Modelo.Usuario usuario);
        /// <summary>
        /// Cambia la contrasena del usuario validando la actual
        /// </summary>
        /// <returns>true si se cambio; false si la contrasena actual no coincide</returns>
        Task<bool> CambiarContrasena(Guid guid, string contrasenaActual, string contrasenaNueva);
    }
}
