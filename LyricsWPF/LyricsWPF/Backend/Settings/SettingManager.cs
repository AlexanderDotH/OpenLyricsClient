using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.IO;
using LyricsWPF.Backend.Collector;
using LyricsWPF.Backend.Romanisation;
using LyricsWPF.Backend.Structure;
using Newtonsoft.Json;
using SpotifyApi.NetCore.Authorization;

namespace LyricsWPF.Backend.Settings
{
    public class SettingManager
    {
        private Settings _settings;
        private AFileObject _settingsFilePath;
        private string _workingDirectory;

        private const string SETTING_FILE_NAME = "settings.json";
        
        public SettingManager(string workingFolder)
        {
            this._settingsFilePath = new AFileObject(new FileInfo(workingFolder + "\\" + SETTING_FILE_NAME));

            this._workingDirectory = workingFolder;

            Setup();
        }

        public void Setup()
        {
            if (!Directory.Exists(this._workingDirectory))
                Directory.CreateDirectory(this._workingDirectory);

            if (this._settingsFilePath.FileInfo.Exists)
            {
                Settings settings = ReadSettings();

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

        private Settings GenerateAndWriteSettings()
        {
            Settings settings = new Settings();

            TidalAccess tidalAccess = new TidalAccess();
            tidalAccess.AccessToken = "null";
            tidalAccess.RefreshToken = "null";
            tidalAccess.ApiToken = "null";
            tidalAccess.UniqueKey = "null";
            tidalAccess.ExpirationDate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            tidalAccess.UserID = 0;

            settings.TidalAccess = tidalAccess;

            SpotifyAccess spotifyAccess = new SpotifyAccess();

            BearerAccessToken bearerAccessToken = new BearerAccessToken();
            bearerAccessToken.AccessToken = "null";
            bearerAccessToken.ExpiresIn = 0;
            bearerAccessToken.Scope = "playlist-read-private,playlist-read-collaborative,streaming,user-follow-read,user-library-read,user-read-private,user-read-playback-state,user-modify-playback-state,user-read-currently-playing,user-read-recently-played";

            spotifyAccess.BearerAccess = bearerAccessToken;

            spotifyAccess.IsSpotifyConnected = false;
            spotifyAccess.RefreshToken = string.Empty;
            spotifyAccess.SpotifyExpireTime = DateTime.Now;
            settings.SpotifyAccess = spotifyAccess;

            List<RomanizeSelection> romanizeSelections = new List<RomanizeSelection>();
            romanizeSelections.Add(RomanizeSelection.KOREAN_TO_ROMANJI);
            romanizeSelections.Add(RomanizeSelection.JAPANESE_TO_ROMANJI);
            settings.RomanizeSelection = romanizeSelections;

            settings.LyricSelectionMode = SelectionMode.QUALITY;

            WriteSettings(settings);

            return settings;
        }

        public void WriteSettings(Settings settings)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(this._settingsFilePath.FileInfo.FullName, json);
        }

        public void WriteSettings()
        {
            WriteSettings(this._settings);
        }

        private Settings ReadSettings()
        {
            if (!this._settingsFilePath.FileInfo.Exists)
                return null;

            return JsonConvert.DeserializeObject<Settings>(AFile.ReadFile(this._settingsFilePath.FileInfo).ToStringData());
        }
        
        public Settings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        public string WorkingDirectory
        {
            get => _workingDirectory;
        }
    }
}
