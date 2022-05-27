using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Handler.Song.SongProvider.Tidal
{
    public class TidalProgressListener
    {

        private long _progress;

        private Process _process;
        private ulong? _progressAddress;

        private long _progressTime;

        private Task _listenerTask;
        private bool _disposed;

        public TidalProgressListener()
        {
            this._disposed = false;

            this._listenerTask = new Task(async t => await Listener(), Core.INSTANCE.CancellationTokenSource.Token);
        }

        private async Task Listener()
        {
            while (!this._disposed)
            {
                await Task.Delay(3000);

                if (!DataValidator.ValidateData(this._process))
                    continue;

                Process currentProcess = this._process;

            }
        }

        
        private void FindAddress()
        {
            this._process = FindTidalProcess();

            if (this._process == null)
                return;
        }

        private Process FindTidalProcess()
        {
            if (!DataValidator.ValidateData(this._process))
                return null;

            if (this._process != null || !this._process.HasExited)
                return null;

            Process[] processes = Process.GetProcessesByName("TIDAL");

            for (int i = 0; i < processes.Length; i++)
            {
                Process p = processes[i];

                if (!string.IsNullOrWhiteSpace(p.MainWindowTitle))
                {
                    return p;
                }
            }

            return null;
        }

    }
}
