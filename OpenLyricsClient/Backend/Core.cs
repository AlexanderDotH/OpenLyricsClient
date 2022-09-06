using System;
using System.Threading;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Cache;
using OpenLyricsClient.Backend.Collector.Token;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Lyrics;
using OpenLyricsClient.Backend.Handler.Services;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Helper;
using OpenLyricsClient.Backend.Settings;
using OpenLyricsClient.Backend.Structure.Enum;

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
        private CacheManager _cacheManager;

        private TokenCollector _tokenCollector;

        private static bool _disposed;

        private CancellationTokenSource _cancellationTokenSource;
        private TaskRegister _taskRegister;

        private WindowLogger _windowLogger;
        private Environment.Environment _environment;

        public Core()
        {
            INSTANCE = this;

            _loaded = false;

            _disposed = false;
            
            this._cancellationTokenSource = new CancellationTokenSource();

            this._debugger = new Debugger<Core>(this);
            this._environment = Backend.Environment.Environment.FindEnvironmentFile(System.Environment.GetCommandLineArgs());

            this._taskRegister = new TaskRegister();

            this._windowLogger = new WindowLogger();

            this._settingManager = new SettingManager(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\OpenLyricsClient\\");

            this._cacheManager = new CacheManager();

            this._tokenCollector = new TokenCollector();

            this._serviceHandler = new ServiceHandler();
            this._songHandler = new SongHandler();
            this._lyricHandler = new LyricHandler(this._songHandler);

            SpotifyApi.NetCore.Logger.LoggingEnabled = false;
            
            _loaded = true;
        }

        public void DisposeEverything()
        {
            _disposed = true;
            _cancellationTokenSource.Cancel();

            this.TaskRegister.Kill(EnumRegisterTypes.SHOW_LYRICS, EnumRegisterTypes.SHOW_FULLLYRICS, EnumRegisterTypes.SHOW_PROGRESS, EnumRegisterTypes.SHOW_INFOS);
            this.TaskRegister.Kill(EnumRegisterTypes.COLLECT_TOKENS);

            this._songHandler.Dispose();
            this._lyricHandler.Dispose();
            this._serviceHandler.Dispose();
        }

        public SettingManager SettingManager
        {
            get => _settingManager;
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
