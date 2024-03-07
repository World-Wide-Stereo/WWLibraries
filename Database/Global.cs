using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using Microsoft.Diagnostics.Runtime;
using ww.Utilities;

namespace ww.Tables
{
    public static class Global
    {
        private static string _programName;
        public static string ProgramName { get { return _programName ?? (_programName = Assembly.GetEntryAssembly()?.GetName().Name ?? ""); } }

        public static Database.AdvantageDatabase AdvantageDatabaseInUse { get; private set; }
        public static Database.SqlServerDatabase SqlServerDatabaseInUse { get; private set; }

        public static AdvantageConnection AdsConn;
        public static readonly SqlServerConnection SqlConn = new SqlServerConnection { IsGlobal = true };

        public static bool TestMode => SqlConn.DatabaseInUse != Database.SqlServerDatabase.Production;

        public static void SwitchTestMode(bool enableTestMode)
        {
            SwitchDatabase(enableTestMode ? Database.SqlServerDatabase.Test : Database.SqlServerDatabase.Production);
            SqlServerDatabaseInUse = enableTestMode ? Database.SqlServerDatabase.Test : Database.SqlServerDatabase.Production;
        }

        public static void SwitchDatabase(Database.SqlServerDatabase database)
        {
            SqlServerDatabaseInUse = database;
            SqlConn.DatabaseInUse = database;
            Email.TestMode = database != Database.SqlServerDatabase.Production;
            Email.Enabled = database == Database.SqlServerDatabase.Production;
            Graph.GraphEnabled = database == Database.SqlServerDatabase.Production;
        }

        // Using the network path rather than mapped drives so that our web apps can access the files.
        public const string EomLockFile = @"\\server\locks\eom.dat";
        private static readonly List<string> LockFiles = new List<string> { @"\\server\locks\maint.dat", EomLockFile, @".\lock.dat" };
        public static bool IsSystemLocked { get { return !new DirectoryInfo(@"\\server\locks\").Exists || LockFiles.Any(x => File.Exists(x)); } }

        private static readonly ConcurrentBag<int> UnhandledExceptionEventThreadIDs = new ConcurrentBag<int>();
        public static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            UnhandledExceptionEventThreadIDs.Add(Thread.CurrentThread.ManagedThreadId);
            UnhandledException((Exception)e.ExceptionObject);
        }
        public static void UnhandledExceptionWithLogging(object sender, UnhandledExceptionEventArgs e)
        {
            UnhandledExceptionEventThreadIDs.Add(Thread.CurrentThread.ManagedThreadId);
            Exception ex = (Exception)e.ExceptionObject;
            Logging.WriteLine(ex + Environment.NewLine);
            UnhandledException(ex);
        }
        private static void UnhandledException(Exception ex)
        {
            Email.sendAlertEmail(Global.ProgramName + " Exception", ex.ToString(), priority: MailPriority.High);

            // Wait for all miscellaneous foreground threads to finish before terminating the program.
            // Otherwise, exiting will terminate the entire program before the email is sent.
            bool running = true;
            do
            {
                using (DataTarget target = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive))
                {
                    if (!target.ClrVersions[0].CreateRuntime().Threads.Any(x =>
                        !x.IsBackground // Is a foreground thread.
                        && (x.IsAlive || x.IsUnstarted) // Is running or about to run.
                        && x.BlockingObjects.Count == 0 // Isn't waiting on another thread to finish.
                        && !UnhandledExceptionEventThreadIDs.Contains(x.ManagedThreadId))) // Isn't this thread nor another thread also running this function.
                    {
                        running = false;
                    }
                }
            } while (running);

            Environment.Exit(Environment.ExitCode);
        }
    }
}
