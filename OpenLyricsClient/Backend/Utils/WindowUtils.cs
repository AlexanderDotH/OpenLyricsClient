using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using Avalonia.Platform;
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

        private static int GetWindowsScalingFactor()
        {
            switch (WinAPI.GetDpiForSystem())
            {
                case 96: return 100;
                case 120: return 125;
                case 144: return 150;
                case 160: return 175;
                default: return 100;
            }
        }
        
        public static double GetScalingFactor()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return 1.0;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return 1.0;

            return (220 - GetWindowsScalingFactor()) / 100;
        }
    }
}
