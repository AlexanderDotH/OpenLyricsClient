using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevBase.Async.Task;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Structure.Enum;
using LyricsWPF.Backend.Utils;
using LyricsWPF.Backend.Utils.Service;
using Squalr.Engine.DataTypes;
using Squalr.Engine.Scanning.Scanners;
using Squalr.Engine.Scanning.Scanners.Constraints;
using Squalr.Engine.Scanning.Snapshots;
using Squalr.Engine.Snapshots;

namespace LyricsWPF.Backend.Handler.Song.SongProvider.Tidal
{
    public class TidalProgressListener
    {

        private long _progress;

        private Process _process;
        private ulong? _progressAddress;

        private Stopwatch _progressTime;

        private TaskSuspensionToken _findAddressSuspensionToken;
        private bool _disposed;

        private Debugger<TidalProgressListener> _debugger;

        private int _retryTimes;
        private CancellationTokenSource _tokenSource;

        public TidalProgressListener()
        {
            this._debugger = new Debugger<TidalProgressListener>(this);

            this._progressTime = new Stopwatch();

            this._disposed = false;

            this._retryTimes = 0;

            this._tokenSource = new CancellationTokenSource();

            Core.INSTANCE.TaskRegister.RegisterTask(
                out this._findAddressSuspensionToken, 
                new Task(async t => await Listener(), 
                    Core.INSTANCE.CancellationTokenSource.Token), EnumRegisterTypes.TIDALPROGRESSLISTENER_FINDADDRESS);
        }

        private async Task Listener()
        {
            while (!this._disposed)
            {
                await this._findAddressSuspensionToken.WaitForRelease();
                await Task.Delay(100);

                if (!DataValidator.ValidateData(this._process))
                {
                    this._process = TidalUtils.FindTidalProcess();
                    continue;
                }

                Process currentProcess = this._process;
                Process tidalProcess = TidalUtils.FindTidalProcess();

                if (currentProcess != tidalProcess)
                {
                    this._process = tidalProcess;
                    await FindAddress();
                } else if (this._progressAddress == null || this._retryTimes == 0)
                {
                    await FindAddress();
                }
            }
        }

        private async Task FindAddress()
        {
            if (!TidalUtils.IsTidalRunning())
                return;

            Process tidalProcess = TidalUtils.FindTidalProcess();

            if (tidalProcess == null)
                return;

            if (this._progressTime == null)
                return;

            if (this._retryTimes > 4)
                return;

            this._debugger.Write("Trying to find a new address", DebugType.DEBUG);

            Snapshot snapshot = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromSettings);
            snapshot.ElementDataType = DataType.Double;

            ScanConstraintCollection scanConstraint = new ScanConstraintCollection();

            double upper = (this._progressTime.ElapsedMilliseconds + 2000) / 1000D;
            double lower = (this._progressTime.ElapsedMilliseconds - 3000) / 1000D;

            scanConstraint.AddConstraint(new ScanConstraint(ScanConstraint.ConstraintType.LessThanOrEqual, upper));
            scanConstraint.AddConstraint(new ScanConstraint(ScanConstraint.ConstraintType.GreaterThanOrEqual, lower));

            snapshot = await ManualScanner.Scan(snapshot, DataType.Double, scanConstraint, null, out var scanCts);

            if (snapshot.ElementCount == 0)
            {
                this._debugger.Write("Address could not be found", DebugType.ERROR);
                this._retryTimes++;
            }
            else if (snapshot.ElementCount <= 4)
            {
                this._progressAddress = snapshot[0].BaseAddress;
                this._debugger.Write("Found address " + $"0x{this._progressAddress.Value:X}", DebugType.INFO);
            }
        }

        public void Start()
        {
            this._retryTimes = 0;
            this._progressTime.Start();
        }

        public void Stop()
        {
            this._progressTime.Stop();
        }

    }
}
