using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using ww.Utilities;
using ww.Utilities.Extensions;

namespace ww.Tables
{
    public class SqlServerConnection : DatabaseConnection<Database.SqlServerDatabase, SqlConnection, SqlServerData>
    {
        #region Constants
        private const int RetryCountConnect = 3;
        private const int RetryCountGetData = 10;
        private const int RetryCountLockRecord = 5;
        private const int RetryCountUpdateWithoutLock = 10;
        private const int RetryCountTimeout = 1;
        private const int RetryInterval = 1500; // miliseconds
        private const string LockingClause = "with (rowlock, updlock)";
        #endregion

        #region Constructors
        public SqlServerConnection(Database.SqlServerDatabase? database = null)
        {
            DatabaseInUse = database ?? Global.SqlServerDatabaseInUse;
        }

        public override void Dispose()
        {
            this.CloseConnection();
            this.Connection.Dispose();
        }
        #endregion

        #region Connection
        protected override Database.SqlServerDatabase _databaseToUse { get; set; }

        private string GetIntegratedSecurityConnectionString(string database, string server)
        {
            return $"data source={ServerName ?? server};initial catalog={database};persist security info=False;Integrated Security=SSPI;workstation id={Environment.MachineName};packet size=4096;MultipleActiveResultSets=true;Max Pool Size=10000";
        }

        public override void Connect()
        {
            int counter = 0;
            do
            {
                switch (_databaseToUse)
                {
                    case Database.SqlServerDatabase.Production:
                        Connection = new SqlConnection(GetIntegratedSecurityConnectionString("Production", server: "EXAMPLE"));
                        break;
                    case Database.SqlServerDatabase.Test:
                        Connection = new SqlConnection(GetIntegratedSecurityConnectionString("Test", server: "EXAMPLE"));
                        break;
                    default:
                        throw new ArgumentException(_databaseToUse.ToString());
                }

                try
                {
                    Connection.Open();
                    return;
                }
                catch (Exception ex)
                {
                    Connection.Dispose();
                    counter++;
                    if (counter >= RetryCountConnect)
                    {
                        Email.sendAlertEmail("Failed to open database connection", $"PC: {Environment.MachineName}\nUser: {Environment.UserName}\nProcess: {Process.GetCurrentProcess().MainModule.FileName}\n\nAttempting to open database connection to database {_databaseToUse} failed: {ex}", MailPriority.High, sendInThread: false);
                        return;
                    }
                    Thread.Sleep(RetryInterval);
                }
            } while (true);
        }

        /// <summary>
        /// DO NOT USE outside of this class! It is a waste of resources to close and reopen connections.
        /// </summary>
        protected override void CloseConnection()
        {
            if (Connection?.State == ConnectionState.Open)
            {
                Connection.Close();
            }
        }
        #endregion

        #region Unlocked Data
        protected override IDbCommand GetCommand(string query, int timeoutInSeconds)
        {
            return new SqlCommand(query, Connection) { CommandTimeout = timeoutInSeconds };
        }

