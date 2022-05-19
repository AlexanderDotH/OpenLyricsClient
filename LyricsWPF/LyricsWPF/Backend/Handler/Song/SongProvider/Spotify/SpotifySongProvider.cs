using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Events.EventArgs;
using LyricsWPF.Backend.Handler.Services.Services;
using LyricsWPF.Backend.Utils;
using SpotifyApi.NetCore;
using SpotifyApi.NetCore.Authorization;

namespace LyricsWPF.Backend.Handler.Song.SongProvider.Spotify
{
    class SpotifySongProvider : ISongProvider
    {
        private Debugger<SpotifySongProvider> _debugger;
        private Song _currentSong;

        private PlayerApi _playerApi;
        private string _accessToken;

        private Task _timeSyncTask;
        private Task _updateSongDataTask;
        private Task _updateSongPlaybackTask;

        private IService _service;

        private bool _disposed;

        public SpotifySongProvider() 
        {
            this._debugger = new Debugger<SpotifySongProvider>(this);
            this._disposed = false;

            //songHandler.SongChanged += OnSongChanged;

            this._playerApi = new PlayerApi(new HttpClient(), Core.INSTANCE.Settings.BearerAccess.AccessToken);
            this._accessToken = Core.INSTANCE.Settings.BearerAccess.AccessToken;

            this._service = Core.INSTANCE.ServiceHandler.GetServiceByName("Spotify");

            this._updateSongPlaybackTask = new Task(async() => await UpdatePlayback(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            this._updateSongPlaybackTask.Start();

            this._updateSongDataTask = new Task(async() => await UpdateSongData(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            this._updateSongDataTask.Start();

            this._timeSyncTask = new Task(async () => await TimeSync(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            this._timeSyncTask.Start();
        }

        //Song info time sync -> always
        private async Task TimeSync()
        {
            while (!this._disposed)
            {
                if (!this._service.IsConnected())
                    break;

                if (DataValidator.ValidateData(this._currentSong) &&
                    DataValidator.ValidateData(this._currentSong.TimeStamp) &&
                    DataValidator.ValidateData(this._currentSong.Paused) &&
                    DataValidator.ValidateData(this._currentSong.ProgressMs))
                {
                    if (!this._currentSong.Paused)
                    {
                        try
                        {
                            if (this._currentSong != null)
                            {
                                BigInteger currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                BigInteger timeStamp = this._currentSong.TimeStamp;

                                BigInteger diff = 0;
                                BigInteger progress = this._currentSong.ProgressMs;

                                if (this._currentSong.TimeStamp > 0)
                                {
                                    diff = BigInteger.Subtract(currentTime, timeStamp);
                                }
                                this._currentSong.Time = (long)BigInteger.Add(progress, diff);
                                this._currentSong.TimeStamp = 0;
                            }
                        }
                        catch (Exception e)
                        {
                            this._debugger.Write(e);
                        }
                    }
                }
            }
        }

        //Song info sync -> data update
        private async Task UpdatePlayback()
        {
            while (!this._disposed)
            {
                if (!this._service.IsConnected())
                    break;

                await Task.Delay(50);

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
        private async Task UpdateSongData()
        {
            while (!this._disposed)
            {
                if (!this._service.IsConnected())
                    break;

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
        public async Task<Song> UpdateCurrentPlaybackTrack()
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
                        Song song = SpotifyDataMerger.ValidateConvertAndMerge(currentTrack);
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
            if (this._accessToken != Core.INSTANCE.Settings.BearerAccess.AccessToken)
            {
                this._playerApi = new PlayerApi(new HttpClient(), Core.INSTANCE.Settings.BearerAccess.AccessToken);
                this._accessToken = Core.INSTANCE.Settings.BearerAccess.AccessToken;
            }

            return this._playerApi;
        }

        public void Dispose()
        {
            this._disposed = true;
        }

        public Song GetCurrentSong()
        {
            return this._currentSong;
        }

        public EnumSongProvider GetEnum()
        {
            return EnumSongProvider.SPOTIFY;
        }
    }
}
