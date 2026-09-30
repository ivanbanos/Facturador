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
            =>_sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.SaveOrUpdateUser, 
                new { usuario = usuarios.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Usuario) });
    }
}
