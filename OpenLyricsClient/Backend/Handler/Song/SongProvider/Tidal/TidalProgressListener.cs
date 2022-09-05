using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;
using Squalr.Engine.DataTypes;
using Squalr.Engine.OS;
using Squalr.Engine.Scanning.Scanners;
using Squalr.Engine.Scanning.Scanners.Constraints;
using Squalr.Engine.Scanning.Snapshots;
using Squalr.Engine.Snapshots;

namespace OpenLyricsClient.Backend.Handler.Song.SongProvider.Tidal
{
    public class TidalProgressListener
    {
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
                await Task.Delay(500);

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

            if (this._retryTimes > 8)
            {
                this._debugger.Write("Too many failed search attempts", DebugType.ERROR);
                return;
            }

            this._debugger.Write("Trying to find a new address", DebugType.DEBUG);
            _retryTimes++;

            Processes.Default.OpenedProcess = tidalProcess;

            Snapshot snapshot = SnapshotManager.GetSnapshot(Snapshot.SnapshotRetrievalMode.FromSettings);
            snapshot.ElementDataType = DataType.Double;

            ScanConstraintCollection scanConstraint = new ScanConstraintCollection();

            double upper = (this._progressTime.ElapsedMilliseconds + 6875) / 1000D;
            double lower = (this._progressTime.ElapsedMilliseconds - 1000) / 1000D;

            scanConstraint.AddConstraint(new ScanConstraint(ScanConstraint.ConstraintType.LessThanOrEqual, upper));
            scanConstraint.AddConstraint(new ScanConstraint(ScanConstraint.ConstraintType.GreaterThanOrEqual, lower));

            snapshot = await ManualScanner.Scan(snapshot, DataType.Double, scanConstraint, null, out var scanCts);

            this._debugger.Write("Found " + snapshot.ElementCount + " Elements", DebugType.DEBUG);

            if (snapshot.ElementCount == 0)
            {
                this._debugger.Write("Address could not be found", DebugType.ERROR);
                Stop();
            }
            else if (snapshot.ElementCount <= 8)
            {
                this._progressAddress = snapshot[0].BaseAddress;
                this._debugger.Write("Found address " + $"0x{this._progressAddress.Value:X}", DebugType.INFO);
            }
        }

        public void Start()
        {
            this._retryTimes = 0;
            this._progressTime.Restart();
        }

        public void Stop()
        {
            this._retryTimes = 0;
            this._progressTime.Stop();
        }

        public ulong? ProgressAddress
        {
            get => _progressAddress;
            set => _progressAddress = value;
        }
    }
}
