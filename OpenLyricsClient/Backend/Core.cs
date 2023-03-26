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
using OpenLyricsClient.Backend.Handler.Lyrics;
using OpenLyricsClient.Backend.Handler.Services;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Helper;
using OpenLyricsClient.Backend.Settings;
using OpenLyricsClient.Backend.Structure.Enum;
using TaskRegister = OpenLyricsClient.Backend.Overwrite.TaskRegister;

namespace OpenLyricsClient.Backend
{
    // Ideas:
    // Textfarbe im fade zum nächsten lyric zeitpunkt ändern,
    // also die aktuelle textfarbe ist grün und wird rot, wenn der textabschnitt dran kommt

    class Core
    {
        public static Core INSTANCE;
        public const bool DEBUG_MODE = true;

        private static bool _loaded;

        private Debugger<Core> _debugger;

        private SettingManager _settingManager;

        private ServiceHandler _serviceHandler;
        private SongHandler _songHandler;
        private LyricHandler _lyricHandler;
        private ArtworkHandler _artworkHandler;
        
        private CacheManager _cacheManager;

        private TokenCollector _tokenCollector;

        private static bool _disposed;

        private CancellationTokenSource _cancellationTokenSource;
        private TaskRegister _taskRegister;

        private WindowLogger _windowLogger;
        private Environment.Environment _environment;

        private TaskSuspensionToken _tickSuspensionToken;

        public event TickEventHandler TickHandler;
        public event SlowTickEventHandler SlowTickHandler;

        public Core()
        {
            INSTANCE = this;

            _loaded = false;

            _disposed = false;
            
            this._cancellationTokenSource = new CancellationTokenSource();

            this._debugger = new Debugger<Core>(this);
            this._environment = Backend.Environment.Environment.FindEnvironmentFile(System.Environment.GetCommandLineArgs());

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

            this._settingManager = new SettingManager(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + 
                                                      string.Format("{0}OpenLyricsClient{0}", System.IO.Path.DirectorySeparatorChar));

            this._cacheManager = new CacheManager(10, TimeSpan.FromMinutes(5).Milliseconds);

            this._tokenCollector = new TokenCollector();

            this._serviceHandler = new ServiceHandler();
            
            SongHandler songHandler = new SongHandler();
            
            this._lyricHandler = new LyricHandler(songHandler);
            this._artworkHandler = new ArtworkHandler(songHandler);
            
            songHandler.InitializeSongCollector(this._lyricHandler, this._artworkHandler);
            
            this._songHandler = songHandler;

            _loaded = true;
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

        public SettingManager SettingManager
        {
            get => _settingManager;
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

        public Environment.Environment Environment
        {
            get => this._environment;
        }

        public LyricHandler LyricHandler
        {
            get => this._lyricHandler;
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
