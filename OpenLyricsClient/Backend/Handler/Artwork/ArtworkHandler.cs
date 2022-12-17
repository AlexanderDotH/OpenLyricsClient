using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Cache;
using OpenLyricsClient.Backend.Collector.Artwork;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Handler.Artwork
{
    public class ArtworkHandler : IHandler
    {
        private Debugger<ArtworkHandler> _debugger;

        private ArtworkCollector _artworkCollector;
        private SongHandler _songHandler;

        private TaskSuspensionToken _applyArtworkSuspensionToken;

        private bool _disposed;

        public ArtworkHandler(SongHandler songHandler)
        {
            this._debugger = new Debugger<ArtworkHandler>(this);
            
            this._disposed = false;
            
            this._songHandler = songHandler;
            this._artworkCollector = new ArtworkCollector();
            
            Core.INSTANCE.TaskRegister.Register(
                out _applyArtworkSuspensionToken, 
                new Task(async () => await ApplyArtworkTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
                EnumRegisterTypes.APPLY_ARTWORK_TO_SONG);
        }

        public async Task FireArtworkSearch(SongResponseObject songResponseObject, SongChangedEventArgs songChangedEventArgs)
        {
            if (songChangedEventArgs.EventType == EventType.PRE)
                return;
            
            if (DataValidator.ValidateData(songChangedEventArgs) &&
                DataValidator.ValidateData(songChangedEventArgs.Song))
            {
                if (Core.INSTANCE.CacheManager.IsArtworkInCache(songResponseObject.SongRequestObject))
                    return;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                await this._artworkCollector.CollectArtwork(songResponseObject);

                this._debugger.Write("Took " + stopwatch.ElapsedMilliseconds + "ms to fetch the artwork!", DebugType.INFO);
            }
        }

        private async Task ApplyArtworkTask()
        {
            while (!this._disposed)
            {
                await Task.Delay(500);

                Structure.Song.Song song = this._songHandler.CurrentSong;
                
                if (!DataValidator.ValidateData(song))
                    continue;
                
                if (!DataValidator.ValidateData(song.SongMetadata))
                    continue;
                
                SongRequestObject songRequestObject = SongRequestObject.FromSong(song);
                
                if (!DataValidator.ValidateData(songRequestObject))
                    continue;

                Structure.Artwork.Artwork artworkCache = Core.INSTANCE.CacheManager.GetArtworkByRequest(songRequestObject);

                if (!DataValidator.ValidateData(artworkCache))
                {
                    continue;
                }
                
                if (artworkCache.Equals(song.Artwork))
                    continue;

                if (artworkCache.ArtworkColor.A == 0 &&
                    artworkCache.ArtworkColor.R == 0 &&
                    artworkCache.ArtworkColor.G == 0 &&
                    artworkCache.ArtworkColor.B == 0)
                {
                    await artworkCache.CalculateColor();
                    Core.INSTANCE.CacheManager.WriteToCache(songRequestObject, artworkCache);
                }
                
                song.Artwork = artworkCache;
                
                if (!DataValidator.ValidateData(artworkCache.ArtworkColor))
                    continue;

                SolidColorBrush primaryColor = App.Current.FindResource("PrimaryColorBrush") as SolidColorBrush;
                SolidColorBrush color = App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush;
                SolidColorBrush textColor = App.Current.FindResource("PrimaryThemeFontColorBrush") as SolidColorBrush;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (color.Color == Color.FromRgb(22,22,22))
                    {
                        color.Color = primaryColor.Color;
                    }
                    else
                    {
                        color.Color = artworkCache.ArtworkColor;
                    }
                    
                    color.Color = artworkCache.ArtworkColor;
                    
                    if (artworkCache.GetBrightness() < 15)
                    {
                        textColor.Color = new Color(255, 255, 255, 255);
                    }
                    else
                    {
                        textColor.Color = new Color(255, 0, 0, 0);
                    }
                });
            }
        }

        public void Dispose()
        {
            this._disposed = true;
            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.APPLY_LYRICS_TO_SONG);
        }
    }
}
