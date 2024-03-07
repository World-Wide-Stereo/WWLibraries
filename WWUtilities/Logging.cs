using System;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Threading;
using System.Security.AccessControl;
using System.Collections.ObjectModel;
using System.Linq;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    [DebuggerStepThrough]
    public static class Logging
    {
        public static string DefaultPath { get { return Environment.GetCommandLineArgs()[0] + ".log"; } }

        private static LogWriterStream _logStream = new LogWriterStream();
        public static LogWriterStream Stream { get { return _logStream; } }

        private static string _currentPath;
        private static LogWriter _logWriter;
        public static LogWriter Current { get { return _logWriter; } }
        public static string LogFile
        {
            get
            {
                return _currentPath;
            }
            set
            {
                if (_logWriter != null)
                {
                    _logWriter.Close();
                }
                _currentPath = value;
                _logWriter = new LogWriter(value);
                _logWriter.ConsoleEcho = true;
            }
        }
        private static ReadOnlyCollection<string> _mappedDrives;
        private static ReadOnlyCollection<string> MappedDrives
        {
            get
            {
                return _mappedDrives == null
                    ? (_mappedDrives = DriveInfo.GetDrives().Where(x => x.DriveType == DriveType.Network).Select(x => x.Name).ToList().AsReadOnly())
                    : _mappedDrives;
            }
        }

        private static void Setup()
        {
            if (_logWriter == null)
            {
                LogFile = DefaultPath;
                _logWriter.WriteLine("Starting logging at " + DateTime.Now.ToSortableDateTimeString(), prependDateTime: false);
            }
        }

        public static void WriteFinishedLine()
        {
            WriteLine($"Finished logging at {DateTime.Now.ToSortableDateTimeString()}{Environment.NewLine}", prependDateTime: false);
        }

        public static void WriteLine(string value, bool prependDateTime = true)
        {
            Setup();
            _logWriter.WriteLine(value, prependDateTime: prependDateTime);
        }
        public static void WriteLine(string header, string data, bool prependDateTime = true)
        {
            Setup();
            WriteLine(header.ToUpper().PadRight(_logWriter.Padding) + " " + data, prependDateTime: prependDateTime);
        }

        public static void Write(char data)
        {
            Setup();
            _logWriter.Write(data);
        }
        public static void Write(string data)
        {
            Setup();
            _logWriter.Write(data);
        }

        public static void ToggleIndent()
        {
            Setup();
            _logWriter.ToggleIndent();
        }
        public static void ToggleIndent(string value, bool prependDateTime = true)
        {
            Setup();
            _logWriter.ToggleIndent(value, prependDateTime: prependDateTime);
        }

        public static string GetLogReplay()
        {
            return _logWriter != null && _logWriter.MaintainMemoryLog ? _logWriter.GetMemoryLog() : null;
        }

        public static void Close()
        {
            if (_logWriter != null)
            {
                _logWriter.Close();
            }
        }

        // This function should be used for all log file copying in order to retain the correct file permissions.
        // overwrite parameter: true if the destination file can be overwritten; otherwise, false
        public static void Copy(string sourcePath, string destinationPath, bool overwrite = false)
        {
            File.Copy(sourcePath, destinationPath, overwrite);
            SetLogFilePermissions(destinationPath);
        }

        private static void SetLogFilePermissions(string logFilepath)
        {
            // Sets appropriate file permissions for log files
            FileSecurity oFileSecurity = File.GetAccessControl(logFilepath);
            oFileSecurity.SetAccessRuleProtection(true, false); // Disables permission inheritance
            oFileSecurity.RemoveAccessRule(new FileSystemAccessRule("Domain Users", FileSystemRights.FullControl, AccessControlType.Allow));
            oFileSecurity.RemoveAccessRule(new FileSystemAccessRule("Domain Users", FileSystemRights.Modify, AccessControlType.Allow));
            oFileSecurity.RemoveAccessRule(new FileSystemAccessRule("Domain Users", FileSystemRights.ReadAndExecute, AccessControlType.Allow));
            oFileSecurity.RemoveAccessRule(new FileSystemAccessRule("Domain Users", FileSystemRights.Read, AccessControlType.Allow));
            oFileSecurity.RemoveAccessRule(new FileSystemAccessRule("Domain Users", FileSystemRights.FullControl, AccessControlType.Deny));
            oFileSecurity.RemoveAccessRule(new FileSystemAccessRule("Domain Users", FileSystemRights.Modify, AccessControlType.Deny));
            oFileSecurity.RemoveAccessRule(new FileSystemAccessRule("Domain Users", FileSystemRights.ReadAndExecute, AccessControlType.Deny));
            oFileSecurity.RemoveAccessRule(new FileSystemAccessRule("Domain Users", FileSystemRights.Read, AccessControlType.Deny));

            oFileSecurity.SetAccessRule(new FileSystemAccessRule("Domain Users", FileSystemRights.Write | FileSystemRights.ReadAttributes | FileSystemRights.ReadExtendedAttributes | FileSystemRights.ReadPermissions, AccessControlType.Allow));

            oFileSecurity.SetAccessRule(new FileSystemAccessRule("Domain Admins", FileSystemRights.FullControl, AccessControlType.Allow));
            oFileSecurity.SetAccessRule(new FileSystemAccessRule("autoprocess", FileSystemRights.FullControl, AccessControlType.Allow));
            if (!logFilepath.Contains(Path.DirectorySeparatorChar) || (!logFilepath.StartsWithAnyOf(MappedDrives, StringComparison.OrdinalIgnoreCase) && !new Uri(logFilepath).IsUnc))
            {
                // If the file is on a local PC, the user must have full control, or else developers won't be able to open log files
                // on their own machines. We learned that just Domain Admins having Full Control isn't enough on local PCs.
                oFileSecurity.SetAccessRule(new FileSystemAccessRule(Environment.UserName, FileSystemRights.FullControl, AccessControlType.Allow));
            }

            File.SetAccessControl(logFilepath, oFileSecurity);
        }

        [DebuggerStepThrough]
        public class LogWriter : IDisposable
        {
            public int NumTries = 5;
            public int MinSleepMilliSec = 1000;
            public int MaxSleepMilliSec = 5000;
            public int Padding = 15;
            public bool ConsoleEcho;
            public bool CloseAfterWrite = true;
            public string Prefix = "";
            public bool MaintainMemoryLog = false;

            private string filePath;
            private StreamWriter writer;
            private StringBuilder internalLog;
            private bool fileAppend;
            private object lockObject = new object();

            public LogWriter(string path, bool append = true)
            {
                filePath = path;
                fileAppend = append;
            }

            private void Setup()
            {
                if (writer == null)
                {
                    bool isNewFile = !File.Exists(filePath);
                    writer = new StreamWriter(filePath, fileAppend); // If the log file doesn't exist yet, this creates an empty file

                    if (MaintainMemoryLog)
                    {
                        internalLog = new StringBuilder();
                    }

                    if (isNewFile)
                    {
                        SetLogFilePermissions(filePath);
                    }
                }
            }

            public void WriteLine(string value, bool prependDateTime = true)
            {
                lock (lockObject)
                {
                    int counter = 0;
                    do
                    {
                        try
                        {
                            Setup();

                            if (prependDateTime && !value.IsNullOrBlank())
                            {
                                writer.Write(DateTime.Now.ToSortableDateTimeString() + " - ");
                            }

                            writer.WriteLine(Prefix + value);

                            if (ConsoleEcho)
                            {
                                Console.WriteLine(Prefix + value);
                            }
                            if (CloseAfterWrite)
                            {
                                writer.Flush();
                                writer.Close();
                                writer = null;
                            }
                            if (MaintainMemoryLog)
                            {
                                internalLog.AppendLine(value);
                            }
                            break;
                        }
                        catch (IOException)
                        {
                            counter++;
                            if (counter >= NumTries)
                            {
                                throw;
                            }
                            Thread.Sleep(RandomThreadSafe.Next(MinSleepMilliSec, MaxSleepMilliSec));
                        }
                    } while (true);
                }
            }
            public void WriteLine(string header, string data, bool prependDateTime = true)
            {
                this.WriteLine(header.PadRight(Padding) + " " + data, prependDateTime: prependDateTime);
            }

            public void Write(char data)
            {
                lock (lockObject)
                {
                    int counter = 0;
                    do
                    {
                        try
                        {
                            Setup();

                            writer.Write(data);

                            if (ConsoleEcho)
                            {
                                Console.Write(data);
                            }
                            if (CloseAfterWrite)
                            {
                                writer.Flush();
                                writer.Close();
                                writer = null;
                            }
                            if (MaintainMemoryLog)
                            {
                                internalLog.Append(data);
                            }
                            break;
                        }
                        catch (IOException)
                        {
                            counter++;
                            if (counter >= NumTries)
                            {
                                throw;
                            }
                            Thread.Sleep(RandomThreadSafe.Next(MinSleepMilliSec, MaxSleepMilliSec));
                        }
                    } while (true);
                }
            }
            public void Write(string data)
            {
                lock (lockObject)
                {
                    int counter = 0;
                    do
                    {
                        try
                        {
                            Setup();

                            writer.Write(data);

                            if (ConsoleEcho)
                            {
                                Console.Write(data);
                            }
                            if (CloseAfterWrite)
                            {
                                writer.Flush();
                                writer.Close();
                                writer = null;
                            }
                            if (MaintainMemoryLog)
                            {
                                internalLog.Append(data);
                            }
                            break;
                        }
                        catch (IOException)
                        {
                            counter++;
                            if (counter >= NumTries)
                            {
                                throw;
                            }
                            Thread.Sleep(RandomThreadSafe.Next(MinSleepMilliSec, MaxSleepMilliSec));
                        }
                    } while (true);
                }
            }

            public void ToggleIndent()
            {
                if (this.Prefix == "")
                {
                    this.Prefix = "\t";
                }
                else if (this.Prefix == "\t")
                {
                    this.Prefix = "";
                }
            }
            public void ToggleIndent(string value, bool prependDateTime = true)
            {
                if (this.Prefix == "")
                {
                    WriteLine(value, prependDateTime: prependDateTime);
                    ToggleIndent();
                }
                else
                {
                    ToggleIndent();
                    WriteLine(value, prependDateTime: prependDateTime);
                }
            }

            internal void Flush()
            {
                if (writer != null && writer.BaseStream.CanWrite)
                {
                    lock (lockObject)
                    {
                        writer.Flush();
                    }
                }
            }

            public string GetMemoryLog()
            {
                return MaintainMemoryLog ? internalLog.ToString() : null;
            }

            /// <summary>
            /// Gets the log file path of the current instance.
            /// </summary>
            public string GetPath()
            {
                return this.filePath;
            }

            public void Close()
            {
                if (writer != null)
                {
                    lock (lockObject)
                    {
                        writer.Close();
                        writer = null;
                    }
                }
            }

            public void Dispose()
            {
                this.Close();
            }
        }

        [DebuggerStepThrough]
        public class LogWriterStream : TextWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
            public override void Write(char value)
            {
                Logging.Write(value);
                if (value == '\n')
                {
                    Logging.Current.Flush();
                }
            }
            public override void Write(string value)
            {
                Logging.Write(value);
                if (value.EndsWith("\n") || value.EndsWith("\r"))
                {
                    Logging.Current.Flush();
                }
            }
        }
    }
}
