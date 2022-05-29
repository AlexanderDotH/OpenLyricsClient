using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.Utilities;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Events.EventArgs;
using LyricsWPF.Backend.Handler.Services.Services;
using LyricsWPF.Backend.Handler.Services.Services.Tidal;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;
using LyricsWPF.Backend.Utils.Service;
using TidalLib;

namespace LyricsWPF.Backend.Handler.Song.SongProvider.Tidal
{
    class TidalSongProvider : ISongProvider
    {

        private Song _currentSong;
        private TidalProgressListener _tidalProgressListener;

        private IService _tidalService;

        private Task _loginTask;
        private Task _updateTrackTask;
        private bool _disposed;

        private TidalAccess _tidalAccess;
        private LoginKey _loginKey;

        private Debugger<TidalSongProvider> _debugger;

        public TidalSongProvider()
        {
            this._debugger = new Debugger<TidalSongProvider>(this);

            this._disposed = false;

            this._tidalService = Core.INSTANCE.ServiceHandler.GetServiceByName("Tidal");

            this._tidalProgressListener = new TidalProgressListener();

            this._tidalAccess = Core.INSTANCE.SettingManager.Settings.TidalAccess;

            this._loginTask = new Task(async t => await LoginTask(), Core.INSTANCE.CancellationTokenSource.Token);
            this._loginTask.Start();

            this._updateTrackTask = new Task(async t => await UpdateCurrentTrack(), Core.INSTANCE.CancellationTokenSource.Token);
            this._updateTrackTask.Start();
        }

        private async Task LoginTask()
        {
            while (!this._disposed)
            {
                await Task.Delay(500);

                if (!DataValidator.ValidateData(this._tidalAccess))
                    continue;

                if (!this._tidalService.IsConnected())
                    continue;

                if (this._tidalAccess.AccessToken.Equals("null"))
                    this._tidalAccess = Core.INSTANCE.SettingManager.Settings.TidalAccess;

                if (this._loginKey == null ||
                    this._tidalAccess != Core.INSTANCE.SettingManager.Settings.TidalAccess)
                {
                    (string s, LoginKey lg) = await Client.Login(this._tidalAccess.AccessToken);

                    if (!DataValidator.ValidateData(lg))
                    {
                        this._debugger.Write(s, DebugType.ERROR);
                        continue;
                    }

                    this._loginKey = lg;

                    Core.INSTANCE.SettingManager.Settings.TidalAccess.IsTidalConnected = true;
                    Core.INSTANCE.SettingManager.WriteSettings();
                     
                    this._tidalAccess = Core.INSTANCE.SettingManager.Settings.TidalAccess;


                    this._debugger.Write("Logged into Tidal!", DebugType.INFO);
                }
            }
        }

        private async Task UpdateCurrentTrack()
        {
            while (!this._disposed)
            {
                await Task.Delay(500);

                Track tidalTrack = await FindTidalTrack();

                if (!DataValidator.ValidateData(tidalTrack))
                    return;
                
                this._currentSong = TidalDataMerger.ValidateUpdatePlayBack(this._currentSong, tidalTrack);
            }
        }

        public async Task<Song> UpdateCurrentPlaybackTrack()
        {
            if (!this._tidalService.IsConnected())
                return null;

            if (!TidalUtils.IsTidalRunning())
                return null;

            if (!DataValidator.ValidateData(this._loginKey))
                return null;


            Track tidalTrack = await FindTidalTrack();

            if (!DataValidator.ValidateData(tidalTrack))
                return null;

            this._currentSong = TidalDataMerger.ConvertAndMerge(tidalTrack);
            return this._currentSong;
        }

        private async Task<Track> FindTidalTrack()
        {
            Process tidalProcess = TidalUtils.FindTidalProcess();

            if (!DataValidator.ValidateData(tidalProcess))
                return null;

            if (!DataValidator.ValidateData(this._loginKey))
                return null;

            string title = tidalProcess.MainWindowTitle.Split('-')[0].Trim();
            string artists = tidalProcess.MainWindowTitle.Split('-')[1].Trim().Replace(",", " /");

            (string error, SearchResult searchResult) = await Client.Search(this._loginKey, title);

            if (!DataValidator.ValidateData(searchResult))
            {
                this._debugger.Write(error, DebugType.ERROR);
            }

            ObservableCollection<Track> tracks = searchResult.Tracks;

            for (int i = 0; i < tracks.Count; i++)
            {
                Track currentTrack = tracks[i];

                if (currentTrack.Title.Equals(title) &&
                    currentTrack.ArtistsName.Equals(artists))
                {
                    return currentTrack;
                }
            }

            return null;
        }


        public Song GetCurrentSong()
        {
            return this._currentSong;
        }

        public EnumSongProvider GetEnum()
        {
            return EnumSongProvider.TIDAL;
        }


        public void Dispose()
        {
            this._disposed = true;
        }
    }
}
