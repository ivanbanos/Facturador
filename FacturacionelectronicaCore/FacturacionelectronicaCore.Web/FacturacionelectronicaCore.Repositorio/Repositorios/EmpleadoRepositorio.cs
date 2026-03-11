using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Recursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public class EmpleadoRepositorio : IEmpleadoRepositorio
    {
        private readonly ISQLHelper _sqlHelper;

        public EmpleadoRepositorio(ISQLHelper sqlHelper)
        {
            _sqlHelper = sqlHelper;
        }

        public async Task<string> GetEmpleadoByName(string vendedor)
        {
            var paramList = new DynamicParameters();
            paramList.Add("Nombre", vendedor);
            return (await _sqlHelper.GetsAsync<Empleado>(StoredProcedures.GetEmpleadosByNombre, paramList)).FirstOrDefault()?.Cedula;
        }
    }
}
