using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ww.Utilities.Extensions;

namespace ww.Tables
{
    public abstract class DatabaseConnection : IDisposable
    {
        #region Constants
        protected const int DefaultTimeoutInSeconds = 30;
        protected const char UnicodeReplacementChar = '?';
        #endregion

        #region Connection
        public bool IsGlobal { get; internal set; }
        public abstract void Connect();
        protected abstract void CloseConnection();
        #endregion

        #region Unlocked Data
        protected abstract IDbCommand GetCommand(string query, int timeoutInSeconds);
        protected IDbCommand GetCommand(string query, object anonTypeParameters, int timeoutInSeconds)
        {
            IDbCommand cmd = GetCommand(query, timeoutInSeconds);
            if (anonTypeParameters != null)
            {
                foreach (PropertyInfo pi in anonTypeParameters.GetType().GetProperties())
                {
                    cmd.Parameters.Add(GetParameter(pi.Name, pi.GetValue(anonTypeParameters), SqlStringDataType.Default));
                }
            }
            return cmd;
        }
        protected IDbCommand GetCommand(string query, IEnumerable<IDbDataParameter> parameters, int timeoutInSeconds)
        {
            IDbCommand cmd = GetCommand(query, timeoutInSeconds);
            if (parameters != null)
            {
                foreach (IDbDataParameter parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
            }
            return cmd;
        }
        protected abstract IDbCommand GetCommand(string query, IEnumerable<OleDbParameter> parameters, int timeoutInSeconds);
        protected IDbCommand GetCommandInterpolated(FormattableString query, int timeoutInSeconds)
        {
            object[] args = query.GetArguments();
            IDbCommand cmd = GetCommand(string.Format(query.Format, Enumerable.Range(1, args.Length).Select(index => $"{NamedParameterChar}p{index}").ToArray()), timeoutInSeconds);
            for (int index = 1; index <= args.Length; index++)
            {
                cmd.Parameters.Add(GetParameter($"{NamedParameterChar}p{index}", args[index - 1], SqlStringDataType.Default));
            }
            return cmd;
        }

        public DataTable GetData(string query, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            return GetData(GetCommand(query, timeoutInSeconds));
        }
        public DataTable GetData(string query, object anonTypeParameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            return GetData(GetCommand(query, anonTypeParameters, timeoutInSeconds));
        }
        internal DataTable GetData(string query, IEnumerable<IDbDataParameter> parameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            return GetData(GetCommand(query, parameters, timeoutInSeconds));
        }
        public DataTable GetData(string query, IEnumerable<OleDbParameter> parameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            return GetData(GetCommand(query, parameters, timeoutInSeconds));
        }
        public DataTable GetDataInterpolated(FormattableString query, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            return GetData(GetCommandInterpolated(query, timeoutInSeconds));
        }
        public async Task<DataTable> GetDataInterpolatedAsync(FormattableString query, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            return await Task.Run(() => GetData(GetCommandInterpolated(query, timeoutInSeconds)));
        }
        protected abstract DataTable GetData(IDbCommand cmd);

        public DbDataReader GetDataReader(string query, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            return GetDataReader(GetCommand(query, timeoutInSeconds));
        }
        public DbDataReader GetDataReader(string query, object anonTypeParameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            return GetDataReader(GetCommand(query, anonTypeParameters, timeoutInSeconds));
        }
        internal DbDataReader GetDataReader(string query, IEnumerable<IDbDataParameter> parameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            return GetDataReader(GetCommand(query, parameters, timeoutInSeconds));
        }
        public DbDataReader GetDataReader(string query, IEnumerable<OleDbParameter> parameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            return GetDataReader(GetCommand(query, parameters, timeoutInSeconds));
        }
        public DbDataReader GetDataReaderInterpolated(FormattableString query, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            return GetDataReader(GetCommandInterpolated(query, timeoutInSeconds));
        }
        protected abstract DbDataReader GetDataReader(IDbCommand cmd);

        public void UpdateData(string query, DataTable dt, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            UpdateData(GetCommand(query, timeoutInSeconds), dt, out _);
        }
        public void UpdateData(string query, DataTable dt, object anonTypeParameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            UpdateData(GetCommand(query, anonTypeParameters, timeoutInSeconds), dt, out _);
        }
        internal void UpdateData(string query, DataTable dt, IEnumerable<IDbDataParameter> parameters, out int autoNumber, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            UpdateData(GetCommand(query, parameters, timeoutInSeconds), dt, out autoNumber);
        }
        public void UpdateData(string query, DataTable dt, IEnumerable<OleDbParameter> parameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            UpdateData(GetCommand(query, parameters, timeoutInSeconds), dt, out _);
        }
        public void UpdateDataInterpolated(FormattableString query, DataTable dt, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            UpdateData(GetCommandInterpolated(query, timeoutInSeconds), dt, out _);
        }
        protected abstract void UpdateData(IDbCommand cmd, DataTable dt, out int autoNumber);

        public void DeleteData(string query, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            DeleteData(GetCommand(query, timeoutInSeconds));
        }
        public void DeleteData(string query, object anonTypeParameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            DeleteData(GetCommand(query, anonTypeParameters, timeoutInSeconds));
        }
        public void DeleteData(string query, IEnumerable<OleDbParameter> parameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            DeleteData(GetCommand(query, parameters, timeoutInSeconds));
        }
        public void DeleteDataInterpolated(FormattableString query, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            DeleteData(GetCommandInterpolated(query, timeoutInSeconds));
        }
        protected abstract void DeleteData(IDbCommand cmd);

        public void ExecuteCommand(string query, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            ExecuteCommand(GetCommand(query, timeoutInSeconds));
        }
        public void ExecuteCommand(string query, object anonTypeParameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            ExecuteCommand(GetCommand(query, anonTypeParameters, timeoutInSeconds));
        }
        public void ExecuteCommand(string query, IEnumerable<OleDbParameter> parameters, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            ExecuteCommand(GetCommand(query, parameters, timeoutInSeconds));
        }
        public void ExecuteCommandInterpolated(FormattableString query, int timeoutInSeconds = DefaultTimeoutInSeconds)
        {
            ExecuteCommand(GetCommandInterpolated(query, timeoutInSeconds));
        }
        protected abstract void ExecuteCommand(IDbCommand cmd);
        #endregion

        #region Miscellaneous
        public abstract char NamedParameterChar { get; }
        public abstract IDbDataParameter GetParameter(string name, object value, SqlStringDataType sqlDbType);
        public virtual int GetNextCustomAutoNumber(int autoNumberType) { return 0; }
        public abstract IEnumerable<string> GetLockingUsers(string table, string lockedQuery = null, List<IDbDataParameter> parameters = null);
        public abstract void DisconnectAllUsers();
        #endregion

        #region Update Errors
        //protected abstract void HandleUpdateErrors(object sender, RowUpdatedEventArgs e);

        internal string ParseCommand(IDbCommand cmd)
        {
            string update = cmd.CommandText;
            var parameters = new Dictionary<string, Tuple<string, string>>();
            foreach (DbParameter param in cmd.Parameters.Cast<DbParameter>().Reverse())
            {
                var sourceColumn = param.SourceColumn;
                if (parameters.ContainsKey(sourceColumn))
                {
                    parameters[sourceColumn] = Tuple.Create(parameters[sourceColumn].Item1.Trim(), DisplayParameter(param).Trim());
                }
                else
                {
                    parameters.Add(sourceColumn, Tuple.Create(DisplayParameter(param).Trim(), string.Empty));
                }
                update = update.Replace(NamedParameterChar + param.ParameterName.Replace(NamedParameterChar.ToString(), string.Empty), DisplayParameter(param));
            }
            string changed = parameters.Reverse()
                .Where(x => x.Value.Item1 != x.Value.Item2)
                .Select(x => $"{x.Key}: {x.Value.Item1} -> {x.Value.Item2}")
                .Join("     \n");
            return $"{changed}\n\n{update}";
        }

        internal string DisplayParameter(DbParameter param)
        {
            if (param.Value == DBNull.Value) return "null";
            switch (param.DbType)
            {
                case DbType.AnsiString:
                case DbType.Binary:
                case DbType.DateTime:
                case DbType.Guid:
                case DbType.String:
                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                case DbType.Xml:
                    return $"'{param.Value}'";
                case DbType.Byte:
                case DbType.Boolean:
                case DbType.Currency:
                case DbType.Decimal:
                case DbType.Double:
                case DbType.Int16:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Object:
                case DbType.SByte:
                case DbType.Single:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.VarNumeric:
                    return param.Value.ToString();
                case DbType.Date:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return $"'{DateTime.Parse(param.Value.ToString()).ToShortDateString()}'";
                case DbType.Time:
                    return $"'{DateTime.Parse(param.Value.ToString()).ToShortTimeString()}'";
                default:
                    throw new ArgumentException(param.DbType.ToString());
            }
        }
        internal string DisplayParameter(DbParameter param, DataRow row)
        {
            return $"{param.ParameterName}[{param.SourceColumn}]: {param.Value}{(row.Table.Columns.Contains(param.SourceColumn) ? (param.Direction == ParameterDirection.Input ? $" <-- {row[param.SourceColumn, DataRowVersion.Current]}" : $" --> {row[param.SourceColumn, DataRowVersion.Original]}") : "UNKNOWN COLUMN")}";
        }

        internal IEnumerable<string> DisplayRow(DataRow row)
        {
            foreach (DataColumn col in row.Table.Columns)
            {
                yield return row.HasVersion(DataRowVersion.Proposed)
                    ? $"{row[col, DataRowVersion.Original]} -> {row[col, DataRowVersion.Current]} -> {row[col, DataRowVersion.Proposed]}"
                    : $"{row[col, DataRowVersion.Original]} -> {row[col, DataRowVersion.Current]}";
            }
        }
        #endregion

        public abstract void Dispose();
    }
    public abstract class DatabaseConnection<TDatabaseEnum, TRawConnection, TData> : DatabaseConnection
    {
        #region Connection
        protected TRawConnection Connection { get; set; }

        protected abstract TDatabaseEnum _databaseToUse { get; set; }
        public TDatabaseEnum DatabaseInUse
        {
            get => _databaseToUse;
            set
            {
                _databaseToUse = value;
                if (Connection != null) CloseConnection();
                Connect();
            }
        }

        private string _serverName;
        public string ServerName
        {
            get => _serverName;
            set
            {
                _serverName = value;
                if (Connection != null) CloseConnection();
                Connect();
            }
        }
        #endregion

        #region Locked Data
        public abstract TData GetDataAndLock(string query, List<IDbDataParameter> paramaters = null);
        public abstract void UnlockWithoutUpdatingData(TData data);
        public abstract void UpdateDataAndUnlock(TData data);
        public abstract void DeleteLockedData(TData data);
        protected abstract void LockRecords(TData data);
        protected abstract void UnlockRecords(TData data);
        #endregion
    }
}
