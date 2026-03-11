using Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EstacionesServicio.Repositorio.Common.SQLHelper
{
    public interface ISQLHelper
    {
        /// <summary>
        /// Executes an stored procedure without parameters.
        /// </summary>
        /// <typeparam name="T">Entity to map</typeparam>
        /// <param name="spName">Stored Procedure Name</param>
        /// <returns>IEnumerable of T/></returns>
        IEnumerable<T> Gets<T>(string spName);

        /// <summary>
        /// Executes an stored procedure with Dynamic Parameters.
        /// </summary>
        /// <typeparam name="T">Entity to map</typeparam>
        /// <param name="spName">Stored Procedure Name</param>
        /// <param name="parms">Dynamic Parameters</param>
        // <returns>IEnumerable of T/></returns>
        IEnumerable<T> Gets<T>(string spName, DynamicParameters parms);

        /// <summary>
        /// Executes an stored procedure that recive an entity.
        /// </summary>
        /// <typeparam name="T">Entity to map</typeparam>
        /// <typeparam name="U">Type of Entity to send</typeparam>
        /// <param name="spName">Stored Procedure Name</param>
        /// <param name="entity">Entity to send</param>
        /// <returns>IEnumerable of T/></returns>
        IEnumerable<T> Gets<T, U>(string spName, U entity);

        /// <summary>
        /// Insert values with dinamyc parameters
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="param"></param>
        /// <returns>Int = 1 (Sucess)</returns>
        int InsertOrUpdateOrDelete(string spName, DynamicParameters param);

        /*-------------------------- Asynchronous methods ------------------------*/

        /// <summary>
        /// Executes an stored procedure without parameters in async way.
        /// </summary>
        /// <typeparam name="T">Entity to map</typeparam>
        /// <param name="spName">Stored Procedure Name</param>
        /// <returns>IEnumerable of T/></returns>
        Task<IEnumerable<T>> GetsAsync<T>(string spName);

        /// <summary>
        /// Executes an stored procedure with Dynamic Parameters in async way.
        /// </summary>
        /// <typeparam name="T">Entity to map</typeparam>
        /// <param name="spName">Stored Procedure Name</param>
        /// <param name="parms">Dynamic Parameters</param>
        // <returns>IEnumerable of T/></returns>
        Task<IEnumerable<T>> GetsAsync<T>(string spName, DynamicParameters parms);

        /// <summary>
        /// Async insert values with dynamic parameters 
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="param"></param>
        /// <returns>Int = 1 (Sucess)</returns>
        Task<int> InsertOrUpdateOrDeleteAsync(string spName, DynamicParameters param);

        /// <summary>
        /// Async insert values with entity 
        /// </summary>
        /// <param name="spName"></param>
        /// <param name="param"></param>
        /// <returns>Int = 1 (Sucess)</returns>
        Task<int> InsertOrUpdateOrDeleteAsync<U>(string spName, U entity);

    }
}
