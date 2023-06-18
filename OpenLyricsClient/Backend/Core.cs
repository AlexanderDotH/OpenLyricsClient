using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Cache;
using OpenLyricsClient.Backend.Collector.Song;
using OpenLyricsClient.Backend.Collector.Token;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Events.EventHandler;
using OpenLyricsClient.Backend.Handler.Artwork;
using OpenLyricsClient.Backend.Handler.ColorHandler;
using OpenLyricsClient.Backend.Handler.License;
using OpenLyricsClient.Backend.Handler.Lyrics;
using OpenLyricsClient.Backend.Handler.Services;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Helper;
using OpenLyricsClient.Shared.Plugin;
using OpenLyricsClient.Backend.Settings;
using OpenLyricsClient.Backend.Settings.Sections.Connection.Spotify;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Enum;
using TaskRegister = OpenLyricsClient.Backend.Overwrite.TaskRegister;

namespace OpenLyricsClient.Backend
{
    class Core
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
        
        private CacheManager _cacheManager;

        private TokenCollector _tokenCollector;

        private static bool _disposed;

        private CancellationTokenSource _cancellationTokenSource;
        private TaskRegister _taskRegister;

        private WindowLogger _windowLogger;

        private TaskSuspensionToken _tickSuspensionToken;

        private PluginManager _pluginManager;

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
            
            this._settingsHandler = new SettingsHandler(workingDirectory);
            
            this._licenseHandler = new LicenseHandler();
            
            this._cacheManager = new CacheManager(workingDirectory, 10, TimeSpan.FromMinutes(5).Milliseconds);

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
