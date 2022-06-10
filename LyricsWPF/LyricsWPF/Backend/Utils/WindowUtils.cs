using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using LyricsWPF.Backend.Structure;

namespace LyricsWPF.Backend.Utils
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
