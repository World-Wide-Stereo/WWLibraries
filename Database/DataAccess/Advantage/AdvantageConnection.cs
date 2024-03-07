using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using Advantage.Data.Provider;
using AdvantageClientEngine;
using ww.Utilities;
using ww.Utilities.Extensions;

namespace ww.Tables
{
    public class AdvantageConnection : DatabaseConnection<Database.AdvantageDatabase, AdsConnection, AdvantageData>
    {
        #region Constants
        private const int RetryCountConnectionFailure = 5;
        private const int RetryCountGetData = 10;
        private const int RetryCountLockRecord = 5;
        private const int RetryCountUpdateWithoutLock = 10;
        private const int RetryCountTimeout = 1;
        private const int RetryInterval = 1500; // miliseconds
        #endregion

        #region Constructors
        public AdvantageConnection(Database.AdvantageDatabase? database = null)
        {
            this.DatabaseInUse = database == null ? Global.AdvantageDatabaseInUse : database.Value;
        }

        public override void Dispose()
        {
            this.CloseConnection();
            this.Connection.Dispose();
        }
        #endregion

        #region Connection
        protected override Database.AdvantageDatabase _databaseToUse { get; set; }

        public long ConnectionHandle
        {
            get { return this.Connection.ConnectionHandle.ToInt64(); }
        }

        private string GetConnectionString(string dbPath, string initialCatalog = "advantage.add")
        {
            return $@"data source=\\{ServerName ?? "server"}{dbPath}; Initial Catalog={initialCatalog}; User ID={Environment.UserName.ToLower()}; Password=password; ServerType=remote; TableType=CDX; CharType=ANSI; Pooling=True; Min Pool Size=0;";
        }

        public override void Connect()
        {
            switch (_databaseToUse)
            {
                case Database.AdvantageDatabase.Production:
                    this.Connection = new AdsConnection(GetConnectionString(@"\server\advantage"));
                    break;
                case Database.AdvantageDatabase.Test:
                    this.Connection = new AdsConnection(GetConnectionString(@"\server\advantage\test"));
                    break;
                default:
                    throw new ArgumentException(_databaseToUse.ToString());
            }

            try
            {
                this.Connection.Open();
            }
            catch (Exception ex)
            {
                Email.sendAlertEmail("Failed to open database connection", "PC: " + Environment.MachineName + "\nUser: " + Environment.UserName + "\nProcess: " + Process.GetCurrentProcess().MainModule.FileName + "\n\nAttempting to open database connection to database " + _databaseToUse + " failed: " + ex, MailPriority.High);
            }
        }

        /// <summary>
        /// DO NOT USE outside of this class! It is a waste of resources to close and reopen connections.
        /// </summary>
        protected override void CloseConnection()
        {
            if (this.Connection != null && this.Connection.State == ConnectionState.Open)
            {
                this.Connection.Close();
                AdsConnection.FlushConnectionPool(this.Connection.ConnectionString); // The connection isn't truly closed without this line. See the Advantage Data Architect's documentation on AdsConnection.Close.
            }
        }
        #endregion

