using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using EstacionesSevicio.Respositorio.Extention;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Recursos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class EstacionesRepository : IEstacionesRepository
    {
        private readonly ISQLHelper _sqlHelper;

        public EstacionesRepository(ISQLHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        /// <inheritdoc />
        public async Task AddRange(IEnumerable<Estacion> lists)
        {
            var dataTable = lists.ToDataTable();
            await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.UpdateOrCreateEstacion,
                new { estaciones = dataTable.AsTableValuedParameter(UserDefinedTypes.Estacion) }).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task<IEnumerable<Estacion>> GetEstaciones()
        {
            return _sqlHelper.GetsAsync<Estacion>(StoredProcedures.GetEstaciones);
        }

        /// <inheritdoc />
        public Task<IEnumerable<Estacion>> GetEstacion(Guid estacionGuid)
        {
            var paramList = new DynamicParameters();
            paramList.Add("idEstacion", estacionGuid);

            return _sqlHelper.GetsAsync<Estacion>(StoredProcedures.GetEstacion, paramList);
        }

        /// <inheritdoc />
        public Task<int> BorrarEstacion(IEnumerable<FacturasEntity> estaciones)
        {
            return _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.BorrarEstacion,
                new { Estaciones = estaciones.ToDataTable().AsTableValuedParameter(UserDefinedTypes.Entity) });
        }
    }
}
