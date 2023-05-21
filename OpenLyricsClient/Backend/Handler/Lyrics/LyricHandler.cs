using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using DevBase.Api.Apis.OpenLyricsClient.Structure.Json;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Collector.Lyrics;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Events.EventHandler;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Helper;
using OpenLyricsClient.Shared.Structure;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Backend.Handler.Lyrics
{
    public class LyricHandler : IHandler
    {
        private Debugger<LyricHandler> _debugger;

        private LyricCollector _lyricCollector;

        private SongHandler _songHandler;
        
        private LyricData _oldLyrics;

        private TaskSuspensionToken _manageLyricSuspensionToken;
        private TaskSuspensionToken _manageLyricsRollSuspensionToken;
        private TaskSuspensionToken _applyLyricSuspensionToken;

        private CancellationTokenSource _cancellationTokenSource;

        private bool _disposed;

        private const int LYRIC_OFFSET = 0;
        
        public event LyricChangedEventHandler LyricChanged;
        public event LyricsFoundEventHandler LyricsFound;
        public event LyricsPercentageUpdatedEventHandler LyricsPercentageUpdated;

        public LyricHandler(SongHandler songHandler)
        {
            this._debugger = new Debugger<LyricHandler>(this);

            this._lyricCollector = new LyricCollector();

            this._oldLyrics = new LyricData();

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
            
            this.LyricChanged += OnLyricChanged;
            
            this._disposed = false;
        }

        private void OnLyricChanged(object sender, LyricChangedEventArgs lyricChangedEventArgs)
        {
            Shared.Structure.Song.Song currentSong = this._songHandler.CurrentSong;

            if (currentSong.Lyrics.LyricParts.IsNullOrEmpty())
                return;
            
            for (int i = 0; i < currentSong.Lyrics.LyricParts.Length; i++)
            {
                LyricPart currentPart = currentSong.Lyrics.LyricParts[i];

                if (i + 1 < currentSong.Lyrics.LyricParts.Length)
                {
                    LyricPart nextPart = currentSong.Lyrics.LyricParts[i + 1];

                    if (currentPart.Equals(lyricChangedEventArgs.LyricPart))
                    {
                        long time = nextPart.Time - currentSong.CurrentLyricPart.Time;
                        long currentTime = currentSong.Time - currentSong.CurrentLyricPart.Time;
                        double change = Math.Round((double)(100 * currentTime) / time);

                        lyricChangedEventArgs.LyricPart.Percentage = change;
                        PercentageUpdatedEvent(lyricChangedEventArgs.LyricPart, change);
                    }
                }
                else
                {
                    if (currentPart.Equals(lyricChangedEventArgs.LyricPart))
                    {
                        long time = currentSong.SongMetadata.MaxTime - currentSong.CurrentLyricPart.Time;
                        long currentTime = currentSong.Time - currentSong.CurrentLyricPart.Time;
                        double change = Math.Round((double)(100 * currentTime) / time);
                                    
                        lyricChangedEventArgs.LyricPart.Percentage = change;
                        PercentageUpdatedEvent(lyricChangedEventArgs.LyricPart, change);
                    }
                }
            }
        }

        private async Task ApplyLyricsToSong()
        {
            while (!this._disposed)
            {
                await this._applyLyricSuspensionToken.WaitForRelease();
                await Task.Delay(100);

                Shared.Structure.Song.Song song = _songHandler.CurrentSong;

                if (DataValidator.ValidateData(song) &&
                    DataValidator.ValidateData(song.SongMetadata.Name) &&
                    DataValidator.ValidateData(song.SongMetadata.Artists) &&
                    DataValidator.ValidateData(song.SongMetadata.MaxTime) &&
                    DataValidator.ValidateData(song.SongMetadata.Album) &&
                    DataValidator.ValidateData(song.SongMetadata) &&
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
                        
                        if (this._oldLyrics.Equals(lyricData))
                            continue;

                        this._oldLyrics = lyricData;
                        
                        LyricsFoundEvent(lyricData);
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
                    Shared.Structure.Song.Song currentSong = this._songHandler.CurrentSong;

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
                                            LyricChangedEvent(new LyricChangedEventArgs(currentPart));
                                            continue;
                                        }
                                    }
                                }
                                else 
                                {
                                    if (MathUtils.IsInRange(currentPart.Time, currentSong.SongMetadata.MaxTime, currentSong.Time + LYRIC_OFFSET))
                                    {
                                        currentSong.CurrentLyricPart = currentPart;
                                        LyricChangedEvent(new LyricChangedEventArgs(currentPart));
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
            if (Core.INSTANCE.CacheManager.IsLyricsInCache(songResponseObject.SongRequestObject))
                return;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            songChangedEventArgs.Song.State = SongState.SEARCHING_LYRICS;
            await this._lyricCollector.CollectLyrics(songResponseObject);
            songChangedEventArgs.Song.State = SongState.SEARCHING_FINISHED;

            this._debugger.Write("Took " + stopwatch.ElapsedMilliseconds + "ms to fetch the lyrics!", DebugType.INFO);
        }
        
        protected virtual void LyricChangedEvent(LyricChangedEventArgs lyricChangedEventArgs)
        {
            LyricChangedEventHandler lyricChangedEventHandler = LyricChanged;
            lyricChangedEventHandler?.Invoke(this, lyricChangedEventArgs);
        }
        
        protected virtual void LyricsFoundEvent(LyricData data)
        {
            LyricsFoundEventHandler foundEventHandler = LyricsFound;
            foundEventHandler?.Invoke(this, new LyricsFoundEventArgs(data));
        }

        protected virtual void PercentageUpdatedEvent(LyricPart lyricPart, double percentage)
        {
            LyricsPercentageUpdatedEventArgs args = new LyricsPercentageUpdatedEventArgs(lyricPart, percentage);
            LyricsPercentageUpdatedEventHandler handler = LyricsPercentageUpdated;
            handler?.Invoke(this, args);
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
