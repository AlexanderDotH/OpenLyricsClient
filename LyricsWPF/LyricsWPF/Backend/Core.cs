using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using DevBase.Async.Task;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Handler.Lyrics;
using LyricsWPF.Backend.Handler.Services;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Helper;
using LyricsWPF.Backend.Settings;
using LyricsWPF.Backend.Structure.Enum;
using LyricsWPF.Backend.Utils;
using Newtonsoft.Json;
using SpotifyApi.NetCore;
using SpotifyApi.NetCore.Authorization;

namespace LyricsWPF.Backend
{
    // Ideas:
    // Textfarbe im fade zum nächsten lyric zeitpunkt ändern,
    // also die aktuelle textfarbe ist grün und wird rot, wenn der textabschnitt dran kommt

    class Core
    {
        public static Core INSTANCE;
        public const bool DEBUG_MODE = true;

        private Debugger<Core> _debugger;

        private SettingManager _settingManager;

        private ServiceHandler _serviceHandler;
        private SongHandler _songHandler;
        private LyricHandler _lyricHandler;

        private static bool _disposed;

        private CancellationTokenSource _cancellationTokenSource;
        private TaskRegister _taskRegister;

        private WindowLogger _windowLogger;

        public Core()
        {
            INSTANCE = this;

            this._debugger = new Debugger<Core>(this);

            _disposed = false;
            this._cancellationTokenSource = new CancellationTokenSource();

            this._taskRegister = new TaskRegister();

            this._windowLogger = new WindowLogger();

            this._settingManager = new SettingManager(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Lyrics\\");

            this._serviceHandler = new ServiceHandler();
            this._songHandler = new SongHandler();
            this._lyricHandler = new LyricHandler(this._songHandler);
        }
        
        public void DisposeEverything()
        {
            _disposed = true;
            _cancellationTokenSource.Cancel();

            this.TaskRegister.Kill(EnumRegisterTypes.SHOW_LYRICS, EnumRegisterTypes.SHOW_PROGRESS, EnumRegisterTypes.SHOW_INFOS);
            this.TaskRegister.Kill(EnumRegisterTypes.MUSIXMATCH_COLLECT_TOKENS);

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

        public CancellationTokenSource CancellationTokenSource
        {
            get { return this._cancellationTokenSource; }
        }

        public static bool IsDisposed()
        {
            return _disposed;
        }

    }
}
