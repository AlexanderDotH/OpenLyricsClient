using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Structure.Enum;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Helper
{
    public class WindowLogger
    {
        private GenericList<Window> _lastWindows;
        private TaskSuspensionToken _suspensionToken;
        private bool _disposed;

        public WindowLogger()
        {
            this._lastWindows = new GenericList<Window>();

            this._disposed = false;

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _suspensionToken, 
                new Task(async () => await LogWindowTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
                EnumRegisterTypes.WINDOW_LOGGER);
        }

        private async Task LogWindowTask()
        {
            while (!this._disposed)
            {
                await this._suspensionToken.WaitForRelease();

                await Task.Delay(100);

                Window window = null;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    window = WindowUtils.GetWindowByPointer(WinAPI.GetForegroundWindow());
                }

                if (!DataValidator.ValidateData(window))
                    continue;

                this._lastWindows.Add(window);
            }
        }


        public bool IsLastWindow(string processName)
        {
            for (int i = this._lastWindows.Length - 1; i > 0; i--)
            {
                Window window = this._lastWindows[i];

                if (window.ProcessName.Equals(processName))
                {
                    return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            this._disposed = true;

            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.WINDOW_LOGGER);
        }
    }
}
