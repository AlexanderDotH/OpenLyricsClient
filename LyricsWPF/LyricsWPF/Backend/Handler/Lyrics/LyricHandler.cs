using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LyricsWPF.Backend.Collector;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Events.EventArgs;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Romanisation;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Handler.Lyrics
{

    // TODOO:
    // Lyrics collector

    class LyricHandler : IHandler
    {
        private Debugger<LyricHandler> _debugger;

        private LyricCollector _lyricCollector;

        private SongHandler _songHandler;
        private Task _manageLyricsTask;
        private Task _manageLyricsRollTask;
        private Task _applyLyricTask;
        private CancellationTokenSource _cancellationTokenSource;

        private Romanization _romanization;

        private bool _disposed;

        private const int LYRIC_OFFSET = 0;

        public LyricHandler(SongHandler songHandler)
        {
            this._debugger = new Debugger<LyricHandler>(this);

            this._songHandler = songHandler;
            songHandler.SongChanged += OnSongChanged;

            this._cancellationTokenSource = new CancellationTokenSource();

            this._lyricCollector = new LyricCollector();
            this._romanization = new Romanization();

            this._manageLyricsTask = new Task(async () => await ManageLyrics(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            this._manageLyricsTask.Start();

            this._manageLyricsRollTask = new Task(async () => await ManageLyricsRoll(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            this._manageLyricsRollTask.Start();

            this._applyLyricTask = new Task(async () => await ApplyLyricsToSong(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            this._applyLyricTask.Start();

            this._disposed = false;
        }

        private async Task ApplyLyricsToSong()
        {
            while (!this._disposed)
            {
                await Task.Delay(1);

                Song.Song song = _songHandler.CurrentSong;

                if (DataValidator.ValidateData(song) &&
                    DataValidator.ValidateData(song.Title) &&
                    DataValidator.ValidateData(song.Artists) &&
                    DataValidator.ValidateData(song.MaxTime) &&
                    DataValidator.ValidateData(song.Album) &&
                    DataValidator.ValidateData(Core.INSTANCE.SettingManager.Settings.LyricSelectionMode) &&
                    DataValidator.ValidateData(this._lyricCollector) &&
                    DataValidator.ValidateData(this._lyricCollector.CacheManager))
                {

                    SongRequestObject songRequestObject = new SongRequestObject(
                        song.Title,
                        SongFormatter.FormatSongName(song.Title),
                        song.Artists,
                        song.MaxTime,
                        song.Album,
                        SongFormatter.FormatSongAlbum(song.Album),
                        Core.INSTANCE.SettingManager.Settings.LyricSelectionMode);

                    LyricData lyricData = this._lyricCollector.CacheManager.GetDataByRequest(songRequestObject);
                    song.Lyrics = lyricData;

                }
            }
        }

        private async Task ManageLyrics()
        {
            while (!this._disposed)
            {
                await Task.Delay(50);

                if (DataValidator.ValidateData(this._songHandler))
                {
                    Song.Song currentSong = this._songHandler.CurrentSong;

                    if (DataValidator.ValidateData(currentSong) &&
                        DataValidator.ValidateData(currentSong.Time) &&
                        DataValidator.ValidateData(currentSong.Lyrics) &&
                        DataValidator.ValidateData(currentSong.Lyrics.LyricParts) &&
                        currentSong.State == SongState.HAS_LYRICS_AVAILABLE)
                    {
                        for (int i = 0; i < currentSong.Lyrics.LyricParts.Length; i++)
                        {
                            LyricPart currentPart = currentSong.Lyrics.LyricParts[i];

                            if (i == currentSong.Lyrics.LyricParts.Length)
                            {
                                currentSong.CurrentLyricPart =
                                    currentSong.Lyrics.LyricParts[currentSong.Lyrics.LyricParts.Length - 1];
                                continue;
                            }
                            else
                            {
                                if (i + 1 < currentSong.Lyrics.LyricParts.Length)
                                {
                                    LyricPart nextPart = currentSong.Lyrics.LyricParts[i + 1];

                                    // I thing this is the issue
                                    // What did I do?: nothing, cause I don´t now how to fix it
                                    if (DataValidator.ValidateData(currentPart) &&
                                        DataValidator.ValidateData(currentPart.Part) &&
                                        DataValidator.ValidateData(currentPart.Time) &&
                                        DataValidator.ValidateData(nextPart) &&
                                        DataValidator.ValidateData(nextPart.Part) &&
                                        DataValidator.ValidateData(nextPart.Time))
                                    {
                                        if (MathUtils.IsInRange(currentPart.Time, nextPart.Time, currentSong.Time + LYRIC_OFFSET))
                                        {
                                            currentSong.CurrentLyricPart = currentPart;
                                            continue;
                                        }
                                    }

                                }

                            }
                        }
                    }
                }
            }
        }

        public async Task ManageLyricsRoll()
        {
            while (!this._disposed)
            {
                await Task.Delay(35);

                Song.Song song = this._songHandler.CurrentSong;

                if (DataValidator.ValidateData(song) &&
                    DataValidator.ValidateData(song.CurrentLyricPart) &&
                    DataValidator.ValidateData(song.Lyrics) &&
                    DataValidator.ValidateData(song.Lyrics.LyricParts) &&

                    song.State == SongState.HAS_LYRICS_AVAILABLE)
                {
                    for (int i = 0; i < song.Lyrics.LyricParts.Length; i++)
                    {
                        LyricPart secondLyricPart = song.Lyrics.LyricParts[i];
                        secondLyricPart.Part = await this._romanization.Romanize(secondLyricPart.Part);

                        if (secondLyricPart == song.CurrentLyricPart)
                        {
                            LyricPart firstLyricPart = null;
                            LyricPart thirdLyricPart = null;

                            if (MathUtils.IsInRange(0, song.Lyrics.LyricParts.Length - 1, i - 1))
                            {
                                firstLyricPart = song.Lyrics.LyricParts[i - 1];
                                firstLyricPart.Part = await this._romanization.Romanize(firstLyricPart.Part);
                            }

                            if (MathUtils.IsInRange(0, song.Lyrics.LyricParts.Length - 1, i + 1))
                            {
                                thirdLyricPart = song.Lyrics.LyricParts[i + 1];
                                thirdLyricPart.Part = await this._romanization.Romanize(thirdLyricPart.Part);
                            }

                            song.CurrentLyricsRoll =
                                new LyricsRoll(firstLyricPart, secondLyricPart, thirdLyricPart);
                        }
                    }
                }

            }
        }

        public void OnSongChanged(Object sender, SongChangedEventArgs songChangedEventArgs)
        {
            Task.Factory.StartNew(async () =>
            {
                if (DataValidator.ValidateData(songChangedEventArgs.Song) &&
                    DataValidator.ValidateData(songChangedEventArgs.Song.Title,
                        songChangedEventArgs.Song.Artists, songChangedEventArgs.Song.MaxTime, songChangedEventArgs.Song.Album) &&
                    DataValidator.ValidateData(this._songHandler) && 
                    DataValidator.ValidateData(this._songHandler.CurrentSong) &&
                    this._songHandler.CurrentSong.Lyrics == null)
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    SongRequestObject songRequestObject = new SongRequestObject(
                        songChangedEventArgs.Song.Title,
                        SongFormatter.FormatSongName(songChangedEventArgs.Song.Title),
                        songChangedEventArgs.Song.Artists,
                        songChangedEventArgs.Song.MaxTime,
                        songChangedEventArgs.Song.Album,
                        SongFormatter.FormatSongAlbum(songChangedEventArgs.Song.Album),
                        Core.INSTANCE.SettingManager.Settings.LyricSelectionMode);

                    await this._lyricCollector.CollectLyrics(songRequestObject);
                    
                    this._debugger.Write("Took " + stopwatch.ElapsedMilliseconds + "ms to fetch the lyrics!", DebugType.INFO);
                }
            });

        }

        public void Dispose()
        {
            this._disposed = true;
        }
    }
}
