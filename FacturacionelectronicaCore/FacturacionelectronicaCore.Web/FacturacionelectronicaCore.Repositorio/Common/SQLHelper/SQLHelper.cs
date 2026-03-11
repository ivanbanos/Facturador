using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EstacionesServicio.Repositorio.Common.SQLHelper
{
    public class SQLHelper : ISQLHelper
    {
        private readonly string _connectionString;

        public SQLHelper(string connectionString)
        {
            _connectionString = connectionString;
        }


        /// <inheritdoc />
        public IEnumerable<T> Gets<T>(string spName)
        {
            return WithConnection(c =>
            {
                var result = c.Query<T>(
                    sql: spName,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 300);
                return result;
            });
        }

        /// <inheritdoc />
        public IEnumerable<T> Gets<T>(string spName, DynamicParameters parms)
        {
            return WithConnection(c =>
            {
                var result = c.Query<T>(
                    sql: spName,
                    param: parms,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 300);
                return result;
            });
        }

        /// <inheritdoc />
        public IEnumerable<T> Gets<T, U>(string spName, U entity)
        {
            return WithConnection(c =>
            {
                var result = c.Query<T>(
                    sql: spName,
                    param: entity,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 300);
                return result;
            });
        }

        /// <inheritdoc />
        public int InsertOrUpdateOrDelete(string spName, DynamicParameters param)
        {
            return WithConnection(c =>
            {
                var result = c.Execute(
                    sql: spName,
                    param: param,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 300);
                return result;
            });
        }


        /// <summary>
        /// Gets the connection an executes the querry
        /// </summary>
        /// <typeparam name="T">Generic to map the querry</typeparam>
        /// <param name="getData">Querry data</param>
        /// <returns>Connection</returns>
        private T WithConnection<T>(Func<IDbConnection, T> getData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    return getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout {ex}");
            }
            catch (SqlException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL exception (not a timeout) {ex}");
            }
        }

        #region AsynchronousMethods 


        /// <inheritdoc />
        public Task<IEnumerable<T>> GetsAsync<T>(string spName)
        {
            return WithConnectionAsync(c =>
            {
                var result = c.QueryAsync<T>(
                    sql: spName,
                    commandType: CommandType.StoredProcedure, commandTimeout: 300);
                return result;
            });
        }

        /// <inheritdoc />
        public Task<IEnumerable<T>> GetsAsync<T>(string spName, DynamicParameters parms)
        {

            return WithConnectionAsync(c =>
            {
                var result = c.QueryAsync<T>(
                    sql: spName,
                    param: parms,
                    commandType: CommandType.StoredProcedure, commandTimeout: 300);
                return result;
            });
        }

        /// <inheritdoc />
        public Task<int> InsertOrUpdateOrDeleteAsync(string spName, DynamicParameters param)
        {
            return WithConnectionAsync(c =>
            {
                var affectedRows = c.ExecuteScalarAsync<int>(spName, param,
                    commandTimeout: 300);

                return affectedRows;
            });
        }

        /// <inheritdoc />
        public Task<int> InsertOrUpdateOrDeleteAsync<U>(string spName, U entity)
        {
            return WithConnectionAsync(c =>
            {
                var affectedRows = c.ExecuteScalarAsync<int>(spName, entity, commandType: CommandType.StoredProcedure,
                    commandTimeout: 300);

                return affectedRows;
            });
        }

        private async Task<T> WithConnectionAsync<T>(Func<IDbConnection, Task<T>> getData)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return await getData(connection);
                }
            }
            catch (TimeoutException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL timeout {ex}");
            }
            catch (SqlException ex)
            {
                throw new Exception($"{GetType().FullName}.WithConnection() experienced a SQL exception (not a timeout) {ex}");
            }
        }
    }
    #endregion
}

