using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Handler.Lyrics;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;
using SpotifyApi.NetCore;

namespace LyricsWPF.Backend.Handler.Song
{
    class SongHandler : IHandler
    {
        private List<Song> _songs;
        private Song _currentSong;
        private SongStageChange _songStageChange;

        private LyricHandler _lyricHandler;

        private Debugger<SongHandler> _debugger;

        private Task _manageSongTask;
        private Task _manageLyricsTask;
        private Task _manageTimeSyncTask;
        private Task _debugTask;

        private bool _disposed;

        public SongHandler()
        {
            this._debugger = new Debugger<SongHandler>(this);

            this._songs = new List<Song>();
            this._songStageChange = new SongStageChange();

            this._lyricHandler = new LyricHandler();

            this._manageSongTask = new Task(() => ManageCurrentSong());
            this._manageLyricsTask = new Task(() => ManageLyrics());
            this._debugTask = new Task(() =>
            {
                while (!this._disposed)
                {
                    if (this._disposed)
                        break;

                    PrintSongState(this._currentSong);
                }
            });
            this._manageTimeSyncTask = new Task(() => ManageTimeSync());

            this._manageSongTask.Start();
            this._manageLyricsTask.Start();
            this._debugTask.Start();
            this._manageTimeSyncTask.Start();

            //this._currentSong = new Song("Never Gonna Give You Up", new string[] { "Rick Astley" });
        }

        private async Task ManageTimeSync()
        {
            while (!this._disposed)
            {
                if (DataValidator.ValidateData(this._currentSong))
                {
                    this._currentSong.SyncTime();
                }
            }
        }

        private async Task ManageLyrics()
        {
            while (!this._disposed)
            {
                if (DataValidator.ValidateData(this._currentSong))
                {
                    if (this._songStageChange.HasSongChanged(this._currentSong))
                    {
                        try
                        {
                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();
                            await this._lyricHandler.GetLyrics(new SongRequestObject(_currentSong.Title, _currentSong.Artists, _currentSong.MaxTime));
                            stopwatch.Stop();

                            this._debugger.Write("Took " + stopwatch.ElapsedMilliseconds + "ms to fetch the lyrics!", DebugType.INFO);

                            this._currentSong.Lyrics = this._lyricHandler.FullLyrics;

                            this._debugger.Write("Song changed!", DebugType.INFO);
                        }
                        catch (Exception e)
                        {
                            this._debugger.Write(e.Message, DebugType.ERROR);
                        }
                    }

                    this._currentSong.UpdateLyricsToTime();
                }
            }
        }

        public async Task ManageCurrentSong()
        {
            HttpClient httpClient = new HttpClient();
            while (!this._disposed)
            {
                Thread.Sleep(100);

                if (Core.INSTANCE.Settings.IsSpotifyConnected)
                {
                    try
                    {
                        var playerApi = new SpotifyApi.NetCore.PlayerApi(httpClient,
                            Core.INSTANCE.Settings.BearerAccess.AccessToken);
                        var currentTrack = await playerApi.GetCurrentlyPlayingTrack<CurrentTrackPlaybackContext>();

                        if (DataValidator.ValidateData(currentTrack))
                        {
                            if (!DataValidator.ValidateData(this._currentSong) || 
                                this._songStageChange.HasSongChanged(this._currentSong))
                            {
                                //Song changed

                                if (DataValidator.ValidateData(currentTrack, currentTrack.Item, currentTrack.Item.Artists))
                                {
                                    this._currentSong = new Song(currentTrack.Item.Name, DataConverter.SpotifyArtistsToStrings(currentTrack.Item.Artists))
                                    {
                                        ProgressMs = currentTrack.ProgressMs.Value,
                                        TimeStamp = currentTrack.Timestamp,
                                        MaxTime = currentTrack.Item.DurationMs
                                    };

                                    this._songs.Add(this._currentSong);
                                }
                            }
                            else
                            {
                                //Processing song data/edit
                                if (DataValidator.ValidateData(this._currentSong, currentTrack, currentTrack.Item))
                                {
                                    //this._currentSong.Title = currentTrack.Item.Name;
                                    this._currentSong.Artists = DataConverter.SpotifyArtistsToStrings(currentTrack.Item.Artists);
                                    this._currentSong.MaxTime = currentTrack.Item.DurationMs;
                                    this._currentSong.Paused = !currentTrack.IsPlaying;

                                    if (currentTrack.ProgressMs.HasValue && currentTrack.IsPlaying)
                                    {
                                        this._currentSong.TimeStamp = currentTrack.Timestamp;
                                        this._currentSong.ProgressMs = currentTrack.ProgressMs.Value;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this._debugger.Write(e.Message, DebugType.ERROR);    
                    }
                }
            }
        }

        private void PrintSongState(Song song)
        {

            if (DataValidator.ValidateData(song) && DataValidator.ValidateData(song.Title, song.Time, song.MaxTime))
            {
                this._debugger.Write("Title: " + song.Title, DebugType.INFO);
                this._debugger.Write("Time: " + song.Time, DebugType.INFO);
                this._debugger.Write("MaxTime: " + song.MaxTime, DebugType.INFO);

                if (DataValidator.ValidateData(song.CurrentLyricPart))
                {
                    this._debugger.Write("LyricPart: " + song.CurrentLyricPart.Part, DebugType.INFO);
                }
            }
        }

        public Song CurrentSong
        {
            get { return _currentSong; }
            set { _currentSong = value; }
        }

        public void Dispose()
        {
            this._disposed = true;

            for (int i = 0; i < this._songs.Count; i++)
            {
                Song song = _songs[i];
                song.Dispose();
            }

            try
            {
                this._manageSongTask.Wait(0);
                this._manageLyricsTask.Wait(0);

                this._manageSongTask.Dispose();
                this._manageLyricsTask.Dispose();
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }
        }
    }
}
