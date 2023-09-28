using Microsoft.Extensions.Options;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

namespace MachineUtilizationApi.Repository
{
    public abstract class BaseRepository
    {
        private readonly ConnectionStringSettings _settings;
        private readonly ILogger<BaseRepository> _logger;

        protected BaseRepository(IOptions<ConnectionStringSettings> settings, ILogger<BaseRepository> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public abstract string ConnectionString { get; }

        protected ConnectionStringSettings Settings => _settings;

        protected ILogger<BaseRepository> Logger => _logger;

        public virtual SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        public virtual SqlCommand GetSqlCommand(string commandText, SqlConnection conn)
        {
            return new SqlCommand(commandText, conn);
        }

        public virtual SqlCommand GetSqlStoredProcCommand(string commandText, SqlConnection conn)
        {
            SqlCommand command = GetSqlCommand(commandText, conn);
            command.CommandType = CommandType.StoredProcedure;

            return command;
        }

        public virtual async Task<DataTable> LoadDataTableFromStoredProcAsync(string procName, IDictionary<string, object> parameters, int? timeout = null)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                try
                {
                    Logger.LogInformation($"LoadDataTableFromStoredProcAsync: Calling proc '{procName}'");
                    if (timeout != null)
                    {
                        cmd.CommandTimeout = timeout.Value;
                    }

                    Logger.LogInformation($"LoadDataTableFromStoredProcAsync: Proc '{procName}' setting parameters ");
                    SetParameters(cmd, parameters);

                    await conn.OpenAsync();

                    using (IDataReader reader = await ExecuteReaderWithLogAsync(cmd))
                    {
                        Logger.LogInformation($"LoadDataTableFromStoredProcAsync: Proc '{procName}' Loading datatable");
                        dt.Load(reader, LoadOption.OverwriteChanges);
                    }

                    Logger.LogInformation($"LoadDataTableFromStoredProcAsync: Proc '{procName}' done. Data Table results: {dt.Rows.Count} ");
                }
                catch (Exception e)
                {
                    Logger.LogError(e, $"LoadDataTableFromStoredProcAsync: Proc '{procName}' with exception. {e.Message} ");
                    throw;
                }
            }

            return dt;
        }

        public virtual Task<DataSet> LoadDataSetFromStoredProcAsync(string procName, IDictionary<string, object> parameters, params string[] tables)
        {
            return LoadDataSetFromStoredProcAsync(procName, parameters, null, tables);
        }

        public virtual async Task<DataSet> LoadDataSetFromStoredProcAsync(string procName, IDictionary<string, object> parameters, int? timeout = null, params string[] tables)
        {
            DataSet ds = new DataSet();
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                SetParameters(cmd, parameters);
                cmd.CommandTimeout = timeout.HasValue ? timeout.Value : cmd.CommandTimeout;
                await conn.OpenAsync();
                using (IDataReader reader = await ExecuteReaderWithLogAsync(cmd))
                {
                    ds.Load(reader, LoadOption.OverwriteChanges, tables);
                }
            }

