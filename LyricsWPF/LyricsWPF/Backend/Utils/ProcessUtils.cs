using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;

namespace LyricsWPF.Backend.Utils
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

            processesToSearch.ForEach(s =>
            {
                processes.ForEach(p =>
                {
                    if (p.ProcessName.Equals(s))
                    {
                        if (!foundProcesses.Contains(s))
                        {
                            foundProcesses.Add(s);
                        }
                    }
                });
            });

            return foundProcesses;
        }
    }
}
