using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Collector.Lyrics;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Handler.Lyrics
{

    // TODOO:
    // Lyrics collector

    class LyricHandler : IHandler
    {
        private Debugger<LyricHandler> _debugger;

        private LyricCollector _lyricCollector;

        private SongHandler _songHandler;

        private TaskSuspensionToken _manageLyricSuspensionToken;
        private TaskSuspensionToken _manageLyricsRollSuspensionToken;
        private TaskSuspensionToken _applyLyricSuspensionToken;

        private CancellationTokenSource _cancellationTokenSource;

        private bool _disposed;

        private const int LYRIC_OFFSET = 0;

        public LyricHandler(SongHandler songHandler)
        {
            this._debugger = new Debugger<LyricHandler>(this);

            this._lyricCollector = new LyricCollector();

            this._songHandler = songHandler;
            songHandler.SongChanged += OnSongChanged;

            this._cancellationTokenSource = new CancellationTokenSource();

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _manageLyricSuspensionToken, 
                new Task(async () => await ManageLyrics(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
                EnumRegisterTypes.MANAGE_LYRICS);

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _manageLyricsRollSuspensionToken,
                new Task(async () => await ManageLyricsRoll(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.MANAGE_LYRICS_ROLL);

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _applyLyricSuspensionToken,
                new Task(async () => await ApplyLyricsToSong(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.APPLY_LYRICS_TO_SONG);

            this._disposed = false;
        }

        private async Task ApplyLyricsToSong()
        {
            while (!this._disposed)
            {
                await this._applyLyricSuspensionToken.WaitForRelease();
                await Task.Delay(100);

                Structure.Song.Song song = _songHandler.CurrentSong;

                if (DataValidator.ValidateData(song) &&
                    DataValidator.ValidateData(song.SongMetadata.Name) &&
                    DataValidator.ValidateData(song.SongMetadata.Artists) &&
                    DataValidator.ValidateData(song.SongMetadata.MaxTime) &&
                    DataValidator.ValidateData(song.SongMetadata.Album) &&
                    DataValidator.ValidateData(song.SongMetadata) &&
                    DataValidator.ValidateData(Core.INSTANCE.SettingManager.Settings.LyricSelectionMode) &&
                    DataValidator.ValidateData(this._lyricCollector) &&
                    DataValidator.ValidateData(Core.INSTANCE.CacheManager))
                {
                    SongRequestObject songRequestObject = SongRequestObject.FromSong(song);

                    LyricData lyricData = Core.INSTANCE.CacheManager.GetDataByRequest(songRequestObject);

                    if (!DataValidator.ValidateData(lyricData))
                        continue;

                    if (DataValidator.ValidateData(lyricData.SongMetadata))
                    {
                        if (lyricData.SongMetadata.Name != song.SongMetadata.Name &&
                            lyricData.SongMetadata.Album != song.SongMetadata.Album)
                        {
                            song.Lyrics = null;
                        }
                    }

                    if (lyricData.LyricReturnCode == LyricReturnCode.SUCCESS)
                    {
                        song.Lyrics = lyricData;
                        song.State = SongState.HAS_LYRICS_AVAILABLE;
                    }
                    else if (lyricData.LyricReturnCode == LyricReturnCode.FAILED)
                    {
                        song.Lyrics = null;
                        song.State = SongState.NO_LYRICS_AVAILABLE;
                    }
                }
            }
        }

        private async Task ManageLyrics()
        {
            while (!this._disposed)
            {
                await this._manageLyricSuspensionToken.WaitForRelease();
                await Task.Delay(35);

                if (DataValidator.ValidateData(this._songHandler))
                {
                    Structure.Song.Song currentSong = this._songHandler.CurrentSong;

                    if (DataValidator.ValidateData(currentSong) &&
                        DataValidator.ValidateData(currentSong.Time) &&
                        DataValidator.ValidateData(currentSong.Lyrics) &&
                        DataValidator.ValidateData(currentSong.Lyrics.LyricParts) &&
                        currentSong.State == SongState.HAS_LYRICS_AVAILABLE)
                    {
                        try
                        {
                            for (int i = 0; i < currentSong.Lyrics.LyricParts.Length; i++)
                            {
                                LyricPart currentPart = currentSong.Lyrics.LyricParts[i];

                                if (i == currentSong.Lyrics.LyricParts.Length)
                                {
                                    currentSong.CurrentLyricPart =
                                        currentSong.Lyrics.LyricParts[currentSong.Lyrics.LyricParts.Length];
                                    continue;
                                }
                                else
                                {
                                    if (i + 1 < currentSong.Lyrics.LyricParts.Length)
                                    {
                                        LyricPart nextPart = currentSong.Lyrics.LyricParts[i + 1];

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
                        catch (Exception e)
                        {
                            this._debugger.Write(e);
                        }
                    }
                }
            }
        }

        public async Task ManageLyricsRoll()
        {
            while (!this._disposed)
            {
                await this._manageLyricsRollSuspensionToken.WaitForRelease();
                await Task.Delay(35);

                Structure.Song.Song song = this._songHandler.CurrentSong;

                if (DataValidator.ValidateData(song) &&
                    DataValidator.ValidateData(song.CurrentLyricPart) &&
                    DataValidator.ValidateData(song.Lyrics) &&
                    DataValidator.ValidateData(song.Lyrics.LyricParts) &&

                    song.State == SongState.HAS_LYRICS_AVAILABLE)
                {
                    try
                    {
                        LyricData lyrics = song.Lyrics;

                        for (int i = 0; i < lyrics.LyricParts.Length; i++)
                        {
                            LyricPart thirdLyricPart = lyrics.LyricParts[i];

                            if (thirdLyricPart == song.CurrentLyricPart)
                            {
                                LyricPart firstLyricPart = null;
                                LyricPart secondLyricPart = null;
                                LyricPart fourthLyricPart = null;
                                LyricPart fifthLine = null;

                                if (MathUtils.IsInRange(0, lyrics.LyricParts.Length - 1, i - 2))
                                {
                                    firstLyricPart = lyrics.LyricParts[i - 2];
                                }

                                if (MathUtils.IsInRange(0, lyrics.LyricParts.Length - 1, i - 1))
                                {
                                    secondLyricPart = lyrics.LyricParts[i - 1];
                                }

                                if (MathUtils.IsInRange(0, lyrics.LyricParts.Length - 1, i + 1))
                                {
                                    fourthLyricPart = lyrics.LyricParts[i + 1];
                                }

                                if (MathUtils.IsInRange(0, lyrics.LyricParts.Length - 1, i + 2))
                                {
                                    fifthLine = lyrics.LyricParts[i + 2];
                                }

                                song.CurrentLyricsRoll =
                                    new LyricsRoll(firstLyricPart, secondLyricPart, thirdLyricPart, fourthLyricPart, fifthLine);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this._debugger.Write(e);
                    }
                }
            }
        }

        public void OnSongChanged(Object sender, SongChangedEventArgs songChangedEventArgs)
        {
            if (songChangedEventArgs.EventType == EventType.PRE &&
                DataValidator.ValidateData(this._songHandler.CurrentSong))
            {
                SongRequestObject songRequestObject = SongRequestObject.FromSong(this._songHandler.CurrentSong);

                LyricData lyricData = Core.INSTANCE.CacheManager.GetDataByRequest(songRequestObject);

                if (DataValidator.ValidateData(lyricData))
                {
                    if (lyricData.LyricReturnCode == LyricReturnCode.FAILED)
                        Core.INSTANCE.CacheManager.RemoveDataByRequest(songRequestObject);
                }
            }

            if (songChangedEventArgs.EventType == EventType.POST)
            {
                Task.Factory.StartNew(async () =>
                {
                    if (DataValidator.ValidateData(songChangedEventArgs) &&
                        DataValidator.ValidateData(songChangedEventArgs.Song))
                    {
                        SongRequestObject songRequestObject = SongRequestObject.FromSong(songChangedEventArgs.Song);

                        if (Core.INSTANCE.CacheManager.IsInCache(songRequestObject))
                            return;

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        songChangedEventArgs.Song.State = SongState.SEARCHING_LYRICS;
                        await this._lyricCollector.CollectLyrics(songRequestObject);
                        songChangedEventArgs.Song.State = SongState.SEARCHING_FINISHED;

                        this._debugger.Write("Took " + stopwatch.ElapsedMilliseconds + "ms to fetch the lyrics!", DebugType.INFO);
                    }
                });
            }
        }

        public void Dispose()
        {
            this._disposed = true;

            Core.INSTANCE.TaskRegister.Kill(
                EnumRegisterTypes.MANAGE_LYRICS, 
                EnumRegisterTypes.MANAGE_LYRICS_ROLL,
                EnumRegisterTypes.APPLY_LYRICS_TO_SONG);
        }
    }
}
