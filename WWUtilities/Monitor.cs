using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public abstract class Monitor
    {
        protected abstract string MonitorName { get; }
        protected abstract string[] EmailRecipients { get; }
        protected virtual string[] CopyRecipients => null;
        protected Status status { get; private set; }

        public virtual bool ShouldRunNow { get { return true; } }
        protected virtual bool SendResults { get { return true; } }
        protected virtual bool EmailIsHTML { get { return false; } }
        protected virtual bool NewLineToHtmlLineBreak { get { return false; } }
        protected virtual System.Net.Mail.MailPriority EmailPriority { get { return System.Net.Mail.MailPriority.High; } }
        protected virtual string LogFile { get { return this.GetType().Name + ".log"; } }
        protected Logging.LogWriter Log;
        protected virtual bool DeleteAttachmentsAfterSending { get { return true; } }

        /// <summary>
        /// Tests whatever this monitor does to test.  Sets status.Success to false if the tests failed and an email should be sent.
        /// </summary>
        protected abstract void Test();


        public void RunTest(bool isConsoleApp = true)
        {
            Logging.WriteLine("Started " + MonitorName);
            status = new Status();
            try
            {
                if (isConsoleApp) Console.Title = Assembly.GetEntryAssembly().GetName().Name + " - " + MonitorName;
                using (Log = new Logging.LogWriter(LogFile))
                {
                    Test();
                }
                if (!status.Success && SendResults)
                {
                    Email.sendEmail(EmailRecipients.Join(","), MonitorName, NewLineToHtmlLineBreak ? status.GetMessage().Replace(Environment.NewLine, "<br>") : status.GetMessage(), attachmentFiles: status.Attachments, priority: EmailPriority, isHTML: EmailIsHTML, copyAddresses: CopyRecipients);
                    if (status.Attachments?.Count > 0 && DeleteAttachmentsAfterSending)
                    {
                        var failedPaths = new List<string>();
                        foreach (string file in status.Attachments)
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch
                            {
                                failedPaths.Add(file);
                            }
                        }
                        if (failedPaths.Count > 0)
                        {
                            Email.sendAlertEmail("Error in testing monitor", $"Failed to delete attachments after sending monitor email {failedPaths.Join(", ")}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Email.sendAlertEmail("Error in testing " + MonitorName, NewLineToHtmlLineBreak ? status.GetMessage().Replace(Environment.NewLine, "<br>") : status.GetMessage() + Environment.NewLine + Environment.NewLine + ex, priority: System.Net.Mail.MailPriority.High);
            }
            Logging.WriteLine("Finished " + MonitorName);
        }

        // Inherit from this if you need more properties, or need a different message.
        public class Status
        {
            public bool Success = true;
            protected StringBuilder _message = new StringBuilder();
            public List<string> Attachments = new List<string>();

            public void AddFailureMessage(string message)
            {
                Success = false;
                _message.Append(message);
                _message.AppendLine("   "); // This keeps it from having linebreaks removed.
            }

            public virtual string GetMessage()
            {
                return _message.ToString();
            }
        }

        protected void ProcessStartTimeCheck(string processName, int warnThresholdInMinutes, int terminateThresholdInMinutes, string folder = null, List<string> windowTitleContains = null, List<string> windowTitleDoesNotContain = null)
        {
            foreach (Process p in Process.GetProcessesByName(processName))
            {
                string windowTitle = p.MainWindowTitle;
                if (windowTitleContains != null)
                {
                    if (windowTitleContains.Any(x => windowTitle.Contains(x)))
                    {
                        ProcessStartTimeCheck(processName, warnThresholdInMinutes, terminateThresholdInMinutes, folder, p, windowTitle);
                    }
                }
                else if (windowTitleDoesNotContain != null)
                {
                    if (windowTitleDoesNotContain.All(x => !windowTitle.Contains(x)))
                    {
                        ProcessStartTimeCheck(processName, warnThresholdInMinutes, terminateThresholdInMinutes, folder, p, windowTitle);
                    }
                }
                else
                {
                    ProcessStartTimeCheck(processName, warnThresholdInMinutes, terminateThresholdInMinutes, folder, p, windowTitle);
                }
            }
        }
        private void ProcessStartTimeCheck(string processName, int warnThresholdInMinutes, int terminateThresholdInMinutes, string folder, Process p, string windowTitle)
        {
            Retry.Do(() =>
            {
                if (p.HasExited)
                {
                    return;
                }

                try
                {
                    string path = p.MainModule.FileName.Substring(0, p.MainModule.FileName.LastIndexOf('\\'));
                    string folderName = path.Substring(path.LastIndexOf('\\') + 1, path.Length - path.LastIndexOf('\\') - 1);

                    if (terminateThresholdInMinutes > 0 && p.StartTime.AddMinutes(terminateThresholdInMinutes) < DateTime.Now)
                    {
                        if (folder.IsNullOrBlank() || folder == folderName)
                        {
                            try
                            {
                                p.Kill();
                                p.WaitForExit();
                                status.AddFailureMessage("Found and terminated a \"" + windowTitle + "\" (" + processName + ".exe) instance older than " + terminateThresholdInMinutes + " minutes.");
                            }
                            catch (Exception ex)
                            {
                                status.AddFailureMessage("Failed to terminate a \"" + windowTitle + "\" (" + processName + ".exe) instance older than " + terminateThresholdInMinutes + " minutes due to " + ex.Message);
                            }
                        }
                    }
                    else if (warnThresholdInMinutes > 0 && p.StartTime.AddMinutes(warnThresholdInMinutes) < DateTime.Now)
                    {
                        status.AddFailureMessage("Found a \"" + p.MainWindowTitle + "\" (" + processName + ".exe) instance that has been running for over " + warnThresholdInMinutes + " minutes." + (terminateThresholdInMinutes <= 0 ? "" : "It will be terminated if it runs for over " + terminateThresholdInMinutes + " minutes."));
                    }
                }
                catch (Win32Exception ex)
                {
                    Email.sendEmail("example@example.com", "Process Start Time Exception", $"Process Has Exited: {p.HasExited}{Environment.NewLine}" +
                                                                                            $"Process Name: {p?.ProcessName}{Environment.NewLine}" +
                                                                                            $"Is Monitor 32 Bit: {IntPtr.Size == 4}{Environment.NewLine}" +
                                                                                            $"Native Error Code: {ex.NativeErrorCode}{Environment.NewLine}" +
                                                                                            $"Exception: {ex}");
                    throw;
                }

            });

        }

        public class Ping : System.Net.NetworkInformation.Ping
        {
            public string ServerName { get; set; }
            public string ServerIP { get; set; }

            public Ping(string serverName, string serverIP = null)
            {
                ServerName = serverName;
                ServerIP = serverIP;
            }

            public PingReply Send()
            {
                return Send(ServerIP.IsNullOrBlank() ? ServerName : ServerIP);
            }
        }

        /// <summary>
        /// Pings the specified servers, returning an IEnumerable of the ones that failed.
        /// </summary>
        public static IEnumerable<Ping> PingServers(IEnumerable<Ping> testServers, int numTriesPerServer = 2, int sleepBetweenTriesInSeconds = 30)
        {
            int i = 0;
            foreach (Ping testServer in testServers)
            {
                do
                {
                    PingReply pingReply;
                    try
                    {
                        pingReply = testServer.Send();
                    }
                    catch
                    {
                        pingReply = null;
                    }

                    if (pingReply == null || pingReply.Status != IPStatus.Success)
                    {
                        if (i == numTriesPerServer - 1)
                        {
                            yield return testServer;
                            i = 0;
                            break;
                        }
                        Thread.Sleep(1000 * sleepBetweenTriesInSeconds);
                        i++;
                        continue;
                    }

                    i = 0;
                    break;
                } while (i < numTriesPerServer);
            }
        }
    }
}
