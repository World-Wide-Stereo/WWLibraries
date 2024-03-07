using System;
using System.Diagnostics;
using System.Linq;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public static class MonitorProcess
    {
        public static void TestProcesses(Monitor.Status status, Logging.LogWriter log, ProcessInfoStruct[] processes)
        {
            bool mustRestartPreviousProcess = false;
            bool restartedPreviousProcess = false;

            for (int i = 0; i < processes.Length; i++)
            {
                ProcessInfoStruct processInfoStruct = processes[i];
                Process[] procs = Process.GetProcessesByName(processInfoStruct.ExecutableName);
                if (!procs.Any())
                {
                    RestartProcess(status, log, processInfoStruct, ref restartedPreviousProcess, ref mustRestartPreviousProcess);
                }
                else if (mustRestartPreviousProcess || (processInfoStruct.RestartAfterXHours > 0 && (DateTime.Now - procs[0].StartTime).TotalMinutes >= processInfoStruct.RestartAfterXHours * 60 - 1) || (!processInfoStruct.RequiredWindowName.IsNullOrBlank() && !OpenWindowGetter.GetOpenWindows().Values.Contains(processInfoStruct.RequiredWindowName)))
                {
                    if (processInfoStruct.RestartPreviousProcess && !mustRestartPreviousProcess)
                    {
                        mustRestartPreviousProcess = true;
                        i = i - 2;
                        continue;
                    }

                    int runtimeInMinutes = (int)(DateTime.Now - procs[0].StartTime).TotalMinutes;

                    foreach (Process proc in procs)
                    {
                        try
                        {
                            proc.CloseMainWindow();
                            proc.WaitForExit(1000 * 60 * 1);
                            if (!proc.HasExited)
                            {
                                proc.Kill();
                                proc.WaitForExit();
                            }
                        }
                        catch (Exception ex)
                        {
                            status.AddFailureMessage("Exception in scheduled restart of " + processInfoStruct.ExecutableName + ".\n\n" + ex);
                        }
                    }

                    RestartProcess(status, log, processInfoStruct, ref restartedPreviousProcess, ref mustRestartPreviousProcess, runtimeInMinutes: runtimeInMinutes);
                }
            }
        }

        private static void RestartProcess(Monitor.Status status, Logging.LogWriter log, ProcessInfoStruct processInfoStruct, ref bool restartedPreviousProcess, ref bool mustRestartPreviousProcess, int runtimeInMinutes = 0)
        {
            Process proc = Process.Start(processInfoStruct.ProcessStartInfo);
            if (processInfoStruct.WaitForExit && proc != null)
            {
                proc.WaitForExit();
            }

            if (restartedPreviousProcess)
            {
                restartedPreviousProcess = false;
            }
            else if (runtimeInMinutes == 0)
            {
                string message = "Restarted " + processInfoStruct.ExecutableName + ", which was not running.";
                status.AddFailureMessage(message);
                log.WriteLine(message);
            }
            else
            {
                int runtimeHours = runtimeInMinutes == 0 ? 0 : (int)Math.Floor(runtimeInMinutes / 60m);
                log.WriteLine("Restarted " + processInfoStruct.ExecutableName + ", which had been running for " + runtimeHours + " hours and " + (runtimeInMinutes - runtimeHours * 60) + " minutes.");
                if (mustRestartPreviousProcess)
                {
                    restartedPreviousProcess = true;
                }
                mustRestartPreviousProcess = false;
            }
        }

        public struct ProcessInfoStruct
        {
            public string ExecutableName;
            public ProcessStartInfo ProcessStartInfo;
            public bool WaitForExit;
            public int RestartAfterXHours;
            public bool RestartPreviousProcess;
            public string RequiredWindowName;
        }
    }
}
