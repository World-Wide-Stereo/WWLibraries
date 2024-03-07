using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Windows.Forms;
using Advantage.Data.Provider;
using AdvantageClientEngine;
using ww.Tables;
using ww.Utilities;

namespace ww
{
    public class libadsdata
    {
        private int locks = 0;
        private string sharelock = "";

        private static Database.AdvantageDatabase _databaseToUse;
        public static Database.AdvantageDatabase DatabaseToUse
        {
            set
            {
                _databaseToUse = value;
                if (c.IsConnectionAlive || c.State == ConnectionState.Open)
                {
                    c.Close();
                    AdsConnection.FlushConnectionPool(c.ConnectionString); // The connection isn't truely closed without this line. See the Advantage Data Architect's documentation on AdsConnection.Close.
                }
                c = Connect();
            }
        }

        private static string GetConnectionString(string dbPath, string strInitialCatalog = "ww.add")
        {
            return $@"data source=\\worldads2012{dbPath}; Initial Catalog={strInitialCatalog}; User ID={Environment.UserName.ToLower()}; Password=wwstereo; ServerType=remote; TableType=CDX; CharType=ANSI; Pooling=True; Min Pool Size=0;";
        }

        private static AdsConnection c = Connect(); // The Connect() on this line is called only once because c is static.
        private static AdsConnection Connect()
        {
            switch (_databaseToUse)
            {
                case Database.AdvantageDatabase.Standard:
                    c = new AdsConnection(GetConnectionString(@"\data\wwsa"));
                    break;
                case Database.AdvantageDatabase.UnitTest:
                    c = new AdsConnection(GetConnectionString(@"\unittesting", "UnitTesting.add"));
                    break;
                case Database.AdvantageDatabase.Test:
                    c = new AdsConnection(GetConnectionString(@"\test\wwsa"));
                    break;
                case Database.AdvantageDatabase.Test_Eric:
                    c = new AdsConnection(GetConnectionString(@"\test_eric"));
                    break;
                case Database.AdvantageDatabase.Test_Terrance:
                    c = new AdsConnection(GetConnectionString(@"\test_terrance"));
                    break;
                case Database.AdvantageDatabase.Test_Jon:
                    c = new AdsConnection(GetConnectionString(@"\test_jon"));
                    break;
                default:
                    throw new ArgumentException(_databaseToUse.ToString());
            }

            try
            {
                c.Open();
            }
            catch (Exception ex)
            {
                Email.sendAlertEmail("Failed to open database connection", Email.AlertEmailSubjects.DatabaseConnectionFailed, $"PC: {Environment.MachineName}\nUser: {Environment.UserName}\nProcess: {Process.GetCurrentProcess().MainModule.FileName}\n\nAttempting to open database connection to database {_databaseToUse} failed: {ex}", MailPriority.High);
            }

            return c;
        }
        public object GetField(string field, string table, string condition)
        {
            return(GetField(field, table, condition, null));
        }
        public object GetField(string field, string table, string condition, string dfault)
        {
            object o = dfault;
            string query = null;
            try
            {
                AdsDataReader dr;
                AdsCommand cm = new AdsCommand();
                cm.CommandType = CommandType.Text;
                cm.Connection = c;
                cm.CommandText = query = "SELECT " + field + " FROM " + table + " WHERE " + condition;
                dr = cm.ExecuteReader();
                if (dr.Read()) o = dr[0].ToString();
                dr.Close();
                cm.Dispose();
            }
            catch (AdsException ex)
            {
                ShowAdsException_Static(table, query, ex);
            }
            return (o);
        }
        public AdsDataReader GetData(string s)
        { 
            if (c.State == ConnectionState.Closed) 
                c.Open();
            AdsDataReader dr;

            // this cannot work this way because it breaks the inventory screen

            //bool ok2cont = true;
            //do
            //{
                //try
                //{
                    AdsCommand cm = c.CreateCommand();
                    cm.CommandText = s;
                    dr = cm.ExecuteReader();
                    return (dr);
                //}
                //catch (AdsException ex)
                //{
                    //ok2cont = ShowAdsException_Static("", ex);
                //}
            //} while(ok2cont);
            //return null;
        }
        public DataTable GetDataTable(string s)
        {
            if (c.State == ConnectionState.Closed)
                c.Open();
            AdsDataAdapter da;
            try
            {
                da = new AdsDataAdapter(s, c);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch { }
            return null;
        }
        public AdsDataAdapter GetAdapt(string s)
        {
            if (c.State == ConnectionState.Closed) c.Open();
            AdsDataAdapter da;
            bool ok2cont;
            do
            {
                try
                {
                    da = new AdsDataAdapter(s, c);
                    AdsCommandBuilder cb = new AdsCommandBuilder();
                    cb.RequirePrimaryKey = false;
                    cb.DataAdapter = da;
                    da.InsertCommand = cb.GetInsertCommand();
                    da.UpdateCommand = cb.GetUpdateCommand();
                    return (da);
                }
                catch (AdsException ex)
                {
                    ok2cont = ShowAdsException_Static(null, s, ex);
                }
            } while (ok2cont);
            return null;
        }
        public int GetMax(string s)
        {
            AdsDataReader dr;
            AdsCommand cm = new AdsCommand();
            cm.CommandType = CommandType.Text;
            cm.Connection = c;
            cm.CommandText = s;
            dr = cm.ExecuteReader();
            int i = -1;
            if (dr.Read()) i = int.Parse(dr[0].ToString());
            dr.Close();
            //int i = (int)cm.ExecuteScalar();
            return(i);
        }
        public bool ShowAdsException(string file, AdsException ex)
        {
            return ShowAdsException_Static(file, null, ex);
        }
        public bool ShowAdsException(string file, string query, AdsException ex)
        {
            return ShowAdsException_Static(file, query, ex);
        }
        private static bool ShowAdsException_Static(string file, string query, AdsException ex) // This separate, static function is required for use in the static function Connect().
        {
            bool retry = false;
            if (ex.Number == 5035)
            {
                if (string.IsNullOrEmpty(file))
                {
                    MessageBox.Show("Record lock. OK to Retry.");
                }
                else
                {
                    MessageBox.Show(file + " record lock. OK to Retry.\n\nPossible sources:\n" + String.Join("\n", GetLockingUsers(file, query, new List<string> { "receipt" })));
                }
                retry = true;
            }
            else if (ex.Number == 6420 || ex.Number == 5018)
            {
                MessageBox.Show("Server Connection Lost. OK to Retry.");
                if (c.State == ConnectionState.Open) c.Close();
                if (c.State == ConnectionState.Closed) c.Open();
                retry = true;
            }
            else if (ex.Number == 6610)
            {
                MessageBox.Show("ADS Server Timeout. OK to Retry.");
                if (c.State == ConnectionState.Open) c.Close();
                if (c.State == ConnectionState.Closed) c.Open();
                retry = true;
            }
            else if (ex.Number == 7008)
            {
                if (string.IsNullOrEmpty(file))
                {
                    MessageBox.Show("Table lock. OK to Retry.");
                }
                else
                {
                    MessageBox.Show(file + " table lock. OK to Retry.\n\nPossible sources:\n" + String.Join("\n", GetLockingUsers(file)));
                }
                retry = true;
            }
            else if (ex.Number == 7209)
            {
                MessageBox.Show(file + " could not return any results. Try changing your search.");
                retry = true;
            }
            else MessageBox.Show(file + "\n\n" + ex.Message);
            return (retry);
        }
        public void Update(string file, AdsDataAdapter da, DataTable dt, ref AdsExtendedReader reclock)
        {
            CloseLocks(ref reclock);
            bool ok2cont;
            do
            {
                try
                {
                    da.Update(dt);
                    ok2cont = false;
                }
                catch (AdsException ex)
                {
                    ok2cont = ShowAdsException_Static(file, da.SelectCommand.CommandText, ex);
                }
            } while (ok2cont);
            if (locks > 0) //need to relock
            {
                sharelock = "";
                rlock(file, ref reclock, da.SelectCommand.CommandText);
                locks--;
            }
        }
        public bool rlock(string file, ref AdsExtendedReader dr, string select, bool noshare)
        {
            if (noshare) sharelock = "";
            return (rlock(file, ref dr, select));
        }
        public bool rlock(string file, ref AdsExtendedReader dr, string select)
        {
            try
            {
                if (sharelock == select.Substring(select.Length - 6) && locks > 0)
                {
                    locks++;
                    return true;
                }
                if (c.State == ConnectionState.Closed) { c.Open(); }
                dr = new AdsCommand(select, c).ExecuteExtendedReader();
                while (dr.Read()) { dr.LockRecord(); }
                while (dr.ReadPrevious()) { /*Back up so Read() will take us to the first record*/ }
                sharelock = select.Substring(select.Length - 6);
                locks++;
                return true;
            }
            catch (AdsException ex)
            {
                if (ex.Number == 5035)
                {
                    MessageBox.Show($"{file} record lock. Please try again later.\n\nPossible sources:\n{string.Join("\n", GetLockingUsers(file, select))}");
                    dr.Close();
                }
                return false;
            }
        }
        public void CloseLocks(ref AdsExtendedReader reclock)
        {
            if (reclock != null)
            {
                try
                {
                    if (!reclock.IsClosed)
                    {
                        locks--;
                        reclock.Close();
                    }
                }
                catch (AdsException ex)
                {
                    //if the handle is not valid, the connection is likely already disposed
                    if (ex.Number != 5018 && ex.Number != 5094) throw;
                }
            }
        }
        public void CloseLocks(ref AdsExtendedReader reclock, string file, string cmd)
        {
            CloseLocks(ref reclock);

            if (locks > 0) //need to relock
            {
                sharelock = "";
                rlock(file, ref reclock, cmd);
                locks--;
            }
        }
        public bool Delete(string file, string conditions)
        {
            string query = "select * from " + file + " where " + conditions;
            try
            {
                AdsDataAdapter da = GetAdapt(query);
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow row in dt.Rows) row.Delete();
                da.Update(dt);
                da.Dispose();
                return true;
            }
            catch (AdsException ex)
            {
                return !ShowAdsException_Static(file, query, ex);
            }
        }

