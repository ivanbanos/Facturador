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
    }
}
