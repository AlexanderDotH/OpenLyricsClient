using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LyricsWPF.Backend.Collector;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Events.EventArgs;
using LyricsWPF.Backend.Handler.Song;
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

        private bool _disposed;

        private const int LYRIC_OFFSET = 0;

        public LyricHandler(SongHandler songHandler)
        {
            this._debugger = new Debugger<LyricHandler>(this);

            this._songHandler = songHandler;
            songHandler.SongChanged += OnSongChanged;

            this._lyricCollector = new LyricCollector();

            this._manageLyricsTask = new Task(async () => await ManageLyrics(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            this._manageLyricsTask.Start();

            this._disposed = false;
        }

        private async Task ManageLyrics()
        {
            while (!this._disposed)
            {
                if (DataValidator.ValidateData(this._songHandler))
                {
                    Song.Song currentSong = this._songHandler.CurrentSong;

                    if (DataValidator.ValidateData(currentSong) &&
                        DataValidator.ValidateData(currentSong.Time) &&
                        DataValidator.ValidateData(currentSong.Lyrics) &&
                        DataValidator.ValidateData(currentSong.Lyrics.LyricParts) &&
                        currentSong.HasLyrics)
                    {
                        for (int i = 0; i < currentSong.Lyrics.LyricParts.Length; i++)
                        {
                            LyricPart currentPart = currentSong.Lyrics.LyricParts[i];

                            if (i + 1 < currentSong.Lyrics.LyricParts.Length)
                            {
                                LyricPart nextPart = currentSong.Lyrics.LyricParts[i + 1];

                                // I thing this is the issue
                                // What did I do?: nothing, cause I don´t now how to fix it
                                if (DataValidator.ValidateData(currentPart) &&
                                    DataValidator.ValidateData(currentPart.Part, currentPart.Time) &&
                                    DataValidator.ValidateData(nextPart) &&
                                    DataValidator.ValidateData(nextPart.Part, nextPart.Time))
                                {
                                    if (MathUtils.IsInRange(currentPart.Time, nextPart.Time, currentSong.Time + LYRIC_OFFSET))
                                    {
                                        currentSong.CurrentLyricPart = currentPart;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                currentSong.CurrentLyricPart =
                                    currentSong.Lyrics.LyricParts[currentSong.Lyrics.LyricParts.Length - 1];
                                break;
                            }
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
                    DataValidator.ValidateData(this._songHandler.CurrentSong))
                {
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();

                    SongRequestObject songRequestObject = new SongRequestObject(
                        SongFormatter.FormatSongName(songChangedEventArgs.Song.Title),
                        songChangedEventArgs.Song.Artists,
                        songChangedEventArgs.Song.MaxTime,
                        songChangedEventArgs.Song.Album);

                    LyricData lyricData = await this._lyricCollector.CollectLyrics(songRequestObject, SelectionMode.QUALITY);

                    stopwatch.Stop();

                    this._debugger.Write("Took " + stopwatch.ElapsedMilliseconds + "ms to fetch the lyrics!", DebugType.INFO);

                    if (DataValidator.ValidateData(lyricData) &&
                        DataValidator.ValidateData(lyricData.LyricParts, lyricData.LyricReturnCode))
                    {
                        if (lyricData.LyricReturnCode == LyricReturnCode.Success)
                        {
                            this._songHandler.CurrentSong.Lyrics = lyricData;
                        }
                    }
                }

            }, Core.INSTANCE.CancellationTokenSource.Token);
        }

        public void Dispose()
        {
            this._disposed = true;
        }
    }
}
