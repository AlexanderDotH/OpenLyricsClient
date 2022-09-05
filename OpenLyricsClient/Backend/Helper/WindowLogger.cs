using System;
using System.Runtime.InteropServices;
using DevBase.Generic;
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

        public GenericList<string> LastWindows(params string[] processNames)
        {
            GenericList<string> names = new GenericList<string>(processNames);
            GenericList<string> returnList = new GenericList<string>();

            names.ForEach(n =>
            {
                if (!returnList.Contains(n) && IsLastWindow(n))
                {
                    returnList.Add(n);
                }
            });

            return returnList;
        }

        public bool IsLastWindow(string processName)
        {
            if (this._lastWindow != null && this._lastWindow.ProcessName == processName)
            {
                return true;
            }

            IntPtr pointer = IntPtr.Zero;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                pointer = WinAPI.GetForegroundWindow();
            }

            Window w = WindowUtils.GetWindowByPointer(pointer, processName);

            if (w != null) {
                if (w.ProcessName == processName)
                {
                    this._lastWindow = w;
                    return true;
                }
            }

            return false;
        }
    }
}
