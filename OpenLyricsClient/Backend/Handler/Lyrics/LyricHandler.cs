using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Collector.Lyrics;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Events.EventHandler;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Helper;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Handler.Lyrics
{

    // TODOO:
    // Lyrics collector

    public class LyricHandler : IHandler
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

        public event LyricChangedEventHandler LyricChanged;

        public LyricHandler(SongHandler songHandler)
        {
            this._debugger = new Debugger<LyricHandler>(this);

            this._lyricCollector = new LyricCollector();

            this._songHandler = songHandler;

            this._cancellationTokenSource = new CancellationTokenSource();

            Core.INSTANCE.TaskRegister.Register(
                out _manageLyricSuspensionToken, 
                new Task(async () => await ManageLyrics(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
                EnumRegisterTypes.MANAGE_LYRICS);

            Core.INSTANCE.TaskRegister.Register(
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

                    if (!DataValidator.ValidateData(songRequestObject))
                        continue;
                    
                    LyricData lyricData = Core.INSTANCE.CacheManager.GetLyricsByRequest(songRequestObject);

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
                                            Dispatcher.UIThread.InvokeAsync(() =>
                                            {
                                                LyricChangedEvent(new LyricChangedEventArgs(currentPart));
                                            });
                                            continue;
                                        }
                                    }
                                }
                                else 
                                {
                                    if (MathUtils.IsInRange(currentPart.Time, currentSong.SongMetadata.MaxTime, currentSong.Time + LYRIC_OFFSET))
                                    {
                                        currentSong.CurrentLyricPart = currentPart;
                                        Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            LyricChangedEvent(new LyricChangedEventArgs(currentPart));
                                        });
                                        continue;
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

        public async Task FireLyricsSearch(SongResponseObject songResponseObject, SongChangedEventArgs songChangedEventArgs)
        {
            /*if (songChangedEventArgs.EventType == EventType.PRE &&
                DataValidator.ValidateData(this._songHandler.CurrentSong))
            {
                SongRequestObject songRequestObject = SongRequestObject.FromSong(this._songHandler.CurrentSong);

                LyricData lyricData = Core.INSTANCE.CacheManager.GetLyricsByRequest(songRequestObject);

                if (DataValidator.ValidateData(lyricData))
                {
                    if (lyricData.LyricReturnCode == LyricReturnCode.FAILED)
                        Core.INSTANCE.CacheManager.RemoveDataByRequest(songRequestObject);
                }
            }*/

            if (songChangedEventArgs.EventType == EventType.POST)
            {
                if (Core.INSTANCE.CacheManager.IsLyricsInCache(songResponseObject.SongRequestObject))
                    return;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                songChangedEventArgs.Song.State = SongState.SEARCHING_LYRICS;
                await this._lyricCollector.CollectLyrics(songResponseObject);
                songChangedEventArgs.Song.State = SongState.SEARCHING_FINISHED;

                this._debugger.Write("Took " + stopwatch.ElapsedMilliseconds + "ms to fetch the lyrics!", DebugType.INFO);
            }
        }
        
        protected virtual void LyricChangedEvent(LyricChangedEventArgs lyricChangedEventArgs)
        {
            LyricChangedEventHandler lyricChangedEventHandler = LyricChanged;
            lyricChangedEventHandler?.Invoke(this, lyricChangedEventArgs);
        }

        public void Dispose()
        {
            this._disposed = true;

            Core.INSTANCE.TaskRegister.Kill(
                EnumRegisterTypes.MANAGE_LYRICS,
                EnumRegisterTypes.APPLY_LYRICS_TO_SONG);
        }
    }
}
