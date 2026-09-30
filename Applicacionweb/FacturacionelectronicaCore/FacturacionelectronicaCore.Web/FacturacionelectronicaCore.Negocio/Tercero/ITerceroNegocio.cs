using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FacturacionelectronicaCore.Negocio.Tercero
{
    public interface ITerceroNegocio
    {
        /// <summary>
        /// Agrega o actualiza un conjunto de terceros
        /// </summary>
        /// <param name="terceros">Terceros</param>
        /// <returns>Task complete</returns>
        Task<int> AddOrUpdate(IEnumerable<Modelo.Tercero> terceros);
        /// <summary>
        /// Obtiene la lista de terceros del sistema
        /// </summary>
        /// <returns>Lista de terceros</returns>
        Task<IEnumerable<Modelo.Tercero>> GetTerceros();
        /// <summary>
        /// Obtiene un tercero
        /// </summary>
        /// <param name="idTercero">Id del tercero</param>
        /// <returns></returns>
        Task<Modelo.Tercero> GetTercero(Guid idTercero);
        Task<bool> GetIsTerceroValidoPorIdentificacion(string identificacion);
        Task SincronizarTerceros();
    }
}