            return ds;
        }

        public virtual async Task<object> LoadObjectFromTextAsync(string commandText, IDictionary<string, object> parameters = null)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = GetSqlCommand(commandText, conn))
            {
                SetParameters(cmd, parameters);
                await conn.OpenAsync();
                return await ExecuteScalarWithLogAsync(cmd);
            }
        }

        public virtual async Task<T> ExecuteStoredProcScalarAsync<T>(string procName, IDictionary<string, object> parameters)
        {
            T retVal;
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                SetParameters(cmd, parameters);
                await conn.OpenAsync();
                object rawReturnValue = await ExecuteScalarWithLogAsync(cmd);
                try
                {
                    retVal = (T)rawReturnValue;
                }
                catch (InvalidCastException)
                {
                    retVal = (T)Convert.ChangeType(rawReturnValue, typeof(T));
                }
            }

            return retVal;
        }

        public virtual async Task<T> ExecuteStoredProcWithResultAsync<T>(string procName, IDictionary<string, object> parameters)
        {
            const string ReturnValueParamName = "@ReturnValue";
            T retVal;
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                SetParameters(cmd, parameters);
                SqlParameter outputParam = cmd.CreateParameter();
                outputParam.ParameterName = ReturnValueParamName;
                outputParam.Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(outputParam);

                await conn.OpenAsync();
                await ExecuteScalarWithLogAsync(cmd);

                object rawReturnValue = cmd.Parameters[ReturnValueParamName].Value;
                try
                {
                    retVal = (T)rawReturnValue;
                }
                catch (InvalidCastException)
                {
                    retVal = (T)Convert.ChangeType(rawReturnValue, typeof(T));
                }
            }
            return retVal;
        }

        public virtual async Task ExecuteStoredProcWithoutOutputAsync(string procName,
            IDictionary<string, object> parameters, int? timeout = null, CancellationToken cancellationToken = default)
        {

            using (var conn = GetConnection())
            using (var cmd = GetSqlStoredProcCommand(procName, conn))
            {
                SetParameters(cmd, parameters);
                await conn.OpenAsync(cancellationToken);
                await ExecuteNonQueryWithLogAsync(cmd, cancellationToken);
            }

        }

        public virtual async Task<int> ExecuteStoredProcWithoutResultsAsync(string procName, IDictionary<string, object> parameters)
        {
            int rowAffected = 0;
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                SetParameters(cmd, parameters);
                await conn.OpenAsync();
                rowAffected = await ExecuteNonQueryWithLogAsync(cmd);
            }
            return rowAffected;
        }

        public virtual async Task<int> ExecuteStoredProcWithScalarOuputAsync(string procName, IDictionary<string, object> parameters)
        {
            int rowAffected = 0;
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                SetParameters(cmd, parameters);
                await conn.OpenAsync();
                rowAffected = await ExecuteNonQueryWithLogAsync(cmd);
            }
            return rowAffected;
        }

        public virtual async Task<IDictionary<string, object>> ExecuteStoredProcOutputParametersAsync(string procName, IDictionary<string, object> inputParameters, IDictionary<string, object> outputParameters)
        {
            IDictionary<string, object> parametersReturn = new Dictionary<string, object>();
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = GetSqlStoredProcCommand(procName, conn))
            {
                SetParameters(cmd, inputParameters);
                SetOutputParameters(cmd, outputParameters);
                await conn.OpenAsync();
                int rowAffected = await ExecuteNonQueryWithLogAsync(cmd);
                foreach (KeyValuePair<string, object> item in outputParameters)
                {
                    parametersReturn.Add(new KeyValuePair<string, object>(item.Key, cmd.Parameters[item.Key].Value));
                }
                parametersReturn.Add(new KeyValuePair<string, object>("rowAffected", rowAffected));//Add to return dictionary, the quantity of affected rows.
            }
            return parametersReturn;
        }

        protected virtual void SetParameters(SqlCommand cmd, IDictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    if (param.Value is SqlParameter)
                    {
                        cmd.Parameters.Add(param.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }
                }
            }
        }

        protected virtual void SetOutputParameters(SqlCommand cmd, IDictionary<string, object> parameters)
        {
            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    SqlParameter parameter = new SqlParameter(param.Key, param.Value ?? DBNull.Value)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        public virtual DataTable InitListTVP<T>(string columnName, bool allowNull = false)
        {
            DataTable tvp = new DataTable();
            tvp.Columns.Add(new DataColumn(columnName, typeof(T))
            {
                AllowDBNull = allowNull
            });

            return tvp;
        }

        public virtual DataTable LoadTVPFromList<TDataType>(string columnName, IEnumerable<TDataType> list)
        {
            DataTable tvp = InitListTVP<TDataType>(columnName);

            foreach (TDataType item in list)
            {
                DataRow dr = tvp.NewRow();
                dr[columnName] = item;
                tvp.Rows.Add(dr);
            }

            return tvp;
        }

        public virtual DataTable CreateListTVP(Dictionary<string, Type> columns)
        {
            DataTable tvp = new DataTable();
            foreach (KeyValuePair<string, Type> column in columns)
            {
                tvp.Columns.Add(new DataColumn(column.Key, column.Value)
                {
                    AllowDBNull = true
                });
            }
            return tvp;
        }

        #region Logging Functions

        /// <summary>
        /// Logs a stored proc command along with its parameter values and elapsed time
        /// </summary>
        /// <param name="cmd">The command to be logged</param>
        /// <returns>The data reader object resulting from the command</returns>
        protected virtual async Task<IDataReader> ExecuteReaderWithLogAsync(SqlCommand cmd)
        {
            return await ExecuteActionWithLogAsync<IDataReader>(cmd, async c => await c.ExecuteReaderAsync());
        }

        /// <summary>
        /// Logs a scalar query along with its parameter values and elapsed time
        /// </summary>
        /// <param name="cmd">The command to be logged</param>
        /// <returns>The object resulting from the command</returns>
        protected virtual async Task<object> ExecuteScalarWithLogAsync(SqlCommand cmd)
        {
            return await ExecuteActionWithLogAsync<object>(cmd, async c => await c.ExecuteScalarAsync());
        }

        /// <summary>
        /// Logs a non query along with its parameter values and elapsed time
        /// </summary>
        /// <param name="cmd">The command to be logged</param>
        /// <returns>The affected row count resulting from the command</returns>
        protected virtual async Task<int> ExecuteNonQueryWithLogAsync(SqlCommand cmd, CancellationToken cancellationToken = default)
        {
            return await ExecuteActionWithLogAsync<int>(cmd, async c => await c.ExecuteNonQueryAsync(cancellationToken));
        }

        /// <summary>
        /// Base function for logging SQL Commands
        /// </summary>
        /// <typeparam name="T">The type of object returned from the action</typeparam>
        /// <param name="cmd">The command to be logged and executed</param>
        /// <param name="action">The action to take on the command</param>
        /// <returns>The result of the action parameter</returns>
        protected virtual async Task<T> ExecuteActionWithLogAsync<T>(SqlCommand cmd, Func<SqlCommand, Task<T>> action)
        {
            StringBuilder logEntry = new StringBuilder();
            Stopwatch stopWatch = new Stopwatch();
            try
            {
                logEntry.AppendLine($"Executing proc {cmd.CommandText}");
                

                // Note that we aren't checking the cmd's type to ensure its a stored proc. We want all of our calls to be through stored procs though
                // so a command that isn't logged in a way that can be easily executed isn't an issue.
                LogSqlProcCommand(logEntry, cmd);

                stopWatch.Start();

                T retVal = await action(cmd);

                stopWatch.Stop();

                logEntry.AppendLine($"Executed in {stopWatch.ElapsedMilliseconds} ms");

                WriteLogEntry(logEntry);

                return retVal;
            }
            catch (Exception ex)
            {
                logEntry.AppendLine($"Execution failed in {stopWatch.ElapsedMilliseconds} ms: ({ex.Message})");

                Logger.LogError(ex, logEntry.ToString());

                throw;
            }
            finally
            {
                stopWatch.Stop();
            }
        }

        /// <summary>
        /// Writes the log entry.
        /// </summary>
        /// <param name="logEntry">The log entry message to write</param>
        private void WriteLogEntry(StringBuilder logEntry)
        {
            Logger.LogInformation(logEntry.ToString());
        }

        /// <summary>
        /// Logs the command executed in SQL Server
        /// </summary>
        /// <param name="logEntry">The StringBuilder containing the current log entry</param>
        /// <param name="cmd">The command to be logged</param>
        protected virtual void LogSqlProcCommand(StringBuilder logEntry, SqlCommand cmd)
        {
            logEntry.AppendLine($"exec {cmd.CommandText} {string.Join(", ", cmd.Parameters.OfType<SqlParameter>().Where(p => p.Direction != ParameterDirection.ReturnValue).Select(p => FormatSqlParam(p)))}");
        }

        /// <summary>
        /// Formats the parameter expression for the command in the log
        /// </summary>
        /// <param name="param">The parameter to be logged</param>
        /// <returns>The parameter name and value string to be used in the command.</returns>
        protected virtual string FormatSqlParam(SqlParameter param)
        {
            if (param.SqlDbType == SqlDbType.Structured)
            {
                return $"{PrefixParamName(param.ParameterName)} = {PrefixParamName(param.ParameterName)}";
            }

            string paramText = $"{PrefixParamName(param.ParameterName)} = {FormatSqlParamValue(param.Value, param.Value.GetType())}";
            switch (param.Direction)
            {
                case ParameterDirection.InputOutput:
                case ParameterDirection.Output:
                    paramText += " OUTPUT";
                    break;
            }

            return paramText;
        }


        /// <summary>
        /// Formats the parameter value to be added to a SQL command string
        /// </summary>
        /// <param name="parameterValue">The value of the parameter</param>
        /// <param name="dataType">The type of the parameter</param>
        /// <returns>The parameter value formatted as a string for use in SQL Server</returns>
        protected virtual string FormatSqlParamValue(object parameterValue, Type dataType)
        {
            switch (dataType.Name)
            {
                case nameof(Int64):
                case nameof(Int32):
                case nameof(Int16):
                case nameof(Byte):
                case nameof(Decimal):
                case nameof(Double):
                    return parameterValue?.ToString() ?? "null";

                case nameof(Boolean):
                    return GetSqlFormatFromBooleanType(parameterValue);

                case nameof(DateTime):
                    return $"'{(DateTime)parameterValue:s}'";

                case nameof(DateTimeOffset):
                    return $"'{(DateTimeOffset)parameterValue:o}'";

                case nameof(DBNull):
                    return "null";

                default:
                    return $"N'{parameterValue?.ToString().Replace("'", "''")}'" ?? "null";
            }
        }

        private static string GetSqlFormatFromBooleanType(object parameterValue)
        {
            bool? parameterValueBoolean = (bool?)parameterValue;
            string formattedValue = "null";
            if (parameterValueBoolean.Value)
            {
                formattedValue = "1";
            }
            else if (!parameterValueBoolean.Value)
            {
                formattedValue = "0";
            }

            return formattedValue;
        }

        

        /// <summary>
        /// Adds the '@' to the start of the parameter name if not specified.
        /// </summary>
        /// <param name="parameterName">The parameter name to add the '@' to.</param>
        /// <returns>The parameter name with the '@' prefix</returns>
        protected virtual string PrefixParamName(string parameterName)
        {
            return parameterName[0] == '@' ? parameterName : $"@{parameterName}";
        }

        #endregion Logging Functions
    }
}