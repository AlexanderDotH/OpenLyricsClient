using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Handler.Lyrics;
using LyricsWPF.Backend.Handler.Services;
using LyricsWPF.Backend.Handler.Song;
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

        private Settings.Settings _settings;

        private ServiceHandler _serviceHandler;
        private SongHandler _songHandler;
        private LyricHandler _lyricHandler;

        private static bool _disposed;

        public Core()
        {
            INSTANCE = this;
            this._debugger = new Debugger<Core>(this);

            _disposed = false;

            SetupInternals();

            this._serviceHandler = new ServiceHandler();
            this._songHandler = new SongHandler();
            this._lyricHandler = new LyricHandler(this._songHandler);
        }

        public void SetupInternals()
        {

            if (File.Exists("settings.json"))
            {
                Settings.Settings settings =
                    JsonConvert.DeserializeObject<Settings.Settings>(File.ReadAllText("settings.json"));

                if (settings != null)
                {
                    this._settings = settings;
                }
                else
                {
                    this._settings = GenerateAndWriteSettings();
                }
            }
            else
            {
                this._settings = GenerateAndWriteSettings();
            }
        }

        public Settings.Settings GenerateAndWriteSettings()
        {
            LyricsWPF.Backend.Settings.Settings settings = new Settings.Settings();

            BearerAccessRefreshToken bearerRefreshAccess = new BearerAccessRefreshToken();
            bearerRefreshAccess.RefreshToken = "empty";
            bearerRefreshAccess.AccessToken = "empty";
            bearerRefreshAccess.ExpiresIn = 0;
            bearerRefreshAccess.Scope = "playlist-read-private,playlist-read-collaborative,streaming,user-follow-read,user-library-read,user-read-private,user-read-playback-state,user-modify-playback-state,user-read-currently-playing,user-read-recently-played";
            settings.BearerAccess = bearerRefreshAccess;

            settings.IsSpotifyConnected = false;
            settings.SpotifyExpireTime = DateTime.Now;

            WriteSettings();

            return settings;
        }

        public void WriteSettings()
        {
            string json = JsonConvert.SerializeObject(this._settings, Formatting.Indented);
            File.WriteAllText("settings.json", json);
        }

        public void DisposeEverything()
        {
            _disposed = true;

            this._songHandler.Dispose();
            this._lyricHandler.Dispose();
            this._serviceHandler.Dispose();
        }

        public Settings.Settings Settings
        {
            get { return this._settings; }
            set
            {
                this._settings = value;
            }
        }

        public ServiceHandler ServiceHandler
        {
            get { return this._serviceHandler; }
        }

        public SongHandler SongHandler
        {
            get { return this._songHandler; }
        }

        public static bool IsDisposed()
        {
            return _disposed;
        }

    }
}
