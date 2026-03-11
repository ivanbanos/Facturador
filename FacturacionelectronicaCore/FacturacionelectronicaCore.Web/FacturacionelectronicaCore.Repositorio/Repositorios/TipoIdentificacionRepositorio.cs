using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using FacturacionelectronicaCore.Repositorio.Recursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class TipoIdentificacionRepositorio : ITipoIdentificacionRepositorio
    {
        private readonly ISQLHelper _sqlHelper;

        public TipoIdentificacionRepositorio(ISQLHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }
        public int GetIdByTexto(string descripcionTipoIdentificacion)
        {
            var paramList = new DynamicParameters();
            paramList.Add("Texto", descripcionTipoIdentificacion);
            return _sqlHelper.GetsAsync<int>(StoredProcedures.GetTipoIdentificacionIdByTexto, paramList).Result.FirstOrDefault();
        }

        public async Task<IEnumerable<string>> GetTipos()
        {
            var paramList = new DynamicParameters();
            return await _sqlHelper.GetsAsync<string>(StoredProcedures.GetTiposIdentificaciones, paramList);
        }
    }
}
