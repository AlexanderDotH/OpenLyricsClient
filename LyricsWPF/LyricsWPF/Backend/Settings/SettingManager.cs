using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.IO;
using LyricsWPF.Backend.Collector;
using Newtonsoft.Json;
using SpotifyApi.NetCore.Authorization;

namespace LyricsWPF.Backend.Settings
{
    public class SettingManager
    {
        private Settings _settings;
        private AFileObject _filePath;

        public SettingManager(string filePath)
        {
            this._filePath = new AFileObject(new FileInfo(filePath));
        }

        public void Setup()
        {
            if (this._filePath.FileInfo.Exists)
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

            BearerAccessRefreshToken bearerRefreshAccess = new BearerAccessRefreshToken();
            bearerRefreshAccess.RefreshToken = "empty";
            bearerRefreshAccess.AccessToken = "empty";
            bearerRefreshAccess.ExpiresIn = 0;

            // TODO: Remove unnecessary permissions
            bearerRefreshAccess.Scope = "playlist-read-private,playlist-read-collaborative,streaming,user-follow-read,user-library-read,user-read-private,user-read-playback-state,user-modify-playback-state,user-read-currently-playing,user-read-recently-played";
            settings.BearerAccess = bearerRefreshAccess;

            settings.IsSpotifyConnected = false;
            settings.SpotifyExpireTime = DateTime.Now;
            settings.RomanizeSelection = new List<RomanizeSelection>();
            settings.RomanizeSelection.Add(RomanizeSelection.JAPANESE_TO_ROMANJI);

            settings.LyricSelectionMode = SelectionMode.QUALITY;

            WriteSettings();

            return settings;
        }

        public void WriteSettings()
        {
            string json = JsonConvert.SerializeObject(this._settings, Formatting.Indented);
            File.WriteAllText(this._filePath.FileInfo.FullName, json);
        }

        private Settings ReadSettings()
        {
            if (!this._filePath.FileInfo.Exists)
                return null;

            return JsonConvert.DeserializeObject<Settings>(AFile.ReadFile(this._filePath.FileInfo).ToStringData());
        }
        
        public Settings Settings
        {
            get => _settings;
            set => _settings = value;
        }
    }
}
