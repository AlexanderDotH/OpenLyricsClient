using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using DevBase.Generics;

namespace OpenLyricsClient.Shared.Utils
{
    public class ProcessUtils
    {
        public static bool IsProcessRunning(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName.Replace(".exe", string.Empty));
            return processes.Length > 0;
        }

        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
        
        public static AList<string> GetRunningProcesses(params string[] processNames)
        {
            AList<string> processesToSearch = new AList<string>(processNames);
            AList<Process> processes = new AList<Process>(Process.GetProcesses());
            AList<string> foundProcesses = new AList<string>();

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
