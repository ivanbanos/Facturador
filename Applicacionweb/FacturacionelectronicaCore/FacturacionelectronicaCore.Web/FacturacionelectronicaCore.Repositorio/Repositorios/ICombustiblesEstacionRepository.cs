using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface ICombustiblesEstacionRepository
    {
        Task<IEnumerable<CombustibleEstacion>> GetCombustiblesEstacion(Guid estacionGuid);
        Task UpsertCombustibleEstacion(Guid estacionGuid, string combustible, decimal precio, bool esGas);
    }
}
