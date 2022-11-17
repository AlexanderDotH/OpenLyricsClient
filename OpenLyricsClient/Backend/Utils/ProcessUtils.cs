using System;
using System.Diagnostics;
using DevBase.Generic;

namespace OpenLyricsClient.Backend.Utils
{
    public class ProcessUtils
    {
        public static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName.Replace(".exe", string.Empty));
            return processes.Length > 0;
        }

        public static GenericList<string> GetRunningProcesses(params string[] processNames)
        {
            GenericList<string> processesToSearch = new GenericList<string>(processNames);
            GenericList<Process> processes = new GenericList<Process>(Process.GetProcesses());
            GenericList<string> foundProcesses = new GenericList<string>();

            for (int i = 0; i < processesToSearch.Length; i++)
            {
                string pts = processesToSearch.Get(i);

                for (int j = 0; j < processes.Length; j++)
                {
                    Process p = processes.Get(i);
                    
                    if (p.ProcessName.Equals(pts))
                    {
                        if (!foundProcesses.Contains(pts))
                        {
                            foundProcesses.Add(pts);
                        }
                    }
                }
            }

            return foundProcesses;
        }
    }
}
