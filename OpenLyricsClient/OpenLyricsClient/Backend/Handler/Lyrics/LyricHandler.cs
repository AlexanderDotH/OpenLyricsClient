using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Collector.Lyrics;
using OpenLyricsClient.Backend.Debug;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Romanisation;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
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

        private Romanization _romanization;

        private bool _disposed;

        private const int LYRIC_OFFSET = 0;

        public LyricHandler(SongHandler songHandler)
        {
            this._debugger = new Debugger<LyricHandler>(this);

            this._lyricCollector = new LyricCollector();
            this._romanization = new Romanization();

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

                Song.Song song = _songHandler.CurrentSong;

                if (DataValidator.ValidateData(song) &&
                    DataValidator.ValidateData(song.Title) &&
                    DataValidator.ValidateData(song.Artists) &&
                    DataValidator.ValidateData(song.MaxTime) &&
                    DataValidator.ValidateData(song.Album) &&
                    DataValidator.ValidateData(Core.INSTANCE.SettingManager.Settings.LyricSelectionMode) &&
                    DataValidator.ValidateData(this._lyricCollector) &&
                    DataValidator.ValidateData(Core.INSTANCE.CacheManager))
                {

                    SongRequestObject songRequestObject = new SongRequestObject(
                        song.Title,
                        SongFormatter.FormatSongName(song.Title),
                        song.Artists,
                        song.MaxTime,
                        song.Album,
                        SongFormatter.FormatSongAlbum(song.Album),
                        Core.INSTANCE.SettingManager.Settings.LyricSelectionMode);

                    LyricData lyricData = Core.INSTANCE.CacheManager.GetDataByRequest(songRequestObject);

                    if (DataValidator.ValidateData(lyricData) && 
                        lyricData.LyricReturnCode == LyricReturnCode.Success)
                    {
                        song.Lyrics = lyricData;
                        song.State = SongState.HAS_LYRICS_AVAILABLE;
                    }
                    else if (song.State == SongState.SEARCHING_FINISHED)
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
                    Song.Song currentSong = this._songHandler.CurrentSong;

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

                Song.Song song = this._songHandler.CurrentSong;

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
                            thirdLyricPart.Part = await this._romanization.Romanize(thirdLyricPart.Part);

                            if (thirdLyricPart == song.CurrentLyricPart)
                            {
                                LyricPart firstLyricPart = null;
                                LyricPart secondLyricPart = null;
                                LyricPart fourthLyricPart = null;
                                LyricPart fifthLine = null;

                                if (MathUtils.IsInRange(0, lyrics.LyricParts.Length - 1, i - 2))
                                {
                                    firstLyricPart = lyrics.LyricParts[i - 2];
                                    firstLyricPart.Part = await this._romanization.Romanize(firstLyricPart.Part);
                                }

                                if (MathUtils.IsInRange(0, lyrics.LyricParts.Length - 1, i - 1))
                                {
                                    secondLyricPart = lyrics.LyricParts[i - 1];
                                    secondLyricPart.Part = await this._romanization.Romanize(secondLyricPart.Part);
                                }

                                if (MathUtils.IsInRange(0, lyrics.LyricParts.Length - 1, i + 1))
                                {
                                    fourthLyricPart = lyrics.LyricParts[i + 1];
                                    fourthLyricPart.Part = await this._romanization.Romanize(fourthLyricPart.Part);
                                }

                                if (MathUtils.IsInRange(0, lyrics.LyricParts.Length - 1, i + 2))
                                {
                                    fifthLine = lyrics.LyricParts[i + 2];
                                    fifthLine.Part = await this._romanization.Romanize(fifthLine.Part);
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

                    songChangedEventArgs.Song.State = SongState.SEARCHING_LYRICS;
                    await this._lyricCollector.CollectLyrics(songRequestObject);
                    songChangedEventArgs.Song.State = SongState.SEARCHING_FINISHED;

                    this._debugger.Write("Took " + stopwatch.ElapsedMilliseconds + "ms to fetch the lyrics!", DebugType.INFO);
                }
            });
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
