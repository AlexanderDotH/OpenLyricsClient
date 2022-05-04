using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Events.EventArgs;
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

        private Thread _updateTokenThread;
        private Thread _timeSyncThread;

        private Task _updateSongDataTask;
        private Task _updateSongPlaybackTask;

        private bool _disposed;

        public SpotifySongProvider(NewSongHandler songHandler) 
        {
            this._debugger = new Debugger<SpotifySongProvider>(this);

            songHandler.SongChanged += OnSongChanged;

            this._playerApi = new PlayerApi(new HttpClient(), Core.INSTANCE.Settings.BearerAccess.AccessToken);
            this._accessToken = Core.INSTANCE.Settings.BearerAccess.AccessToken;

            this._updateTokenThread = new Thread(UpdateToken);
            this._updateTokenThread.Start();

            this._updateSongDataTask = new Task(() => UpdateSongData());
            this._updateSongDataTask.Start();

            this._updateSongPlaybackTask = new Task(() => UpdatePlayback());
            this._updateSongPlaybackTask.Start();

            this._timeSyncThread = new Thread(TimeSync);
            this._timeSyncThread.Start();

            this._disposed = false;
        }

        //Song info time sync -> always
        private void TimeSync()
        {
            while (!this._disposed)
            {
                if (DataValidator.ValidateData(this._currentSong) &&
                    DataValidator.ValidateData(this._currentSong.TimeStamp, this._currentSong.Paused,
                        this._currentSong.ProgressMs))
                {
                    if (!this._currentSong.Paused)
                    {
                        long current_time = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                        long diff = 0;

                        if (this._currentSong.TimeStamp > 0)
                        {
                            diff = (current_time - this._currentSong.TimeStamp);
                        }

                        this._currentSong.Time = this._currentSong.ProgressMs + diff;
                        this._currentSong.TimeStamp = 0;
                    }
                }
            }
        }

        //Song info sync -> data update
        private async Task UpdatePlayback()
        {
            while (!this._disposed)
            {
                Thread.Sleep(50);

                if (DataValidator.ValidateData(this._playerApi))
                {
                    CurrentPlaybackContext currentPlayback =
                        await this._playerApi.GetCurrentlyPlayingTrack<CurrentPlaybackContext>();

                    if (DataValidator.ValidateData(currentPlayback))
                    {
                        this._currentSong =
                            SpotifyDataMerger.ValidateUpdatePlayBack(this._currentSong, currentPlayback);

                    }
                }
            }
        }

        //Song info update -> data update
        private async Task UpdateSongData()
        {
            while (!this._disposed)
            {
                Thread.Sleep(100);

                if (DataValidator.ValidateData(this._playerApi))
                {
                    CurrentTrackPlaybackContext currentTrack = 
                        await this._playerApi.GetCurrentlyPlayingTrack<CurrentTrackPlaybackContext>();

                    if (DataValidator.ValidateData(currentTrack))
                    {
                        this._currentSong =
                            SpotifyDataMerger.ValidateUpdateAndMerge(this._currentSong, currentTrack);
                    }
                }
            }
        }

        //Song changed -> get new song
        private async Task UpdateCurrentPlaybackTrack()
        {
            if (DataValidator.ValidateData(this._playerApi))
            {
                CurrentTrackPlaybackContext currentTrack =
                    await this._playerApi.GetCurrentlyPlayingTrack<CurrentTrackPlaybackContext>();

                if (DataValidator.ValidateData(currentTrack))
                {
                    this._currentSong = SpotifyDataMerger.ValidateConvertAndMerge(currentTrack);
                }
            }
        }

        //Kinda useless idk
        private void UpdateToken()
        {
            while (!this._disposed)
            {
                if (this._accessToken != Core.INSTANCE.Settings.BearerAccess.AccessToken)
                {
                    this._playerApi = new PlayerApi(new HttpClient(), Core.INSTANCE.Settings.BearerAccess.AccessToken);
                    this._accessToken = Core.INSTANCE.Settings.BearerAccess.AccessToken;
                }
            }
        }

        public void OnSongChanged(Object sender, SongChangedEventArgs songChangedEventArgs)
        {
            Task task = new Task(() => UpdateCurrentPlaybackTrack());
            task.Start();
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