        protected override IDbCommand GetCommand(string query, IEnumerable<OleDbParameter> parameters, int timeoutInSeconds)
        {
            var cmd = new SqlCommand(query, Connection) { CommandTimeout = timeoutInSeconds };
            if (parameters != null)
            {
                foreach (OleDbParameter parameter in parameters)
                {
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = parameter.ParameterName,
                        Value = parameter.Value,
                        SqlDbType = (SqlDbType)parameter.DbType,
                        Size = parameter.Size,
                        Precision = parameter.Precision,
                        Scale = parameter.Scale,
                        IsNullable = parameter.IsNullable,
                        SourceColumn = parameter.SourceColumn,
                        SourceVersion = parameter.SourceVersion,
                        Direction = parameter.Direction,
                    });
                }
            }
            return cmd;
        }

        protected override DataTable GetData(IDbCommand cmd)
        {
            SqlDataAdapter da = null;
            int counter = 0;
            do
            {
                try
                {
                    if (Connection.State == ConnectionState.Closed)
                    {
                        Connect();
                        cmd.Connection = Connection;
                    }

                    da = new SqlDataAdapter((SqlCommand)cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    da.Dispose();
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                    return dt;
                }
                catch (SqlException ex)
                {
                    HandleGetDataException(ex, ref counter, cmd, da);
                }
            } while (true);
        }

        public override int GetNextCustomAutoNumber(int autoNumberType)
        {
            return SqlServerCustomAutoNumber.GetNextCustomAutoNumber(new SqlServerConnection(), (SqlServerCustomAutoNumberType)autoNumberType);
        }

        protected override DbDataReader GetDataReader(IDbCommand cmd)
        {
            int counter = 0;
            do
            {
                try
                {
                    if (Connection.State == ConnectionState.Closed)
                    {
                        Connect();
                        cmd.Connection = Connection;
                    }

                    DbDataReader dr = ((SqlCommand)cmd).ExecuteReader();
                    cmd.Dispose();
                    return dr;
                }
                catch (SqlException ex)
                {
                    HandleGetDataException(ex, ref counter, cmd, null);
                }
            } while (true);
        }

        protected override void UpdateData(IDbCommand cmd, DataTable dt, out int autoNumber)
        {
            if (Connection.State == ConnectionState.Closed)
            {
                Connect();
                cmd.Connection = Connection;
            }

            autoNumber = 0;
            SqlDataAdapter da = null;
            SqlDataAdapter daUpdate = null;
            SqlCommandBuilder cb = null;
            bool ok2cont = true;
            int counter = 0;
            do
            {
                try
                {
                    da = new SqlDataAdapter((SqlCommand)cmd);
                    cb = new SqlCommandBuilder() { DataAdapter = da };
                    da.InsertCommand = cb.GetInsertCommand();
                    da.InsertCommand.CommandText += "; select @AutoNumber = scope_identity();";
                    da.InsertCommand.Parameters.Add(new SqlParameter { ParameterName = "AutoNumber", Direction = ParameterDirection.Output, SqlDbType = SqlDbType.Int });
                    da.UpdateCommand = cb.GetUpdateCommand();
                    da.DeleteCommand = cb.GetDeleteCommand();

                    // Output parameters are removed from a DataAdapter when using a CommandBuilder.
                    // Using a new DataAdapter here to avoid this.
                    daUpdate = new SqlDataAdapter()
                    {
                        InsertCommand = da.InsertCommand,
                        UpdateCommand = da.UpdateCommand,
                        DeleteCommand = da.DeleteCommand
                    };
                    daUpdate.Update(dt);
                    if (dt.Rows.Count == 1)
                    {
                        autoNumber = daUpdate.InsertCommand.Parameters["AutoNumber"].Value.ToInt();
                    }

                    da.Dispose();
                    daUpdate.Dispose();
                    cb.Dispose();
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                    ok2cont = false;
                }
                catch (SqlException ex)
                {
                    string query = string.Empty;
                    List<IDbDataParameter> parameters = null;
                    da?.Dispose();
                    daUpdate?.Dispose();
                    cb?.Dispose();
                    if (cmd != null)
                    {
                        query = cmd.CommandText;
                        parameters = cmd.Parameters.Cast<IDbDataParameter>().ToList();
                    }

                    int retryCount = RetryCountUpdateWithoutLock;
                    HandleExceptionBeforeRetry(ex, (SqlServerException.ErrorNumberEnum)ex.Number, cmd, query, parameters, ref counter, ref retryCount);

                    if (counter > retryCount)
                    {
                        if (DatabaseException.ReattemptOnLockFailure(ex.Message, ex.Number, (int)SqlServerException.ErrorNumberEnum.LockFailed))
                        {
                            counter = 0;
                        }
                        else
                        {
                            if (cmd != null)
                            {
                                cmd.Parameters.Clear();
                                cmd.Dispose();
                            }
                            throw new SqlServerException($"Hung when saving data from query \"{query}\"{(parameters == null || parameters.Count == 0 ? string.Empty : $", parameters \"{parameters.Select(x => x.Value).Join("\", \"")}\"")}: {(ex.InnerException == null ? ex.Message : ex.InnerException.Message)}", ex);
                        }
                    }
                }
            } while (ok2cont);
        }

        protected override void DeleteData(IDbCommand cmd)
        {
            DataTable dt = GetData(cmd);
            foreach (DataRow row in dt.Rows)
            {
                row.Delete();
            }
            IDbDataParameter[] parameters = cmd.Parameters.Cast<IDbDataParameter>().ToArray();
            var newCmd = new SqlCommand(cmd.CommandText, Connection) { CommandTimeout = cmd.CommandTimeout };
            cmd.Parameters.Clear();
            cmd.Dispose();
            newCmd.Parameters.AddRange(parameters);
            UpdateData(newCmd, dt, out _);
            newCmd.Parameters.Clear();
            newCmd.Dispose();
        }

        protected override void ExecuteCommand(IDbCommand cmd)
        {
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                cmd.Dispose();
            }
            catch
            {
                if (cmd != null)
                {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                throw;
            }
        }
        #endregion

        #region Locked Data
        public override SqlServerData GetDataAndLock(string query, List<IDbDataParameter> parameters = null)
        {
            var data = new SqlServerData(this, query, parameters);
            LockRecords(data);
            return data;
        }

        public override void UnlockWithoutUpdatingData(SqlServerData data)
        {
            UnlockRecords(data);
        }

        public override void UpdateDataAndUnlock(SqlServerData data)
        {
            var commandBuilder = new SqlCommandBuilder();
            try
            {
                commandBuilder.DataAdapter = data.Adapter;
                data.Adapter.InsertCommand = commandBuilder.GetInsertCommand();
                data.Adapter.UpdateCommand = commandBuilder.GetUpdateCommand();
                data.Adapter.Update(data.Table);
                (data.Transaction ?? (data.Transaction = Connection.BeginTransaction())).Commit();
                data.IsLocked = false;

                commandBuilder.Dispose();
                data.Adapter.Dispose();
                data.Adapter = null;
                data.Transaction.Dispose();
                data.Transaction = null;
            }
            catch (SqlException ex)
            {
                commandBuilder.Dispose();
                if (data.Adapter != null)
                {
                    data.Adapter.Dispose();
                    data.Adapter = null;
                }
                if (data.Transaction != null)
                {
                    data.Transaction.Dispose();
                    data.Transaction = null;
                }

                if (((SqlServerException.ErrorNumberEnum)ex.Number).EqualsAnyOf(SqlServerException.ErrorNumberEnum.DataTruncationAvoided, SqlServerException.ErrorNumberEnum.DataTruncationAvoidedExtended))
                {
                    string columnName;
                    int? columnSize;
                    int indexOfStartOfTableName = ex.Message.IndexOf("table '", StringComparison.OrdinalIgnoreCase);
                    if (indexOfStartOfTableName == -1)
                    {
                        columnName = null;
                        columnSize = null;
                    }
                    else
                    {
                        indexOfStartOfTableName += 7;
                        int indexOfEndOfTableName = ex.Message.IndexOf("'", indexOfStartOfTableName, StringComparison.OrdinalIgnoreCase);
                        string tableName = ex.Message.Substring(indexOfEndOfTableName, indexOfEndOfTableName - indexOfStartOfTableName);

                        int indexOfStartOfColumnName = ex.Message.IndexOf("column '", StringComparison.OrdinalIgnoreCase) + 8;
                        int indexOfEndOfColumnName = ex.Message.IndexOf("'", indexOfStartOfColumnName, StringComparison.OrdinalIgnoreCase);
                        columnName = ex.Message.Substring(indexOfStartOfColumnName, indexOfEndOfColumnName - indexOfStartOfColumnName);
                        columnSize = this.GetData("select character_maximum_length from information_schema.columns where table_schema = schema_name() and table_name = @p1 and column_name = @p2", new[] { new SqlParameter("p1", tableName), new SqlParameter("p2", columnName) }).Rows[0][0].ToInt();
                    }

                    throw new SqlServerDataTruncationException(columnName, columnSize, columnName == null ? null : data.Table.Rows[0][columnName], ex);
                }
                throw;
            }
        }

        public override void DeleteLockedData(SqlServerData data)
        {
            foreach (DataRow row in data.Table.Rows)
            {
                row.Delete();
            }
            UpdateDataAndUnlock(data);
        }

        protected override void LockRecords(SqlServerData data)
        {
            if (Connection.State == ConnectionState.Closed) Connection.Open();

            SqlCommand cmd = null;
            int counter = 0;
            do
            {
                try
                {
                    int indexOfWhere = data.Query.IndexOf("where", StringComparison.OrdinalIgnoreCase);
                    string query = indexOfWhere == -1
                        ? $"{data.Query} {LockingClause}"
                        : $"{data.Query.Substring(0, indexOfWhere)}{LockingClause} {data.Query.Substring(indexOfWhere)}";

                    cmd = new SqlCommand($"set lock_timeout 1000; {query}", Connection);
                    if (data.Parameters != null) cmd.Parameters.AddRange(data.Parameters.ToArray());
                    data.Transaction = this.Connection.BeginTransaction();
                    cmd.Transaction = data.Transaction;
                    data.Adapter = new SqlDataAdapter(cmd);
                    data.Table = new DataTable();
                    data.Adapter.Fill(data.Table);
                    data.IsLocked = data.Table.Rows.Count > 0;
                    if (!data.IsLocked)
                    {
                        cmd.Parameters.Clear(); // Allows us to readd the parameters to another SqlCommand.

                        // Even though there is nothing to lock, the transaction will prevent the record from being created unless it is rolled back.
                        data.Transaction.Rollback();
                        data.Transaction.Dispose();
                        data.Transaction = null;
                    }

                    return;
                }
                catch (SqlException ex)
                {
                    if (cmd != null)
                    {
                        cmd.Parameters.Clear();
                        cmd.Dispose();
                    }
                    if (data.Transaction != null)
                    {
                        data.Transaction.Rollback();
                        data.Transaction.Dispose();
                        data.Transaction = null;
                    }
                    if (data.Adapter != null)
                    {
                        data.Adapter.Dispose();
                        data.Adapter = null;
                    }

                    int retryCount = RetryCountLockRecord;
                    HandleExceptionBeforeRetry(ex, (SqlServerException.ErrorNumberEnum)ex.Number, null, data.Query, data.Parameters, ref counter, ref retryCount);

                    if (counter > retryCount)
                    {
                        if (DatabaseException.ReattemptOnLockFailure(ex.Message, ex.Number, (int)SqlServerException.ErrorNumberEnum.LockFailed))
                        {
                            counter = 0;
                        }
                        else if (ex.Number == (int)SqlServerException.ErrorNumberEnum.LockFailed)
                        {
                            throw new SqlServerException($"Unable to secure lock. Record is already locked in query \"{data.Query}\"{(data.Parameters == null || data.Parameters.Count == 0 ? string.Empty : $", parameters \"{data.Parameters.Select(x => x.Value).Join("\", \"")}\"")}.", ex);
                        }
                        else
                        {
                            throw new SqlServerException(ex);
                        }
                    }
                }
            } while (true);
        }

        protected override void UnlockRecords(SqlServerData data)
        {
            if (data.Transaction != null)
            {
                data.Transaction.Rollback();
                data.Transaction.Dispose();
                data.Transaction = null;
                data.Adapter.Dispose();
                data.Adapter = null;
                data.IsLocked = false;
            }
        }
        #endregion

        #region Handle Exceptions
        private void HandleGetDataException(SqlException ex, ref int counter, IDbCommand cmd, DbDataAdapter da)
        {
            string query = string.Empty;
            List<IDbDataParameter> parameters = null;
            da?.Dispose();
            if (cmd != null)
            {
                query = cmd.CommandText;
                parameters = cmd.Parameters.Cast<IDbDataParameter>().ToList();
            }

            int retryCount = RetryCountGetData;
            HandleExceptionBeforeRetry(ex, (SqlServerException.ErrorNumberEnum)ex.Number, cmd, query, parameters, ref counter, ref retryCount);

            if (counter > retryCount)
            {
                if (cmd != null)
                {
                    cmd.Parameters.Clear();
                    cmd.Dispose();
                }
                throw new SqlServerException($"Hung when reading data from query \"{query}\"{(parameters == null || parameters.Count == 0 ? string.Empty : $", parameters \"{parameters.Select(x => x.Value).Join("\", \"")}\"")}: {(ex.InnerException == null ? ex.Message : ex.InnerException.Message)}", ex);
            }
        }

        private void HandleExceptionBeforeRetry(SqlException ex, SqlServerException.ErrorNumberEnum errorNumberEnum, IDbCommand cmd, string query, List<IDbDataParameter> parameters, ref int counter, ref int retryCount)
        {
            switch (errorNumberEnum)
            {
                case SqlServerException.ErrorNumberEnum.BadSqlStatement:
                case SqlServerException.ErrorNumberEnum.TableNotFound:
                case SqlServerException.ErrorNumberEnum.ColumnNotFound:
                case SqlServerException.ErrorNumberEnum.OrderByColumnFailed:
                    if (cmd != null)
                    {
                        cmd.Parameters.Clear();
                        cmd.Dispose();
                    }
                    throw new SqlServerException($"Invalid SQL statement in query \"{query}\"{(parameters == null || parameters.Count == 0 ? string.Empty : $", parameters \"{parameters.Select(x => x.Value).Join("\", \"")}\"")}: {ex.Message}", ex);
                case SqlServerException.ErrorNumberEnum.Timeout:
                    retryCount = RetryCountTimeout;
                    break;
                case SqlServerException.ErrorNumberEnum.KeyViolation:
                    retryCount = 0;
                    counter++;
                    return;
            }

            counter++;
            Thread.Sleep(RetryInterval);
        }
        #endregion

        #region Miscellaneous
        public override char NamedParameterChar { get { return '@'; } }

        public override IDbDataParameter GetParameter(string name, object value, SqlStringDataType sqlDbType)
        {
            var parameter = new SqlParameter(name, value);
            // DateTime2 datatype aligns better with the .NET DateTime https://stackoverflow.com/a/468096
            if (parameter.SqlDbType == SqlDbType.DateTime) { parameter.SqlDbType = SqlDbType.DateTime2; }
            switch (sqlDbType)
            {
                case SqlStringDataType.Char:
                    parameter.SqlDbType = SqlDbType.Char;
                    break;
                case SqlStringDataType.NChar:
                    parameter.SqlDbType = SqlDbType.NChar;
                    break;
                case SqlStringDataType.VarChar:
                    parameter.SqlDbType = SqlDbType.VarChar;
                    break;
                case SqlStringDataType.NVarChar:
                    parameter.SqlDbType = SqlDbType.NVarChar;
                    break;
            }
            return parameter;
        }

        public override IEnumerable<string> GetLockingUsers(string table, string lockedQuery = null, List<IDbDataParameter> parameters = null)
        {
            if (Connection.State == ConnectionState.Closed) Connection.Open();

            table = table.Trim();

            //Get users locking anything in table.
            const string lockingUsersTableQuery =
                @"SELECT * FROM (
                    SELECT
                        DTL.request_session_id,
                        DTL.resource_description,
                        (
                            SELECT OBJECT_NAME([object_id])
                            FROM sys.partitions
                            WHERE sys.partitions.hobt_id = DTL.resource_associated_entity_id
                        ) AS requested_object_name,
                        SP.loginame,
                        SESS.host_name
                    FROM
                        sys.dm_tran_locks DTL
                        INNER JOIN sys.sysprocesses SP ON DTL.request_session_id = SP.spid
                        INNER JOIN sys.dm_exec_sessions SESS ON DTL.request_session_id = SESS.session_id
                    WHERE
                        SP.dbid = DB_ID()
                        AND DTL.resource_type = 'KEY'
                ) AS temp
                WHERE
                    requested_object_name = @Table";
            DataTable lockingUsersTable = GetData(lockingUsersTableQuery, new { Table = table });

            List<string> results = new List<string>();
            if (lockingUsersTable.Rows.Count > 0)
            {
                if (lockedQuery.IsNullOrBlank())
                {
                    //If we don't have a specific query to check, report anyone with locks on that table.
                    foreach (DataRow row in lockingUsersTable.AsEnumerable())
                    {
                        AddLockingUserResult(results, row);
                    }
                    return results;
                }

                //Get the key columns for the table.
                const string keyColumnQuery =
                    @"SELECT COLUMN_NAME
                        FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                        WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + QUOTENAME(CONSTRAINT_NAME)), 'IsPrimaryKey') = 1
                        AND TABLE_NAME = @Table";
                DataTable keyColumnsTable = GetData(keyColumnQuery, new { Table = table });

                if (keyColumnsTable.Rows.Count > 0)
                {
                    List<string> keyColumnNames = keyColumnsTable.Rows.Cast<DataRow>().Select(x => x[0].ToString()).ToList();
                    List<object> ourKeyValues = new List<object>();
                    using (var cmd = new SqlCommand(lockedQuery.Replace(LockingClause, ""), Connection))
                    {
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters.ToArray());
                        }

                        DataRow ourRowValues = GetData(cmd).AsEnumerable().FirstOrDefault();
                        if (ourRowValues != null)
                        {
                            ourKeyValues = keyColumnNames.ConvertAll(x => ourRowValues[x]);
                        }
                    }

                    if (ourKeyValues.Count > 0)
                    {
                        foreach (DataRow row in lockingUsersTable.Rows)
                        {
                            using (var cmd = new SqlCommand($"select {keyColumnNames.Join(", ")} from {table} WHERE %%lockres%% = '{row["resource_description"].ToTrimmedString()}'", this.Connection))
                            {
                                DataRow lockedRowValues = GetData(cmd).AsEnumerable().FirstOrDefault();
                                if (lockedRowValues != null && ourKeyValues.SequenceEqual(keyColumnNames.ConvertAll(x => lockedRowValues[x])))
                                {
                                    AddLockingUserResult(results, row);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return results;
        }

        private void AddLockingUserResult(List<string> results, DataRow row)
        {
            string result = null;
            if (!row["loginame"].ToString().IsNullOrBlank()) result = $"{row["loginame"].ToTrimmedString()} on {row["host_name"].ToTrimmedString()}";
            if (result != null && !results.Contains(result)) results.Add(result);
        }

        public override void DisconnectAllUsers()
        {
            // https://stackoverflow.com/questions/7197574/script-to-kill-all-connections-to-a-database-more-than-restricted-user-rollback
            if (Connection.State == ConnectionState.Closed) Connection.Open();

            var command = new StringBuilder();
            command.AppendLine("DECLARE @kill varchar(8000) = ''; ");
            command.AppendLine("SELECT @kill = @kill + 'kill ' + CONVERT(varchar(5), session_id) + ';'");
            command.AppendLine("FROM sys.dm_exec_sessions");
            command.AppendLine($"WHERE database_id = db_id('{Connection.Database}') and session_id != @@SPID");
            command.AppendLine("EXEC(@kill);");

            ExecuteCommand(command.ToString());
        }
        #endregion
    }
}
