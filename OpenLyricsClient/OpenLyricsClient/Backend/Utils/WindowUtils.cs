using System;
using System.Diagnostics;
using OpenLyricsClient.Backend.Structure;

namespace OpenLyricsClient.Backend.Utils
{
    public class WindowUtils
    {
        public static Window GetWindowByPointer(IntPtr pointer, string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);

            for (int i = 0; i < processes.Length; i++)
            {
                Process p = processes[i];

                if (p.MainWindowHandle == pointer || p.Handle == pointer)
                {
                    return new Window(p.MainWindowTitle, p.ProcessName);
                }
            }

            return null;
        }
    }
}
