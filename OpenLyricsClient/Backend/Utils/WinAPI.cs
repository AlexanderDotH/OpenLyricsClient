using System;
using System.Runtime.InteropServices;

namespace OpenLyricsClient.Backend.Utils
{
    public class WinAPI
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
    }
}
