using Dapper;
using EstacionesServicio.Repositorio.Common.SQLHelper;
using FacturacionelectronicaCore.Repositorio.Entities;
using FacturacionelectronicaCore.Repositorio.Recursos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
            foreach (var candidato in BuildSearchCandidates(vendedor))
            {
                // Some stations already store the employee document in the vendedor field.
                if (candidato.All(char.IsDigit))
                {
                    return candidato;
                }

                var paramList = new DynamicParameters();
                paramList.Add("Nombre", candidato);
                var cedula = (await _sqlHelper.GetsAsync<Empleado>(StoredProcedures.GetEmpleadosByNombre, paramList)).FirstOrDefault()?.Cedula;
                if (!string.IsNullOrWhiteSpace(cedula))
                {
                    return cedula;
                }
            }

            return null;
        }

        private static IEnumerable<string> BuildSearchCandidates(string vendedor)
        {
            if (string.IsNullOrWhiteSpace(vendedor))
            {
                yield break;
            }

            var trimmed = vendedor.Trim();
            var collapsedSpaces = Regex.Replace(trimmed, "\\s+", " ");

            foreach (var candidate in new[]
            {
                trimmed,
                collapsedSpaces,
                trimmed.ToUpperInvariant(),
                collapsedSpaces.ToUpperInvariant(),
                trimmed.ToLowerInvariant(),
                collapsedSpaces.ToLowerInvariant(),
            }.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal))
            {
                yield return candidate;
            }
        }
    }
}
