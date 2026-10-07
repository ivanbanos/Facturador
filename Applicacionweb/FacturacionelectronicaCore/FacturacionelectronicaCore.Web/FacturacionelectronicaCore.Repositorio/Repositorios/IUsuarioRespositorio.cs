using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EstacionesServicio.Repositorio.Entities;

namespace EstacionesServicio.Repositorio.Repositorios
{
    public interface IUsuarioRespositorio
    {
        /// <summary>
        /// Obtiene una lista de todos los usuarios
        /// </summary>
        /// <returns>Coleccion de Usuarios</returns>
        Task<IEnumerable<Usuario>> GetUsuarios();
        /// <summary>
        /// Obtiene un usuario por usuario y constrasena 
        /// </summary>
        /// <param name="usuario">usuario</param>
        /// <param name="contrasena">contrasena</param>
        /// <returns>Usuario</returns>
        Task<Usuario> GetUsuario(string usuario, string contrasena);
        /// <summary>
        /// Obtiene un usuario por Guid
        /// </summary>
        /// <param name="guid">Identificador unico</param>
        /// <returns>Usuario</returns>
        Task<Usuario> GetUsuario(Guid guid);
        /// <summary>
        /// Crear un usuario
        /// </summary>
        /// <param name="usuario">usuario a crear</param>
        /// <returns>Usuario</returns>
        Task<int> SaveUsuario(IEnumerable<Usuario> usuario);
        /// <summary>
        /// Crea un usuario nuevo validando que el nombre de usuario no exista
        /// </summary>
        /// <param name="usuario">usuario a crear</param>
        /// <returns>1 si se creo, 0 si el nombre de usuario ya existe</returns>
        Task<int> CrearUsuario(Usuario usuario);
        /// <summary>
        /// Cambia la contrasena de un usuario validando la contrasena actual
        /// </summary>
        /// <returns>Cantidad de filas actualizadas (0 si la contrasena actual no coincide)</returns>
        Task<int> CambiarContrasena(Guid guid, string contrasenaActual, string contrasenaNueva);
    }
}
