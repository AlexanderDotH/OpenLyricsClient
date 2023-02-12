using System;
using System.Runtime.InteropServices;
using Avalonia;
using DevBase.Generics;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Helper
{
    public class WindowLogger
    {
        private Window _lastWindow;

        public WindowLogger()
        {
            this._lastWindow = null;
        }

        public AList<string> LastWindows(params string[] processNames)
        {
            AList<string> names = new AList<string>(processNames);
            AList<string> returnList = new AList<string>();

            for (int i = 0; i < names.Length; i++)
            {
                string n = names.Get(i);
                
                if (!returnList.Contains(n) && IsLastWindow(n))
                {
                    returnList.Add(n);
                }
            }
            
            return returnList;
        }

        public bool IsLastWindow(string processName)
        {
            if (this._lastWindow != null && this._lastWindow.ProcessName == processName)
            {
                return true;
            }

            IntPtr pointer = IntPtr.Zero;
            Window w = null;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                pointer = WinAPI.GetForegroundWindow();
                w = WindowUtils.GetWindowByPointer(pointer, processName);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                w = Utils.X11.GetFocusedWindow();
            }
            
            if (!DataValidator.ValidateData(w))
                return false;

            if (w != null) {
                if (w.ProcessName.ToLower().Equals(processName.ToLower()))
                {
                    this._lastWindow = w;
                    return true;
                }
            }

            return false;
        }
    }
}
