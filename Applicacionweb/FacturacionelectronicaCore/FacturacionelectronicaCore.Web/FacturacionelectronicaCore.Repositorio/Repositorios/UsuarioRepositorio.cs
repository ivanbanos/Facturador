using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EstacionesServicio.Repositorio.Entities;
using EstacionesSevicio.Respositorio.Extention;
using FacturacionelectronicaCore.Repositorio.Recursos;

namespace EstacionesServicio.Repositorio.Repositorios
{
    public class UsuarioRepositorio : IUsuarioRespositorio
    {

        private readonly ISQLHelper _sqlHelper;

        public UsuarioRepositorio(ISQLHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public Task<IEnumerable<EstacionesServicio.Repositorio.Entities.Usuario>> GetUsuarios()
        {
            return _sqlHelper.GetsAsync<EstacionesServicio.Repositorio.Entities.Usuario>(StoredProcedures.GetUsuarios);
        }

        /// <inheritdoc />
        public async Task<Usuario> GetUsuario(string usuario, string contrasena)
        {
            var paramList = new DynamicParameters();
            paramList.Add("Usuario", usuario);
            paramList.Add("Contrasena", contrasena);

            return _sqlHelper.GetsAsync<Usuario>(StoredProcedures.GetUserGuidByUserNPass, paramList)
                    .Result?.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<Usuario> GetUsuario(Guid guid)
        {
            var paramList = new DynamicParameters();
            paramList.Add("Guid", guid.ToString());

            return _sqlHelper.GetsAsync<Usuario>(StoredProcedures.GetUserByGuid, paramList)
                    .Result?.FirstOrDefault();
        }

        /// <inheritdoc />
        public Task<int> SaveUsuario(IEnumerable<Usuario> usuarios)
        {
            // El tipo UsuarioType solo tiene Guid, Nombre, Usuario y Contrasena (en ese orden)
            var filas = usuarios.Select(x => new UsuarioTvp
            {
                Guid = x.guid,
                Nombre = x.Nombre,
                Usuario = x.NombreUsuario,
                Contrasena = x.Contrasena
            });
            return _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.SaveOrUpdateUser,
                new { usuario = filas.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Usuario) });
        }

        /// <inheritdoc />
        public async Task<int> CrearUsuario(Usuario usuario)
        {
            var paramList = new DynamicParameters();
            paramList.Add("Guid", usuario.guid);
            paramList.Add("Nombre", usuario.Nombre);
            paramList.Add("Usuario", usuario.NombreUsuario);
            paramList.Add("Contrasena", usuario.Contrasena);
            paramList.Add("IdentificacionTercero", usuario.IdentificacionTercero);
            paramList.Add("Rol", usuario.Rol);

            var resultado = await _sqlHelper.GetsAsync<int>(StoredProcedures.CrearUsuario, paramList);
            return resultado?.FirstOrDefault() ?? 0;
        }

        /// <inheritdoc />
        public async Task<int> CambiarContrasena(Guid guid, string contrasenaActual, string contrasenaNueva)
        {
            var paramList = new DynamicParameters();
            paramList.Add("Guid", guid);
            paramList.Add("ContrasenaActual", contrasenaActual);
            paramList.Add("ContrasenaNueva", contrasenaNueva);

            var resultado = await _sqlHelper.GetsAsync<int>(StoredProcedures.CambiarContrasenaUsuario, paramList);
            return resultado?.FirstOrDefault() ?? 0;
        }

        /// <inheritdoc />
        public async Task<int> ActualizarUsuario(Usuario usuario)
        {
            var paramList = new DynamicParameters();
            paramList.Add("Guid", usuario.guid);
            paramList.Add("Nombre", usuario.Nombre);
            paramList.Add("Usuario", usuario.NombreUsuario);
            paramList.Add("Contrasena", string.IsNullOrEmpty(usuario.Contrasena) ? null : usuario.Contrasena);
            paramList.Add("IdentificacionTercero", usuario.IdentificacionTercero);
            paramList.Add("Rol", usuario.Rol);

            var resultado = await _sqlHelper.GetsAsync<int>(StoredProcedures.ActualizarUsuario, paramList);
            return resultado?.FirstOrDefault() ?? 0;
        }

        /// <inheritdoc />
        public async Task<int> CambiarEstado(Guid guid, bool activo)
        {
            var paramList = new DynamicParameters();
            paramList.Add("Guid", guid);
            paramList.Add("Activo", activo);

            var resultado = await _sqlHelper.GetsAsync<int>(StoredProcedures.CambiarEstadoUsuario, paramList);
            return resultado?.FirstOrDefault() ?? 0;
        }

        /// <inheritdoc />
        public async Task<int> EliminarUsuario(Guid guid)
        {
            var paramList = new DynamicParameters();
            paramList.Add("Guid", guid);

            var resultado = await _sqlHelper.GetsAsync<int>(StoredProcedures.EliminarUsuario, paramList);
            return resultado?.FirstOrDefault() ?? 0;
        }

        private class UsuarioTvp
        {
            public Guid Guid { get; set; }
            public string Nombre { get; set; }
            public string Usuario { get; set; }
            public string Contrasena { get; set; }
        }
    }
}
