using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Structure.Enum;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Helper
{
    public class WindowLogger
    {
        private GenericList<IntPtr> _lastPointer;
        private TaskSuspensionToken _suspensionToken;
        private bool _disposed;

        private Debugger<WindowLogger> _debugger;

        private Window _lastWindow;

        public WindowLogger()
        {
            this._lastPointer = new GenericList<IntPtr>();

            this._disposed = false;
            this._debugger = new Debugger<WindowLogger>(this);
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

        public void Dispose()
        {
            this._disposed = true;
        }
    }
}
