using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Services.Services;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Backend.Utils.Service;
using TidalLib;

namespace OpenLyricsClient.Backend.Handler.Song.SongProvider.Tidal
{
    class TidalSongProvider : ISongProvider
    {
        private Structure.Song.Song _currentSong;
        private TidalProgressListener _tidalProgressListener;

        private IService _tidalService;

        private TaskSuspensionToken _loginTaskSuspensionToken;
        private TaskSuspensionToken _updateCurrentTrackSuspensionToken;
        private TaskSuspensionToken _updateTimeSuspensionToken;

        private bool _disposed;

        private Debugger<TidalSongProvider> _debugger;

        private long _startTime;

        private LoginKey _loginKey;

        private Process _tidalProcess;

        public TidalSongProvider()
        {
            this._debugger = new Debugger<TidalSongProvider>(this);

            this._disposed = false;

            this._tidalService = Core.INSTANCE.ServiceHandler.GetServiceByName("Tidal");

            this._tidalProgressListener = new TidalProgressListener();

            this._startTime = 0;

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _loginTaskSuspensionToken,
                new Task(async () => await LoginTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
                EnumRegisterTypes.TIDALSONGPROVIDER_LOGIN);

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _updateCurrentTrackSuspensionToken,
                new Task(async () => await UpdateCurrentTrack(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.TIDALSONGPROVIDER_UPDATECURRENTTRACK);

            Core.INSTANCE.TaskRegister.RegisterTask(
                out this._updateTimeSuspensionToken,
                new Task(async () => await UpdateTimeTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.TIDALSONGPROVIDER_UPDATETIME);
        }

        private async Task UpdateTimeTask()
        {
            while (!this._disposed)
            {
                await this._updateTimeSuspensionToken.WaitForRelease();

                await Task.Delay(100);

                if (!DataValidator.ValidateData(this._currentSong))
                    continue;

                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                long diff = currentTime - this._startTime;

                if (diff > 0)
                {
                    this._currentSong.Time = diff;
                }
            }
        }


        private async Task LoginTask()
        {
            while (!this._disposed)
            {
                await this._loginTaskSuspensionToken.WaitForRelease();

                TidalAccess tidalAccess = Core.INSTANCE.SettingManager.Settings.TidalAccess;

                if (!DataValidator.ValidateData(tidalAccess))
                    continue;

                if (!this._tidalService.IsConnected())
                    continue;

                if (tidalAccess.AccessToken.Equals("null"))
                    tidalAccess = Core.INSTANCE.SettingManager.Settings.TidalAccess;

                if (this._loginKey == null ||
                    tidalAccess != Core.INSTANCE.SettingManager.Settings.TidalAccess)
                {
                    (string s, LoginKey lg) = await Client.Login(tidalAccess.AccessToken);

                    if (!DataValidator.ValidateData(lg))
                    {
                        this._debugger.Write(s, DebugType.ERROR);
                        continue;
                    }

                    this._loginKey = lg;

                    Core.INSTANCE.SettingManager.Settings.TidalAccess.IsTidalConnected = true;
                    Core.INSTANCE.SettingManager.WriteSettings();

                    this._debugger.Write("Logged into Tidal!", DebugType.INFO);
                }

                await Task.Delay(5000);
            }
        }


        private async Task UpdateCurrentTrack()
        {
            while (!this._disposed)
            {
                await this._updateCurrentTrackSuspensionToken.WaitForRelease();

                await Task.Delay(500);

                Track tidalTrack = await FindTidalTrack();

                if (!TidalUtils.IsTidalRunning())
                    continue;
                
                this._currentSong = TidalDataMerger.ValidateUpdatePlayBack(this._currentSong, tidalTrack);
            }
        }

        public async Task<Structure.Song.Song> UpdateCurrentPlaybackTrack()
        {
            if (!this._tidalService.IsConnected())
                return null;

            if (!TidalUtils.IsTidalRunning())
                return null;

            Track tidalTrack = await FindTidalTrack();

            if (!DataValidator.ValidateData(tidalTrack))
                return null;

            this._currentSong = TidalDataMerger.ValidateConvertAndMerge(tidalTrack);

            this._startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            this._tidalProgressListener.Start();

            return this._currentSong;
        }

        private async Task<Track> FindTidalTrack()
        {
            if (!TidalUtils.IsTidalRunning())
                return null;

            Process tidalProcess = TidalUtils.FindTidalProcess();

            if (!DataValidator.ValidateData(tidalProcess))
                return null;

            if (!DataValidator.ValidateData(this._loginKey))
                return null;

            if (!tidalProcess.MainWindowTitle.Contains("-"))
                return null;

            string title = tidalProcess.MainWindowTitle.Split('-')[0].Trim();
            string artists = tidalProcess.MainWindowTitle.Split('-')[1].Trim().Replace(",", " /");

            (string error, SearchResult searchResult) = await Client.Search(this._loginKey, title + " " + artists, iLimit:100);

            if (!DataValidator.ValidateData(searchResult))
            {
                this._debugger.Write(error, DebugType.ERROR);
            }

            ObservableCollection<Track> tracks = searchResult.Tracks;

            for (int i = 0; i < tracks.Count; i++)
            {
                Track currentTrack = tracks[i];

                string title1 = currentTrack.Title.ToLower();
                string title2 = title1.ToLower();

                if (title1.Equals(title2) &&
                    currentTrack.ArtistsName.Equals(artists))
                {
                    return currentTrack;
                }
            }

            return null;
        }

        public Structure.Song.Song GetCurrentSong()
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

            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.TIDALSONGPROVIDER_LOGIN, EnumRegisterTypes.TIDALSONGPROVIDER_UPDATECURRENTTRACK, EnumRegisterTypes.TIDALSONGPROVIDER_UPDATETIME);
        }
    }
}
