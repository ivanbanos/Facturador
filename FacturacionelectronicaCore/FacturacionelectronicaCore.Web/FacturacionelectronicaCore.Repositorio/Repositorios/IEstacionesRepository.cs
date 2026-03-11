using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface IEstacionesRepository
    {
        Task AddRange(IEnumerable<Estacion> lists);
        Task<int> BorrarEstacion(IEnumerable<FacturasEntity> estaciones);
        Task<IEnumerable<Estacion>> GetEstacion(Guid estacionGuid);
        Task<IEnumerable<Estacion>> GetEstaciones();
    }
}