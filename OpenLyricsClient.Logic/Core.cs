using DevBase.Async.Task;
using OpenLyricsClient.Logic.Authentication;
using OpenLyricsClient.Logic.Cache;
using OpenLyricsClient.Logic.Collector.Token;
using OpenLyricsClient.Logic.Communication;
using OpenLyricsClient.Logic.Debugger;
using OpenLyricsClient.Logic.Events.EventHandler;
using OpenLyricsClient.Logic.Handler.Artwork;
using OpenLyricsClient.Logic.Handler.Color;
using OpenLyricsClient.Logic.Handler.Debug;
using OpenLyricsClient.Logic.Handler.License;
using OpenLyricsClient.Logic.Handler.Lyrics;
using OpenLyricsClient.Logic.Handler.Services;
using OpenLyricsClient.Logic.Handler.Song;
using OpenLyricsClient.Logic.Helper;
using OpenLyricsClient.Logic.Plugins;
using OpenLyricsClient.Logic.Settings;
using OpenLyricsClient.Shared.Structure.Enum;
using TaskRegister = OpenLyricsClient.Logic.Overwrite.TaskRegister;

namespace OpenLyricsClient.Logic
{
    public class Core
    {
        public static Core INSTANCE;
        public const bool DEBUG_MODE = true;

        private static bool _loaded;

        private Debugger<Core> _debugger;

        private Sealing _sealing;
        private SettingsHandler _settingsHandler;

        private ServiceHandler _serviceHandler;
        private SongHandler _songHandler;
        private LyricHandler _lyricHandler;
        private ArtworkHandler _artworkHandler;
        private LicenseHandler _licenseHandler;
        private ColorHandler _colorHandler;
        private DebugHandler _debugHandler;
        
        private CacheManager _cacheManager;

        private TokenCollector _tokenCollector;

        private static bool _disposed;

        private CancellationTokenSource _cancellationTokenSource;
        private TaskRegister _taskRegister;

        private WindowLogger _windowLogger;

        private TaskSuspensionToken _tickSuspensionToken;

        private PluginManager _pluginManager;

        private InterProcessService _interProcessService;
        private AuthenticationPipe _authenticationPipe;

        public event TickEventHandler TickHandler;
        public event SlowTickEventHandler SlowTickHandler;

        public Core()
        {
            INSTANCE = this;

            _loaded = false;

            _disposed = false;
            
            this._cancellationTokenSource = new CancellationTokenSource();

            this._debugger = new Debugger<Core>(this);

            this._taskRegister = new TaskRegister();

            TaskRegister.Register(
                out _tickSuspensionToken,
                new Task(async () => await this.TickTask(), CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.GLOBAL_TICK);
            
            TaskRegister.Register(
                out _tickSuspensionToken,
                new Task(async () => await this.SlowTickTask(), CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.GLOBAL_TICK);
            
            this._windowLogger = new WindowLogger();

            this._sealing = new Sealing();
            
            string workingDirectory =
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) +
                string.Format("{0}OpenLyricsClient{0}", System.IO.Path.DirectorySeparatorChar);

            this._debugHandler = new DebugHandler(Path.Combine(workingDirectory, "Logs"));
            
            this._settingsHandler = new SettingsHandler(workingDirectory);
            
            this._licenseHandler = new LicenseHandler();
            
            this._cacheManager = new CacheManager(workingDirectory, 20, TimeSpan.FromMinutes(5).Milliseconds);

            this._tokenCollector = new TokenCollector();

            this._serviceHandler = new ServiceHandler();

            this._pluginManager = new PluginManager(workingDirectory);
            this._pluginManager.LoadPlugins(); // TODO: find where to put it
            
            SongHandler songHandler = new SongHandler();
            
            this._lyricHandler = new LyricHandler(songHandler);
            
            this._artworkHandler = new ArtworkHandler(songHandler);
            this._colorHandler = new ColorHandler();
            
            songHandler.InitializeSongCollector(this._lyricHandler, this._artworkHandler);
            
            this._songHandler = songHandler;

            _loaded = true;

            Task.Factory.StartNew(async () => await SettingsHandler.TriggerGlobal());
            
            this._interProcessService = new InterProcessService("openlyricsclient", null);
            this._authenticationPipe = new AuthenticationPipe();
        }

        private async Task TickTask()
        {
            while (!_disposed)
            {
                await Task.Delay(10);
                this.TickEvent();
            }
        }
        
        private async Task SlowTickTask()
        {
            while (!_disposed)
            {
                await Task.Delay(3000);
                this.SlowTickEvent();
            }
        }
        
        public void DisposeEverything()
        {
            _disposed = true;
            _cancellationTokenSource.Cancel();

            this.TaskRegister.Kill(EnumRegisterTypes.SHOW_LYRICS, EnumRegisterTypes.SHOW_FULLLYRICS, EnumRegisterTypes.SHOW_PROGRESS, EnumRegisterTypes.SHOW_INFOS);
            this.TaskRegister.Kill(EnumRegisterTypes.COLLECT_TOKENS);
            this.TaskRegister.Kill(EnumRegisterTypes.GLOBAL_TICK);

            this._songHandler.Dispose();
            this._lyricHandler.Dispose();
            this._serviceHandler.Dispose();
            this._licenseHandler.Dispose();
        }
        
        protected virtual void TickEvent()
        {
            TickEventHandler tickEventHandler = TickHandler;
            tickEventHandler?.Invoke(this);
        }
        
        protected virtual void SlowTickEvent()
        {
            SlowTickEventHandler slowTickEventHandler = SlowTickHandler;
            slowTickEventHandler?.Invoke(this);
        }

        public Sealing Sealing
        {
            get => _sealing;
        }

        public SettingsHandler SettingsHandler
        {
            get => this._settingsHandler;
        }

        public PluginManager PluginManager
        {
            get => this._pluginManager;
        }

        public ArtworkHandler ArtworkHandler
        {
            get => _artworkHandler;
        }

        public ServiceHandler ServiceHandler
        {
            get { return this._serviceHandler; }
        }

        public SongHandler SongHandler
        {
            get { return this._songHandler; }
        }

        public TaskRegister TaskRegister
        {
            get => _taskRegister;
        }

        public WindowLogger WindowLogger
        {
            get => _windowLogger;
        }

        public CacheManager CacheManager
        {
            get => _cacheManager;
        }

        public ColorHandler ColorHandler
        {
            get => _colorHandler;
        }

        public CancellationTokenSource CancellationTokenSource
        {
            get { return this._cancellationTokenSource; }
        }

        public LyricHandler LyricHandler
        {
            get => this._lyricHandler;
        }

        public LicenseHandler LicenseHandler
        {
            get => _licenseHandler;
        }

        public InterProcessService InterProcessService
        {
            get => _interProcessService;
        }

        public AuthenticationPipe AuthenticationPipe
        {
            get => _authenticationPipe;
        }

        public DebugHandler DebugHandler
        {
            get => _debugHandler;
        }

        public static bool IsDisposed()
        {
            return _disposed;
        }

        public static bool IsLoaded()
        {
            return _loaded;
        }
    }
}
