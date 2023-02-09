using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using DevBase.IO;
using Newtonsoft.Json;
using OpenLyricsClient.Backend.Collector.Lyrics;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Events.EventHandler;
using OpenLyricsClient.Backend.Romanization;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using SharpDX.Text;
using SpotifyAPI.Web;

namespace OpenLyricsClient.Backend.Settings
{
    public class SettingManager
    {
        private Settings _settings;
        private AFileObject _settingsFilePath;
        private string _workingDirectory;

        private const string SETTING_FILE_NAME = "settings.json";
        
        public event SettingsChangedEventHandler SettingsChanged;
        
        public SettingManager(string workingFolder)
        {
            this._workingDirectory = workingFolder;
            
            this._settingsFilePath = new AFileObject(new FileInfo(string.Format("{0}{1}", 
                this._workingDirectory, SETTING_FILE_NAME)));

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

            settings.TidalAccess = (TidalAccess)DefaultSetting(EnumSetting.TIDAL);
            settings.SpotifyAccess = (SpotifyAccess)DefaultSetting(EnumSetting.SPOTIFY);
            settings.RomanizeSelection = (List<RomanizeSelection>)DefaultSetting(EnumSetting.ROMANIZATION);
            settings.MusixMatchToken = (List<MusixMatchToken>)DefaultSetting(EnumSetting.MUSIXMATCH_TOKENS);

            WriteSettings(settings);

            return settings;
        }

        public object DefaultSetting(EnumSetting enumSetting)
        {
            switch (enumSetting)
            {
                case EnumSetting.SPOTIFY:
                {
                    SpotifyAccess spotifyAccess = new SpotifyAccess();

                    spotifyAccess.AccessToken = "null";
                    spotifyAccess.IsSpotifyConnected = false;
                    spotifyAccess.RefreshToken = string.Empty;
                    spotifyAccess.SpotifyExpireTime = (int)DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    PrivateUser privateUser = new PrivateUser();
                    privateUser.Country = "";
                    privateUser.Email = "";
                    privateUser.Followers = null;
                    privateUser.Href = "";
                    privateUser.Id = "";
                    privateUser.Images = new List<Image>();
                    privateUser.Product = "";
                    privateUser.Type = "";
                    privateUser.Uri = "";
                    privateUser.DisplayName = "";

                    spotifyAccess.UserData = privateUser;

                    return spotifyAccess;
                }
                case EnumSetting.TIDAL:
                {
                    TidalAccess tidalAccess = new TidalAccess();
                    tidalAccess.IsTidalConnected = false;
                    tidalAccess.AccessToken = "null";
                    tidalAccess.RefreshToken = "null";
                    tidalAccess.ExpirationDate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    tidalAccess.UserID = 0;

                    return tidalAccess;
                }
                case EnumSetting.ROMANIZATION:
                {
                    List<RomanizeSelection> romanizeSelections = new List<RomanizeSelection>();
                    romanizeSelections.Add(RomanizeSelection.KOREAN_TO_ROMANJI);
                    romanizeSelections.Add(RomanizeSelection.JAPANESE_TO_ROMANJI);
                    romanizeSelections.Add(RomanizeSelection.RUSSIA_TO_LATIN);
                    return romanizeSelections;
                }
                case EnumSetting.MUSIXMATCH_TOKENS:
                {
                    return new List<MusixMatchToken>();
                }
            }

            return null;
        }

        public void WriteSettings(Settings settings, bool fireEvent = true)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

            File.WriteAllText(this._settingsFilePath.FileInfo.FullName, json);
            
            if (fireEvent)
                SettingsChangedEvent(new SettingsChangedEventArgs(settings));
        }
        
        public void WriteSettings(bool fireEvent = true)
        {
            if (!Core.IsLoaded())
                return;

            WriteSettings(this._settings, fireEvent);
        }
       
        private Settings ReadSettings()
        {
            if (!this._settingsFilePath.FileInfo.Exists)
                return null;

            Stream file = File.Open(this._settingsFilePath.FileInfo.FullName, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(file);

            string content = reader.ReadToEnd();

            reader.Close();
            file.Close();

            return JsonConvert.DeserializeObject<Settings>(content);
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
        
        protected virtual void SettingsChangedEvent(SettingsChangedEventArgs settingsChangedEventArgs)
        {
            SettingsChangedEventHandler settingsChangedEventHandler = SettingsChanged;
            settingsChangedEventHandler?.Invoke(this, settingsChangedEventArgs);
        }
    }
}
