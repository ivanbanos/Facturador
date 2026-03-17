using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class CombustiblesEstacionRepository : ICombustiblesEstacionRepository
    {
        private readonly ISQLHelper _sqlHelper;

        public CombustiblesEstacionRepository(ISQLHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public Task<IEnumerable<CombustibleEstacion>> GetCombustiblesEstacion(Guid estacionGuid)
        {
            var paramList = new DynamicParameters();
            paramList.Add("IdEstacion", estacionGuid);
            return _sqlHelper.GetsAsync<CombustibleEstacion>("[dbo].[GetCombustiblesEstacion]", paramList);
        }

        public async Task UpsertCombustibleEstacion(Guid estacionGuid, string combustible, decimal precio, bool esGas)
        {
            var paramList = new DynamicParameters();
            paramList.Add("IdEstacion", estacionGuid);
            paramList.Add("Combustible", combustible);
            paramList.Add("Precio", precio);
            paramList.Add("EsGas", esGas);

            await _sqlHelper.InsertOrUpdateOrDeleteAsync("[dbo].[UpsertCombustibleEstacion]", paramList).ConfigureAwait(false);
        }
    }
}
