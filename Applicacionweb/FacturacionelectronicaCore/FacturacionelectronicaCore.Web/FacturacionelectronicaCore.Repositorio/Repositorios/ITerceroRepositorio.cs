using FacturacionelectronicaCore.Repositorio.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Repositorio.Repositorios
{
    public interface ITerceroRepositorio
    {
        /// <summary>
        /// Obtiene una lista de todos los terceros
        /// </summary>
        /// <returns>Coleccion de Terceros</returns>
        Task<IEnumerable<Tercero>> GetTerceros();

        /// <summary>
        /// Agrega o Actualiza una lista de terceros
        /// </summary>
        /// <param name="terceros"></param>
        /// <returns></returns>
        Task<int> AddOrUpdate(IEnumerable<TerceroInput> terceros);

        Task<IEnumerable<Tercero>> GetTercerosActualizados(Guid estacion);
        Task<IEnumerable<Tercero>> GetTercerosActualizados();
        Task SetIdFacturacion(Guid guid, int idFacturacion);

        Task<IEnumerable<Tercero>> ObtenerTerceroPorIdentificacion(string identificacion);
    }
}