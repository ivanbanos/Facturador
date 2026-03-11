using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using EstacionesServicio.Repositorio.Entities;
using EstacionesSevicio.Respositorio.Extention;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EstacionesServicio.Repositorio.Repositorios
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity
    {
        private readonly string _agregarStoreProcedure;
        private readonly string _modificarStoreProcedure;
        private readonly string _listarStoreProcedure;
        private readonly string _obtenerStoreProcedure;
        private readonly string _borrarStoreProcedure;
        private readonly string _tipoDefinidoUsuario;
        private readonly ISQLHelper _sqlHelper;

        public Repository(string agregarStoreProcedure, string modificarStoreProcedure,
            string listarStoreProcedure, string obtenerStoreProcedure, string borrarStoreProcedure,
            string tipoDefinidoUsuario, ISQLHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
            _agregarStoreProcedure = agregarStoreProcedure;
            _modificarStoreProcedure = modificarStoreProcedure;
            _listarStoreProcedure = listarStoreProcedure;
            _obtenerStoreProcedure = obtenerStoreProcedure;
            _borrarStoreProcedure = borrarStoreProcedure;
            _tipoDefinidoUsuario = tipoDefinidoUsuario;
        }

        public Task Agregar(IEnumerable<TEntity> entity)
            => _sqlHelper.InsertOrUpdateOrDeleteAsync(_agregarStoreProcedure,
                new { entity = entity.ToDataTable<TEntity>().AsTableValuedParameter(_tipoDefinidoUsuario) });

        public Task Borrar(Guid guid)
            => _sqlHelper.InsertOrUpdateOrDeleteAsync(_borrarStoreProcedure,
                new { Guid = guid });

        public Task<IEnumerable<TEntity>> Listar()
            => _sqlHelper.GetsAsync<TEntity>(_listarStoreProcedure);

        public Task Modificar(IEnumerable<TEntity> entity)
            => _sqlHelper.InsertOrUpdateOrDeleteAsync(_modificarStoreProcedure,
                new { entity = entity.ToDataTable<TEntity>().AsTableValuedParameter(_tipoDefinidoUsuario) });

        public TEntity Obtener(Guid guid)
        {
            var paramList = new DynamicParameters();
            paramList.Add("Guid", guid.ToString());

            return _sqlHelper.GetsAsync<TEntity>(_obtenerStoreProcedure, paramList)
                    .Result?.FirstOrDefault();
        }
    }
}