        public static string[] GetLockingUsers(string table, string lockedQuery = null, List<string> displayValuesForColumns = null)
        {
            if (c.State == ConnectionState.Closed) c.Open();

            bool checkWholeTable = false;
            var lockedQueryRecordNums = new List<int>();
            if (String.IsNullOrEmpty(lockedQuery))
            {
                checkWholeTable = true;
                lockedQuery = "select * from " + table;
            }
            var cmd = new AdsCommand(lockedQuery, c);
            AdsExtendedReader lockedQueryReader = cmd.ExecuteExtendedReader();
            string strPath = GetTablePath(lockedQueryReader.AdsHandle);
            if (strPath == null) throw new FileNotFoundException("File corresponding to table '" + table + "' could not be found.");

            if (checkWholeTable)
            {
                // Get the record numbers of all locked records in the table.
                // In the next loop we will get the locking user for each of these records.
                var sp = new AdsCommand("EXECUTE PROCEDURE sp_mgGetAllLocks('" + strPath + "');", c);
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
                lockedQueryReader.GotoBOF();
            }

            var results = new List<string>();
            if (lockedQueryRecordNums.Count > 0)
            {
                DataColumnCollection columns = null;
                if (displayValuesForColumns != null)
                {
                    DataTable dt = new libadsdata().GetDataTable("select * from " + table + " where 0 = 1");
                    columns = dt.Columns;
                    dt.Dispose();
                }

                foreach (int lockedRecordNum in lockedQueryRecordNums)
                {
                    var sp = new AdsCommand("EXECUTE PROCEDURE sp_mgGetLockOwner('" + strPath + "', " + lockedRecordNum + ");", c);
                    AdsExtendedReader spReader = sp.ExecuteExtendedReader();
                    lockedQueryReader.Read();
                    while (spReader.Read())
                    {
                        string computer = spReader.GetString(0, true);
                        string user = spReader.GetString(5, true);
                        string result = null;
                        if (user.Length > 0)
                        {
                            result = user + " on " + computer;
                        }
                        if (displayValuesForColumns != null && user.Length > 0)
                        {
                            bool columnValueAdded = false;
                            foreach (string column in displayValuesForColumns)
                            {
                                if (columns.Contains(column))
                                {
                                    result += ". " + column + ": " + lockedQueryReader[column] + ", ";
                                    columnValueAdded = true;
                                }
                            }
                            if (columnValueAdded) result = result.Substring(0, result.Length - 3) + ".";
                        }
                        if (result != null && !results.Contains(result)) results.Add(result);
                    }
                    sp.Dispose();
                    spReader.Close();
                    spReader.Dispose();
                }
            }
            cmd.Dispose();
            lockedQueryReader.Close();
            lockedQueryReader.Dispose();

            return results.ToArray();
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
    }
}
