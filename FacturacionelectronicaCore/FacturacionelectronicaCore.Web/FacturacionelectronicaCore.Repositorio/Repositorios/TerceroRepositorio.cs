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
    public class TerceroRepositorio : ITerceroRepositorio
    {
        private readonly ISQLHelper _sqlHelper;

        public TerceroRepositorio(ISQLHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        /// <inheritdoc />
        public async Task<int> AddOrUpdate(IEnumerable<TerceroInput> terceros)
        {
            var terceroDataTable = terceros.ToDataTable();
            return await _sqlHelper.InsertOrUpdateOrDeleteAsync(StoredProcedures.UpdateOrCreateTerceros,
                new { tercero = terceroDataTable.AsTableValuedParameter(UserDefinedTypes.Tercero)});
        }

        /// <inheritdoc />
        public Task<IEnumerable<Tercero>> GetTerceros()
        {
            return _sqlHelper.GetsAsync<Tercero>(StoredProcedures.GetTerceros);
        }


        /// <inheritdoc />
        public Task<IEnumerable<Tercero>> GetTercerosActualizados()
        {
            var paramList = new DynamicParameters();
            return _sqlHelper.GetsAsync<Tercero>(StoredProcedures.GetTercerosActualizados, paramList);
        }

        public Task<IEnumerable<Tercero>> GetTercerosActualizados(Guid estacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("estacion", estacion);
            return _sqlHelper.GetsAsync<Tercero>(StoredProcedures.GetTercerosActualizados, paramList);
        }

        public Task<IEnumerable<Tercero>> ObtenerTerceroPorIdentificacion(string identificacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("identificacion", identificacion);
            return _sqlHelper.GetsAsync<Tercero>(StoredProcedures.ObtenerTerceroPorIdentificacion, paramList);
        }


        public async Task SetIdFacturacion(Guid guid, int idFacturacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("guid", guid);
            paramList.Add("idFacturacion", idFacturacion);
            await _sqlHelper.GetsAsync<int>(StoredProcedures.SetIdFacturacion, paramList);
        }
    }
}
