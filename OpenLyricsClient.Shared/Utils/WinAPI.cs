using System;
using System.Runtime.InteropServices;

namespace OpenLyricsClient.Shared.Utils
{
    public class WinAPI
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        
        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint GetDpiForSystem();
    }
}
