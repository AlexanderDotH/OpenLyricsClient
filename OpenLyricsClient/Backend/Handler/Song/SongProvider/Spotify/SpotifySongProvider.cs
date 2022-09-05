using System;
using System.Net.Http;
using System.Threading.Tasks;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Services.Services;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;
using SpotifyApi.NetCore;

namespace OpenLyricsClient.Backend.Handler.Song.SongProvider.Spotify
{
    class SpotifySongProvider : ISongProvider
    {
        private Debugger<SpotifySongProvider> _debugger;
        private Structure.Song.Song _currentSong;

        private PlayerApi _playerApi;
        private string _accessToken;

        private TaskSuspensionToken _updatePlaybackSuspensionToken;
        private TaskSuspensionToken _updateSongDataSuspensionToken;
        private TaskSuspensionToken _timeSyncSuspensionToken;

        private IService _service;

        private bool _disposed;

        public SpotifySongProvider() 
        {
            this._debugger = new Debugger<SpotifySongProvider>(this);
            this._disposed = false;

            this._playerApi = new PlayerApi(new HttpClient(), Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess.AccessToken);
            this._accessToken = Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess.AccessToken;

            this._service = Core.INSTANCE.ServiceHandler.GetServiceByName("Spotify");

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _updatePlaybackSuspensionToken,
                new Task(async () => await UpdatePlaybackTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATEPLAYBACK);

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _updateSongDataSuspensionToken,
                new Task(async () => await UpdateSongDataTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATESONGDATA);

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _timeSyncSuspensionToken,
                new Task(async () => await TimeSyncTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SPOTIFYSONGPROVIDER_TIMESYNC);
        }

        //Song info time sync -> always
        private async Task TimeSyncTask()
        {
            while (!this._disposed)
            {
                await Task.Delay(1);
                await this._timeSyncSuspensionToken.WaitForRelease();

                if (!this._service.IsConnected())
                    continue;

                if (DataValidator.ValidateData(this._currentSong) &&
                    DataValidator.ValidateData(this._currentSong.TimeStamp) &&
                    DataValidator.ValidateData(this._currentSong.Paused) &&
                    DataValidator.ValidateData(this._currentSong.ProgressMs))
                {
                    try
                    {
                        Structure.Song.Song currentSong = this._currentSong;

                        lock (currentSong)
                        {
                            try
                            {
                                if (currentSong != null)
                                {
                                    if (currentSong.TimeStamp != 0)
                                    {
                                        long diff = DateTimeOffset.Now.ToUnixTimeMilliseconds() - currentSong.TimeStamp;

                                        if (diff < 0)
                                            diff = 0;

                                        if (!currentSong.Paused)
                                        {
                                            currentSong.Time = currentSong.ProgressMs + diff - currentSong.TimeThreshold;
                                        }
                                        else
                                        {
                                            currentSong.Time = currentSong.ProgressMs;
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            { }
                        }
                    }
                    catch (Exception e)
                    {
                        this._debugger.Write(e);
                    }
                }
            }
        }

        //Song info sync -> data update
        private async Task UpdatePlaybackTask()
        {
            while (!this._disposed)
            {
                await this._updatePlaybackSuspensionToken.WaitForRelease();

                if (!this._service.IsConnected())
                    continue;

                await Task.Delay(150);

                if (DataValidator.ValidateData(this._playerApi) &&
                    DataValidator.ValidateData(this._currentSong))
                {
                    try
                    {
                        CurrentPlaybackContext currentPlayback =
                            await this.GetPlayerApi().GetCurrentPlaybackInfo();

                        if (DataValidator.ValidateData(currentPlayback))
                        {
                            this._currentSong =
                                SpotifyDataMerger.ValidateUpdatePlayBack(this._currentSong, currentPlayback);
                        }
                    }
                    catch (Exception e)
                    {
                        this._debugger.Write(e);
                    }
                }
            }
        }

        //Song info update -> data update
        private async Task UpdateSongDataTask()
        {
            while (!this._disposed)
            {
                await this._updateSongDataSuspensionToken.WaitForRelease();

                if (!this._service.IsConnected())
                    continue;

                await Task.Delay(1000);

                if (DataValidator.ValidateData(this._playerApi) && 
                    DataValidator.ValidateData(this._currentSong))
                {
                    try
                    {
                        CurrentTrackPlaybackContext currentTrack =
                            await this.GetPlayerApi().GetCurrentlyPlayingTrack<CurrentTrackPlaybackContext>();

                        if (DataValidator.ValidateData(currentTrack))
                        {
                            this._currentSong =
                                SpotifyDataMerger.ValidateUpdateAndMerge(this._currentSong, currentTrack);
                        }
                    }
                    catch (Exception e)
                    {
                        this._debugger.Write(e);
                    }
                }
            }
        }

        //Song changed -> get new song
        public async Task<Structure.Song.Song> UpdateCurrentPlaybackTrack()
        {
            if (!this._service.IsConnected())
                return null;

            if (DataValidator.ValidateData(this._playerApi))
            {
                try
                {
                    CurrentTrackPlaybackContext currentTrack =
                        await this.GetPlayerApi().GetCurrentlyPlayingTrack<CurrentTrackPlaybackContext>();

                    if (DataValidator.ValidateData(currentTrack))
                    {
                        Structure.Song.Song song = SpotifyDataMerger.ValidateConvertAndMerge(currentTrack);
                        this._currentSong = song;
                        this._debugger.Write("Song has been changed", DebugType.INFO);
                        return song;
                    }
                }
                catch (Exception e)
                {
                    this._debugger.Write(e);
                }
            }

            return null;
        }

        private PlayerApi GetPlayerApi()
        {
            if (this._accessToken != Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess.AccessToken)
            {
                this._playerApi = new PlayerApi(new HttpClient(), Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess.AccessToken);
                this._accessToken = Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess.AccessToken;
            }

            return this._playerApi;
        }

        public void Dispose()
        {
            this._disposed = true;

            Core.INSTANCE.TaskRegister.Kill(
                EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATESONGDATA, 
                EnumRegisterTypes.SPOTIFYSONGPROVIDER_TIMESYNC, 
                EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATEPLAYBACK);
        }

        public Structure.Song.Song GetCurrentSong()
        {
            return this._currentSong;
        }

        public EnumSongProvider GetEnum()
        {
            return EnumSongProvider.SPOTIFY;
        }
    }
}
