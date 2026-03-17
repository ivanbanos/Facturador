using EstacionesServicio.Modelo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Estacion
{
    public interface IEstacionNegocio
    {
        Task AddEstacion(IEnumerable<Modelo.Estacion> estacions);
        Task<int> BorrarEstacion(IEnumerable<FacturasEntity> estaciones);
        Task<Modelo.Estacion> GetEstacion(Guid estacionGuid);
        Task<IEnumerable<Modelo.Estacion>> GetEstaciones();
        Task<IEnumerable<Modelo.CombustibleEstacion>> GetCombustiblesEstacion(Guid estacionGuid);
        Task UpsertCombustibleEstacion(Guid estacionGuid, Modelo.CombustibleEstacion combustible);
    }
}