        #region Unlocked Data
        protected override IDbCommand GetCommand(string query, int timeoutInSeconds)
        {
            return new AdsCommand(query, this.Connection) { CommandTimeout = timeoutInSeconds };
        }
        protected override IDbCommand GetCommand(string query, IEnumerable<OleDbParameter> parameters, int timeoutInSeconds)
        {
            var cmd = new AdsCommand(query, this.Connection) { CommandTimeout = timeoutInSeconds };
            if (parameters != null)
            {
                foreach (OleDbParameter parameter in parameters)
                {
                    cmd.Parameters.Add(new AdsParameter
                    {
                        ParameterName = parameter.ParameterName,
                        Value = parameter.Value,
                        DbType = parameter.DbType,
                        Size = parameter.Size == 0 ? -1 : parameter.Size,
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
            AdsDataAdapter da = null;
            int counter = 0;
            do
            {
                try
                {
                    if (this.Connection.State == ConnectionState.Closed) this.Connection.Open();

                    da = new AdsDataAdapter((AdsCommand)cmd);
                    var dt = new DataTable();
                    da.Fill(dt);
                    da.Dispose();
                    cmd.Dispose();
                    return dt;
                }
                catch (AdsException ex)
                {
                    IDbCommand exCmd = HandleGetDataException(ex, ref counter, cmd, da);
                    if (exCmd != null)
                    {
                        DataTable dt = GetData(exCmd);
                        exCmd.Dispose();
                        cmd.Dispose();
                        return dt;
                    }
                }
            } while (true);
        }

        protected override DbDataReader GetDataReader(IDbCommand cmd)
        {
            int counter = 0;
            do
            {
                try
                {
                    if (this.Connection.State == ConnectionState.Closed) this.Connection.Open();

                    return ((AdsCommand)cmd).ExecuteReader();
                }
                catch (AdsException ex)
                {
                    IDbCommand exCmd = HandleGetDataException(ex, ref counter, cmd, null);
                    if (exCmd != null)
                    {
                        cmd.Dispose();
                        return ((AdsCommand)exCmd).ExecuteReader();
                    }
                }
            } while (true);
        }

        protected override void UpdateData(IDbCommand cmd, DataTable dt, out int autoNumber)
        {
            if (this.Connection.State == ConnectionState.Closed) this.Connection.Open();

            autoNumber = 0;
            AdsDataAdapter da = null;
            AdsCommandBuilder cb = null;
            bool ok2cont = true;
            int counter = 0;
            do
            {
                try
                {
                    da = new AdsDataAdapter((AdsCommand)cmd);
                    cb = new AdsCommandBuilder();
                    cb.RequirePrimaryKey = false;
                    cb.DataAdapter = da;
                    da.InsertCommand = cb.GetInsertCommand();
                    da.UpdateCommand = cb.GetUpdateCommand();
                    da.RowUpdated += HandleUpdateErrors;
                    da.DeleteCommand = cb.GetDeleteCommand();
                    da.Update(dt);
                    if (dt.Rows.Count == 1)
                    {
                        autoNumber = da.InsertCommand.LastAutoinc;
                    }

                    da.Dispose();
                    cb.Dispose();
                    cmd.Dispose();
                    ok2cont = false;
                }
                catch (AdsException ex)
                {
                    string query = "";
                    List<IDbDataParameter> parameters = null;
                    if (da != null) da.Dispose();
                    if (cb != null) cb.Dispose();
                    if (cmd != null)
                    {
                        query = cmd.CommandText;
                        parameters = cmd.Parameters.Cast<IDbDataParameter>().ToList();
                    }

                    var errorNumberEnum = (AdvantageException.ErrorNumberEnum)ex.Number;
                    if (errorNumberEnum == AdvantageException.ErrorNumberEnum.UnicodeNotSupported)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            for (int j = 0; j < dt.Columns.Count; j++)
                            {
                                if (dt.Columns[j].DataType == typeof(string))
                                {
                                    dt.Rows[i][j] = dt.Rows[i][j].ToString().StripUnicode(replacementChar: UnicodeReplacementChar);
                                }
                            }
                        }
                        continue;
                    }

                    int retryCount = RetryCountUpdateWithoutLock;
                    HandleExceptionBeforeRetry(ex, errorNumberEnum, cmd, query, parameters, ref counter, ref retryCount);

                    if (counter > retryCount)
                    {
                        if (DatabaseException.ReattemptOnLockFailure(ex.Message, ex.Number, (int)AdvantageException.ErrorNumberEnum.LockFailed))
                        {
                            counter = 0;
                        }
                        else
                        {
                            if (cmd != null) cmd.Dispose();
                            HandleExceptionAfterAllRetries(ex, errorNumberEnum);
                            throw new AdvantageException("Hung when saving data from query \"" + query + "\"" + (parameters == null || parameters.Count == 0 ? "" : ", parameters \"" + parameters.Select(x => x.Value).Join("\", \"") + "\"") + ": " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message), ex);
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
            var newCmd = new AdsCommand(cmd.CommandText, this.Connection) { CommandTimeout = cmd.CommandTimeout };
            newCmd.Parameters.AddRange(cmd.Parameters.Cast<IDbDataParameter>().ToArray());
            cmd.Dispose();
            UpdateData(newCmd, dt, out _);
            newCmd.Dispose();
        }

        protected override void ExecuteCommand(IDbCommand cmd)
        {
            try
            {
                cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            catch
            {
                if (cmd != null) cmd.Dispose();
                throw;
            }
        }
        #endregion

        #region Locked Data
        public override AdvantageData GetDataAndLock(string query, List<IDbDataParameter> parameters = null)
        {
            var data = new AdvantageData(this, query, parameters);
            LockRecords(data);
            if (data.Reader.BOF) // Beginning of File. There were no records to lock.
            {
                data.Reader.Close();
                data.Reader.Dispose();
                data.Reader = null;
            }
            data.Table = GetData(query, parameters); // When there are no records, this simply populates the DataTable with the proper columns.
            return data;
        }

        public override void UnlockWithoutUpdatingData(AdvantageData data)
        {
            UnlockRecords(data);
        }

        public void UpdateDataAndUnlock_DatabaseTable(AdvantageData data)
        {
            // For use only by the DatabaseTable class.
            // This function cannot be used with code that has directly manipulated the data object because data.Table may
            // no longer line up with data.Reader. For example, the two become out of alignment when a row is deleted from data.Table.
            data.Reader.GotoBOF();
            for (int i = 0; i < data.Table.Rows.Count; i++)
            {
                data.Reader.Read();
                for (int j = 0; j < data.Table.Columns.Count; j++)
                {
                    // Records are not written to the database until data.Reader is unlocked.
                    // No need for the if statement directly below as it has no discernible effect on execution time.
                    //if (!data.Reader.GetValue(j).Equals(data.Table.Rows[i][j]))
                    //{
                        try
                        {
                            data.Reader.SetValue(j, data.Table.Rows[i][j]);
                        }
                        catch (AdsException ex)
                        {
                            switch ((AdvantageException.ErrorNumberEnum)ex.Number)
                            {
                                case AdvantageException.ErrorNumberEnum.UnicodeNotSupported:
                                    data.Table.Rows[i][j] = data.Table.Rows[i][j].ToString().StripUnicode(replacementChar: UnicodeReplacementChar);
                                    j--;
                                    continue;
                                case AdvantageException.ErrorNumberEnum.DataTruncationAvoided:
                                    string columnName = data.Table.Columns[j].ColumnName;
                                    DataTable dt = data.Reader.GetSchemaTable();
                                    int columnSize = dt == null ? 0 : Int32.Parse(dt.Rows.Cast<DataRow>().First(x => x["ColumnName"].ToString() == columnName)["ColumnSize"].ToString());
                                    object value = data.Table.Rows[i][j];
                                    RestoreOriginallyLockedData(data);
                                    throw new AdvantageDataTruncationException(columnName, columnSize, value, ex);
                            }
                            RestoreOriginallyLockedData(data);
                            throw;
                        }
                        catch
                        {
                            RestoreOriginallyLockedData(data);
                            throw;
                        }
                    //}
                }
            }
            UnlockRecords(data);
        }
        private void RestoreOriginallyLockedData(AdvantageData data)
        {
            // Restore the original data so that pending changes are not saved on unlock.
            // This must be done instead of closing the reader and opening a new one in order to avoid temporarily losing the lock.
            data.Table = GetData(data.Query, data.Parameters);
            data.Reader.GotoBOF();
            for (int i = 0; i < data.Table.Rows.Count; i++)
            {
                data.Reader.Read();
                for (int j = 0; j < data.Table.Columns.Count; j++)
                {
                    data.Reader.SetValue(j, data.Table.Rows[i][j]);
                }
            }
            data.Reader.WriteRecord(); // This is necessary because the above flushes the previously pending changes to the DB.
        }
        public override void UpdateDataAndUnlock(AdvantageData data)
        {
            UnlockRecords(data);
            UpdateData(data.Query, data.Table, data.Parameters);
        }

        public void DeleteLockedData_DatabaseTable(AdvantageData data)
        {
            // For use only by the DatabaseTable class.
            // This function cannot be used with code that has directly manipulated this object because data.Table may
            // no longer line up with data.Reader. For example, the two become out of alignment when a row is deleted from data.Table.
            data.Reader.GotoBOF();
            for (int i = 0; i < data.Table.Rows.Count; i++)
            {
                data.Reader.Read();
                data.Reader.DeleteRecord();
            }
            UnlockRecords(data);
        }
        public override void DeleteLockedData(AdvantageData data)
        {
            UnlockRecords(data);
            foreach (DataRow row in data.Table.Rows)
            {
                row.Delete();
            }
            UpdateData(data.Query, data.Table, data.Parameters);
        }

        protected override void LockRecords(AdvantageData data)
        {
            if (this.Connection.State == ConnectionState.Closed) this.Connection.Open();

            AdsCommand cmd = null;
            int counter = 0;
            do
            {
                try
                {
                    cmd = new AdsCommand(data.Query, this.Connection);
                    if (data.Parameters != null) cmd.Parameters.AddRange(data.Parameters.ToArray());
                    data.Reader = cmd.ExecuteExtendedReader();
                    while (data.Reader.Read()) data.Reader.LockRecord();
                    data.IsLocked = !data.Reader.BOF;
                    return;
                }
                catch (AdsException ex)
                {
                    if (HandleLockRecordsException(ex, ref counter, cmd, data))
                    {
                        return;
                    }
                }
            } while (true);
        }

        protected override void UnlockRecords(AdvantageData data)
        {
            if (data.Reader != null && !data.Reader.IsClosed)
            {
                do
                {
                    try
                    {
                        while (data.Reader.ReadPrevious()) data.Reader.UnlockRecord();
                        data.Reader.Close();
                        data.Reader.Dispose();
                        data.Reader = null;
                        data.IsLocked = false;
                        return;
                    }
                    catch (AdsException ex)
                    {
                        if (((AdvantageException.ErrorNumberEnum)ex.Number).EqualsAnyOf(AdvantageException.ErrorNumberEnum.InvalidHandle, AdvantageException.ErrorNumberEnum.InvalidTableHandle, AdvantageException.ErrorNumberEnum.TableClosedButHeldInCache))
                        {
                            if (data.Reader != null)
                            {
                                data.Reader.Dispose();
                                data.Reader = null;
                            }
                            data.IsLocked = false;
                            return;
                        }
                        Thread.Sleep(RetryInterval);
                    }
                } while (true);
            }
        }
        #endregion

        #region Handle Exceptions
        private IDbCommand HandleGetDataException(AdsException ex, ref int counter, IDbCommand cmd, DbDataAdapter da)
        {
            string query = "";
            List<IDbDataParameter> parameters = null;
            if (da != null) da.Dispose();
            if (cmd != null)
            {
                query = cmd.CommandText;
                parameters = cmd.Parameters.Cast<IDbDataParameter>().ToList();
            }

            var errorNumberEnum = (AdvantageException.ErrorNumberEnum)ex.Number;
            int retryCount = RetryCountGetData;

            if (query.Length > 0 && counter <= retryCount)
            {
                switch (errorNumberEnum)
                {
                    case AdvantageException.ErrorNumberEnum.NumericOverflow:
                        Email.sendAlertEmail("Numeric Overflow Warning", "Numeric overflow error in query \"" + query + "\"" + (parameters == null || parameters.Count == 0 ? "" : ", parameters \"" + parameters.Select(x => x.Value).Join("\", \"") + "\"") + Environment.NewLine + Environment.NewLine + (ex.InnerException == null ? ex : ex.InnerException));
                        try
                        {
                            return new AdsCommand(query.Substring(0, query.LastIndexOf("where", StringComparison.OrdinalIgnoreCase)) + "where 0 = 1", this.Connection) { CommandTimeout = cmd.CommandTimeout };
                        }
                        catch (AdsException ex2)
                        {
                            HandleGetDataException(ex2, ref counter, cmd, null);
                        }
                        break;
                    case AdvantageException.ErrorNumberEnum.UnicodeNotSupported:
                        AdsCommand exCmd = null;
                        try
                        {
                            exCmd = new AdsCommand(query, this.Connection) { CommandTimeout = cmd.CommandTimeout };
                            foreach (IDbDataParameter parameter in parameters)
                            {
                                exCmd.Parameters.Add(new AdsParameter(parameter.ParameterName, parameter.Value is string ? parameter.Value.ToString().StripUnicode(replacementChar: UnicodeReplacementChar) : parameter.Value));
                            }
                            return exCmd;
                        }
                        catch (AdsException ex2)
                        {
                            if (exCmd != null) exCmd.Dispose();
                            HandleGetDataException(ex2, ref counter, cmd, null);
                        }
                        break;
                }
            }

            HandleExceptionBeforeRetry(ex, errorNumberEnum, cmd, query, parameters, ref counter, ref retryCount);

            if (counter > retryCount)
            {
                if (cmd != null) cmd.Dispose();
                HandleExceptionAfterAllRetries(ex, errorNumberEnum);
                throw new AdvantageException("Hung when reading data from query \"" + query + "\"" + (parameters == null || parameters.Count == 0 ? "" : ", parameters \"" + parameters.Select(x => x.Value).Join("\", \"") + "\"") + ": " + (ex.InnerException == null ? ex.Message : ex.InnerException.Message), ex);
            }

            return null;
        }

        private bool HandleLockRecordsException(AdsException ex, ref int counter, IDbCommand cmd, AdvantageData data)
        {
            if (data.Reader != null)
            {
                data.Reader.Close();
                data.Reader.Dispose();
            }
            if (cmd != null) cmd.Dispose();

            var errorNumberEnum = (AdvantageException.ErrorNumberEnum)ex.Number;
            int retryCount = RetryCountLockRecord;

            if (errorNumberEnum == AdvantageException.ErrorNumberEnum.NumericOverflow && counter <= retryCount)
            {
                // No email here as GetData() is called immediately after this function.
                try
                {
                    data.Reader = new AdsCommand(data.Query.Substring(0, data.Query.LastIndexOf("where", StringComparison.OrdinalIgnoreCase)) + "where 0 = 1", this.Connection).ExecuteExtendedReader();
                    return true;
                }
                catch (AdsException ex2)
                {
                    // Emailing here as we won't make it to GetData() at this point.
                    Email.sendAlertEmail("Numeric Overflow Warning", "Numeric overflow error in query \"" + data.Query + "\"" + (data.Parameters == null || data.Parameters.Count == 0 ? "" : ", parameters \"" + data.Parameters.Select(x => x.Value).Join("\", \"") + "\"") + Environment.NewLine + Environment.NewLine + (ex.InnerException == null ? ex : ex.InnerException));
                    return HandleLockRecordsException(ex2, ref counter, cmd, data);
                }
            }

            HandleExceptionBeforeRetry(ex, errorNumberEnum, null, data.Query, data.Parameters, ref counter, ref retryCount);

            if (counter > retryCount)
            {
                if (DatabaseException.ReattemptOnLockFailure(ex.Message, ex.Number, (int)AdvantageException.ErrorNumberEnum.LockFailed))
                {
                    counter = 0;
                }
                else if (errorNumberEnum == AdvantageException.ErrorNumberEnum.LockFailed)
                {
                    throw new AdvantageException("Unable to secure lock. Record is already locked in query \"" + data.Query + "\"" + (data.Parameters == null || data.Parameters.Count == 0 ? "" : ", parameters \"" + data.Parameters.Select(x => x.Value).Join("\", \"") + "\"") + ".", ex);
                }
                else
                {
                    HandleExceptionAfterAllRetries(ex, errorNumberEnum);
                    throw new AdvantageException(ex);
                }
            }

            return false;
        }

        private void HandleExceptionBeforeRetry(AdsException ex, AdvantageException.ErrorNumberEnum errorNumberEnum, IDbCommand cmd, string query, List<IDbDataParameter> parameters, ref int counter, ref int retryCount)
        {
            switch (errorNumberEnum)
            {
                case AdvantageException.ErrorNumberEnum.BadSqlStatement:
                case AdvantageException.ErrorNumberEnum.TableNotFound:
                case AdvantageException.ErrorNumberEnum.ColumnNotFound:
                case AdvantageException.ErrorNumberEnum.OrderByColumnFailed:
                case AdvantageException.ErrorNumberEnum.AggregateFunctionNotAllowed:
                case AdvantageException.ErrorNumberEnum.ScalarFunctionArgumentInvalid:
                    if (cmd != null) cmd.Dispose();
                    throw new AdvantageException("Invalid SQL statement in query \"" + query + "\"" + (parameters == null || parameters.Count == 0 ? "" : ", parameters \"" + parameters.Select(x => x.Value).Join("\", \"") + "\"") + ": " + ex.Message, ex);
                case AdvantageException.ErrorNumberEnum.NumericOverflow:
                    if (cmd != null) cmd.Dispose();
                    throw new AdvantageException("Numeric overflow error in query \"" + query + "\"" + (parameters == null || parameters.Count == 0 ? "" : ", parameters \"" + parameters.Select(x => x.Value).Join("\", \"") + "\"") + ": " + ex.Message, ex);
                case AdvantageException.ErrorNumberEnum.Timeout:
                    retryCount = RetryCountTimeout;
                    break;
                case AdvantageException.ErrorNumberEnum.ServerDidntRespond:
                case AdvantageException.ErrorNumberEnum.DiscoveryFailure:
                case AdvantageException.ErrorNumberEnum.CircuitReset:
                case AdvantageException.ErrorNumberEnum.DestinationNotAvailable:
                    retryCount = RetryCountConnectionFailure;
                    break;
                case AdvantageException.ErrorNumberEnum.KeyViolation:
                    retryCount = 0;
                    counter++;
                    return;
            }

            counter++;
            Thread.Sleep(RetryInterval);
        }

        private void HandleExceptionAfterAllRetries(AdsException ex, AdvantageException.ErrorNumberEnum errorNumberEnum)
        {
            if (errorNumberEnum.EqualsAnyOf(AdvantageException.ErrorNumberEnum.ServerDidntRespond, AdvantageException.ErrorNumberEnum.DiscoveryFailure, AdvantageException.ErrorNumberEnum.CircuitReset, AdvantageException.ErrorNumberEnum.DestinationNotAvailable))
            {
                throw new AdvantageConnectionException(ex);
            }
        }

        protected void HandleUpdateErrors(object sender, RowUpdatedEventArgs e)
        {
            if (e.Status == UpdateStatus.ErrorsOccurred)
            {
                if (e.Errors.ToString().Contains("error converting Unicode string")) return;
                if (e.Errors.ToString().Contains("requested lock could not be granted")) return;
                if (e.Errors.ToString().Contains("Concurrency"))
                {
                    if (e.Command.CommandText.Contains("SAINTRN"))
                    {
                        e.Status = UpdateStatus.SkipCurrentRow;
                        Email.sendAlertEmail("Concurrency error in transfer",
                            ParseCommand(e.Command) + "\n\n"
                            + "Records affected: " + e.RecordsAffected + "/" + e.RowCount + "\n"
                            + e.Errors + "\n\n"
                            + "Current Stack Trace: " + Environment.StackTrace, MailPriority.High);
                    }
                    else if (e.Command.CommandText.Contains("GLSYSP"))
                    {
                        var error = ParseCommand(e.Command);
                        Email.sendAlertEmail("Concurrency error in GLSYSP",
                            error.Remove(error.IndexOf("\n\n")) + "\n\n"
                            + e.Errors + "\n\n"
                            + "Current Stack Trace: " + Environment.StackTrace, MailPriority.Low);
                    }
                    else
                    {
                        Email.sendAlertEmail("Error occurred in updating data row",
                            ParseCommand(e.Command) + "\n\n"
                            + "Records affected: " + e.RecordsAffected + "/" + e.RowCount + "\n"
                            + e.Errors + "\n\n"
                            + "Current Stack Trace: " + Environment.StackTrace, MailPriority.High);
                    }
                }
            }
        }
        #endregion

        #region Miscellaneous
        public override char NamedParameterChar { get { return ':'; } }

        public override IDbDataParameter GetParameter(string name, object value, SqlStringDataType sqlDbType)
        {
            return new AdsParameter(name, value);
        }

        public override int GetNextCustomAutoNumber(int autoNumberType)
        {
            return AdvantageCustomAutoNumber.GetNextCustomAutoNumber(this, (AdvantageCustomAutoNumberType)autoNumberType);
        }

        public override IEnumerable<string> GetLockingUsers(string table, string lockedQuery = null, List<IDbDataParameter> parameters = null)
        {
            if (this.Connection.State == ConnectionState.Closed) this.Connection.Open();

            bool checkWholeTable = false;
            var lockedQueryRecordNums = new List<int>();
            AdsCommand cmd;
            if (String.IsNullOrWhiteSpace(lockedQuery))
            {
                checkWholeTable = true;
                cmd = new AdsCommand("select * from " + table, this.Connection);
            }
            else
            {
                cmd = new AdsCommand(lockedQuery, this.Connection);
                if (parameters != null) cmd.Parameters.AddRange(parameters.ToArray());
            }
            AdsExtendedReader lockedQueryReader = cmd.ExecuteExtendedReader();
            string strPath = GetTablePath(lockedQueryReader.AdsHandle);
            if (strPath == null)
            {
                cmd.Dispose();
                lockedQueryReader.Close();
                lockedQueryReader.Dispose();
                throw new FileNotFoundException("File corresponding to table \"" + table + "\" could not be found.");
            }

            if (checkWholeTable)
            {
                // Get the record numbers of all locked records in the table.
                // In the next loop we will get the locking user for each of these records.
                var sp = new AdsCommand("EXECUTE PROCEDURE sp_mgGetAllLocks('" + strPath + "');", this.Connection);
                AdsExtendedReader spReader = sp.ExecuteExtendedReader();
                while (spReader.Read()) lockedQueryRecordNums.Add(spReader.GetInt32(0));
                sp.Dispose();
                spReader.Close();
                spReader.Dispose();
            }
            else
            {
                // Get the record numbers for all of the records in the query that threw a lock error.
                // Not necessarily every record in this query is locked.
                // In the next loop we will check each record for a locking user.
                while (lockedQueryReader.Read()) lockedQueryRecordNums.Add(lockedQueryReader.RecordNumber);
            }
            cmd.Dispose();
            lockedQueryReader.Close();
            lockedQueryReader.Dispose();

            var results = new List<string>();
            foreach (int lockedRecordNum in lockedQueryRecordNums)
            {
                var sp = new AdsCommand("EXECUTE PROCEDURE sp_mgGetLockOwner('" + strPath + "', " + lockedRecordNum + ");", this.Connection);
                AdsExtendedReader spReader = sp.ExecuteExtendedReader();

                while (spReader.Read())
                {
                    string computer = spReader.GetString(0, true);
                    string user = spReader.GetString(5, true);
                    string result = null;
                    if (user.Length > 0) result = user + " on " + computer;
                    if (result != null && !results.Contains(result)) results.Add(result);
                }
                sp.Dispose();
                spReader.Close();
                spReader.Dispose();
            }

            return results;
        }

        public IEnumerable<string> GetTableUsers(string table, bool showAllInfo = false)
        {
            if (this.Connection.State == ConnectionState.Closed) this.Connection.Open();

            var cmd = new AdsCommand("select * from " + table, this.Connection);
            AdsExtendedReader reader = cmd.ExecuteExtendedReader();
            var strPath = GetTablePath(reader.AdsHandle);
            cmd.Dispose();
            reader.Close();
            reader.Dispose();
            if (strPath == null)
            {
                throw new FileNotFoundException("File corresponding to table \"" + table + "\" could not be found.");
            }

            var sp = new AdsCommand("EXECUTE PROCEDURE sp_mgGetTableUsers('" + strPath + "');", this.Connection);
            AdsExtendedReader spReader = sp.ExecuteExtendedReader();

            var results = new List<string>();
            if (showAllInfo)
            {
                while (spReader.Read())
                {
                    string result = Enumerable.Range(0, spReader.GetSchemaTable().Rows.Count).Select(x => spReader.GetValue(x).ToString().Trim()).Join("|");
                    if (!results.Contains(result)) results.Add(result);
                }
            }
            else
            {
                while (spReader.Read())
                {
                    string computer = spReader.GetString(4, true);
                    string user = spReader.GetString(0, true);
                    string result = user + " on " + computer;
                    if (!results.Contains(result)) results.Add(result);
                }
            }
            sp.Dispose();
            spReader.Close();
            spReader.Dispose();

            return results;
        }

        public override void DisconnectAllUsers()
        {
            foreach (string user in GetData("select distinct username from (EXECUTE PROCEDURE sp_mgGetConnectedUsers()) as temp where username <> 'ADS Internal 1' and upper(username) <> :MachineName", new { MachineName = Environment.MachineName.ToUpper() }).AsEnumerable().Select(x => x[0].ToString().Trim()))
            {
                try
                {
                    ExecuteCommand("EXECUTE PROCEDURE sp_mgKillUser('" + user + "');");
                }
                catch (AdsException ex)
                {
                    if (ex.Number != (int)AdvantageException.ErrorNumberEnum.UserNotConnected)
                    {
                        throw;
                    }
                }
            }
        }

        public bool IsQueryLocked(string query)
        {
            if (this.Connection.State == ConnectionState.Closed) this.Connection.Open();

            var lockedQueryRecordNums = new List<int>();
            var cmd = new AdsCommand(query, this.Connection);
            AdsExtendedReader lockedQueryReader = cmd.ExecuteExtendedReader();
            string strPath = GetTablePath(lockedQueryReader.AdsHandle);
            if (strPath == null)
            {
                cmd.Dispose();
                lockedQueryReader.Close();
                lockedQueryReader.Dispose();
                throw new FileNotFoundException("File corresponding to table \"" + query + "\" could not be found.");
            }

            // Get the record numbers for all of the records in the query that threw a lock error.
            // Not necessarily every record in this query is locked.
            // In the next loop we will check each record for a locking user.
            while (lockedQueryReader.Read()) lockedQueryRecordNums.Add(lockedQueryReader.RecordNumber);
            cmd.Dispose();
            lockedQueryReader.Close();
            lockedQueryReader.Dispose();

            foreach (int lockedRecordNum in lockedQueryRecordNums)
            {
                var sp = new AdsCommand("EXECUTE PROCEDURE sp_mgGetLockOwner('" + strPath + "', " + lockedRecordNum + ");", this.Connection);
                AdsExtendedReader spReader = sp.ExecuteExtendedReader();
                while (spReader.Read())
                {
                    if (spReader.GetString(4, true) != "No Lock")
                    {
                        sp.Dispose();
                        spReader.Close();
                        spReader.Dispose();
                        return true;
                    }
                }
                sp.Dispose();
                spReader.Close();
                spReader.Dispose();
            }

            return false;
        }

        private static string GetTablePath(IntPtr adsHandle)
        {
            uint uiRetVal;
            char[] acPath = new char[ACE.ADS_MAX_PATH + 1];
            ushort usLen = ACE.ADS_MAX_PATH + 1;
            uiRetVal = ACE.AdsGetTableFilename(adsHandle, ACE.ADS_FULLPATHNAME, acPath, ref usLen);
            if (uiRetVal != ACE.AE_SUCCESS) return null;
            return new string(acPath, 0, usLen);
        }

        public void PackTable(string strTableName)
        {
            ExecuteCommand($"execute procedure sp_PackTable('{strTableName}')");
        }
        #endregion
    }
}
