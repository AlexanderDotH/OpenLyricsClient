using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using DevBase.Typography;
using DevBaseApi.Apis.Tidal;
using DevBaseApi.Apis.Tidal.Structure.Json;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Services.Services;
using OpenLyricsClient.Backend.Handler.Services.Services.Tidal;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;
using Squalr.Engine.Memory;

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

        private JsonTidalSession _session;
        private TidalClient _tidalClient;

        private Process _tidalProcess;

        public TidalSongProvider()
        {
            this._debugger = new Debugger<TidalSongProvider>(this);

            this._disposed = false;

            this._tidalService = Core.INSTANCE.ServiceHandler.GetServiceByName("Tidal");
            this._tidalClient = new TidalClient();

            this._tidalProgressListener = new TidalProgressListener();

            this._startTime = 0;

            Core.INSTANCE.TaskRegister.Register(
                out _loginTaskSuspensionToken,
                new Task(async () => await LoginTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
                EnumRegisterTypes.TIDALSONGPROVIDER_LOGIN);

            Core.INSTANCE.TaskRegister.Register(
                out _updateCurrentTrackSuspensionToken,
                new Task(async () => await UpdateCurrentTrack(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.TIDALSONGPROVIDER_UPDATECURRENTTRACK);

            Core.INSTANCE.TaskRegister.Register(
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

                if (!DataValidator.ValidateData(this._tidalProgressListener))
                    continue;

                if (!DataValidator.ValidateData(this._tidalProgressListener.ProgressAddress))
                    continue;

                if (!this._tidalProgressListener.ProgressAddress.HasValue)
                    continue;

                var value = Reader.Default.Read<double>(this._tidalProgressListener.ProgressAddress.Value, out var success);

                //long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                //long diff = currentTime - this._startTime;

                //if (diff > 0)
                //{
                //    this._currentSong.Time = diff;
                //}
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

                if (this._session == null)
                {
                    JsonTidalSession session =
                        await this._tidalClient.Login(Core.INSTANCE.SettingManager.Settings.TidalAccess.AccessToken);

                    if (!DataValidator.ValidateData(session))
                        continue;

                    this._session = session;

                    if (!Core.IsLoaded())
                        continue;

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

                JsonTidalTrack tidalTrack = await FindTidalTrack();

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

            JsonTidalTrack tidalTrack = await FindTidalTrack();

            if (!DataValidator.ValidateData(tidalTrack))
                return null;

            this._currentSong = TidalDataMerger.ValidateConvertAndMerge(tidalTrack);

            this._startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            this._tidalProgressListener.Start();

            return this._currentSong;
        }

        private async Task<JsonTidalTrack> FindTidalTrack()
        {
            if (!TidalUtils.IsTidalRunning())
                return null;

            Process tidalProcess = TidalUtils.FindTidalProcess();

            if (!DataValidator.ValidateData(tidalProcess))
                return null;

            if (!DataValidator.ValidateData(this._session))
                return null;

            if (!tidalProcess.MainWindowTitle.Contains("-"))
                return null;

            string title = tidalProcess.MainWindowTitle.Split('-')[0].Trim();
            string artists = tidalProcess.MainWindowTitle.Split('-')[1].Trim().Replace(",", " /");

            JsonTidalSearchResult searchResult =
                await this._tidalClient.Search(this._session, title + " " + artists);

            if (!DataValidator.ValidateData(searchResult))
                return null;

            List<JsonTidalTrack> tracks = searchResult.Items;

            for (int i = 0; i < tracks.Count; i++)
            {
                JsonTidalTrack currentTrack = tracks[i];

                if (currentTrack.Title.ToLower().Equals(title.ToLower()) &&
                    ArtistsMatch(currentTrack.Artists, artists))
                {
                    return currentTrack;
                }
            }

            return new JsonTidalTrack();
        }

        private bool ArtistsMatch(List<JsonTidalArtist> artistsAsList, string artistsAsString)
        {
            int matches = 0;
            
            GenericList<string> list = new AString(artistsAsString.Replace(" /", System.Environment.NewLine)).AsList();

            for (int i = 0; i < artistsAsList.Count; i++)
            {
                JsonTidalArtist artist = artistsAsList[i];

                for (int j = 0; j < list.Length; j++)
                {
                    string name = list.Get(j).Trim();

                    if (artist.Name.ToLower().Equals(name.ToLower()))
                        matches++;
                }
            }

            return matches == artistsAsList.Count;
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
