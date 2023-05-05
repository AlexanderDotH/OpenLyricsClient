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
using OpenLyricsClient.Backend.Events.EventHandler;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Backend.Handler.Artwork
{
    public class ArtworkHandler : IHandler
    {
        private Debugger<ArtworkHandler> _debugger;

        private ArtworkCollector _artworkCollector;
        private SongHandler _songHandler;

        private TaskSuspensionToken _applyArtworkSuspensionToken;

        private bool _disposed;

        private Shared.Structure.Artwork.Artwork _oldArtwork;
        
        public event ArtworkFoundEventHandler ArtworkFoundHandler;
        public event ArtworkAppliedEventHandler ArtworkAppliedHandler;
        
        public ArtworkHandler(SongHandler songHandler)
        {
            this._debugger = new Debugger<ArtworkHandler>(this);
            
            this._disposed = false;
            
            this._songHandler = songHandler;
            this._artworkCollector = new ArtworkCollector();
            
            ArtworkFoundHandler += OnArtworkFoundHandler;
            
            
            Core.INSTANCE.TaskRegister.Register(
                out _applyArtworkSuspensionToken, 
                new Task(async () => await ApplyArtworkTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
                EnumRegisterTypes.APPLY_ARTWORK_TO_SONG);
        }

        private void OnArtworkFoundHandler(object sender, ArtworkFoundEventArgs args)
        {
            Shared.Structure.Song.Song song = this._songHandler.CurrentSong;
            song.Artwork = args.Artwork;
            
            ArtworkAppliedEvent(args.Artwork);
                
            if (!DataValidator.ValidateData(args.Artwork.ArtworkColor))
                return;

            SolidColorBrush primaryColor = App.Current.FindResource("PrimaryColorBrush") as SolidColorBrush;
            SolidColorBrush color = App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush;
            SolidColorBrush secondaryColor = App.Current.FindResource("SecondaryThemeColorBrush") as SolidColorBrush;
            SolidColorBrush textColor = App.Current.FindResource("PrimaryThemeFontColorBrush") as SolidColorBrush;
            SolidColorBrush secondaryTextColor = App.Current.FindResource("SecondaryThemeFontColorBrush") as SolidColorBrush;
            SolidColorBrush lightTextColor = App.Current.FindResource("LightThemeFontColorBrush") as SolidColorBrush;

            SolidColorBrush selectedLineTextColor = App.Current.FindResource("SelectedLineFontColorBrush") as SolidColorBrush;
            SolidColorBrush unSelectedLineTextColor = App.Current.FindResource("UnSelectedLineFontColorBrush") as SolidColorBrush;

            Dispatcher.UIThread.InvokeAsync(() =>
            { 
                if (color.Color == Color.FromRgb(22, 22, 22))
                { 
                    color.Color = primaryColor!.Color;
                }
                else
                { 
                    color.Color = args.Artwork.ArtworkColor;
                }

                color.Color = args.Artwork.ArtworkColor;
                secondaryColor!.Color = args.Artwork.DarkArtworkColor;

                byte light = 120;
                byte primary = 22;
                byte secondary = 40;

                byte selected = 38;
                byte unselected = 70;

                byte darkSelected = 70;
                byte darkUnselected = 38;

                byte minR = (byte)Math.Round((double)(color.Color.R / 100.0));
                byte minG = (byte)Math.Round((double)(color.Color.G / 100.0));
                byte minB = (byte)Math.Round((double)(color.Color.B / 100.0));

                if (args.Artwork.GetBrightness() < 30)
                {
                    selectedLineTextColor!.Color = new Color(255, (byte)(minR * darkSelected),
                            (byte)(minG * darkSelected), (byte)(minB * darkSelected));
                    
                    unSelectedLineTextColor!.Color = new Color(255, (byte)(minR * darkUnselected),
                            (byte)(minG * darkUnselected), (byte)(minB * darkUnselected));

                    lightTextColor!.Color = new Color(255, (byte)(color.Color.R - light),
                        (byte)(color.Color.G - light), (byte)(color.Color.B - light));

                    textColor!.Color = new Color(255, (byte)(255 - primary), 
                        (byte)(255 - primary), (byte)(255 - primary));
                    
                    secondaryTextColor!.Color = new Color(255, (byte)(255 - secondary), 
                        (byte)(255 - secondary), (byte)(255 - secondary));
                }
                else
                {
                    selectedLineTextColor!.Color = new Color(255, (byte)(minR * selected), (byte)(minG * selected), 
                        (byte)(minB * selected)); 
                    
                    unSelectedLineTextColor!.Color = new Color(255, (byte)(minR * unselected),
                            (byte)(minG * unselected), (byte)(minB * unselected));
                    
                    lightTextColor!.Color = new Color(255, light, light, light);
                    
                    textColor!.Color = new Color(255, primary, primary, primary);
                    
                    secondaryTextColor!.Color = new Color(255, secondary, secondary, secondary);
                    
                }
                
            });
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

                Shared.Structure.Song.Song song = this._songHandler.CurrentSong;
                
                if (!DataValidator.ValidateData(song))
                    continue;
                
                if (!DataValidator.ValidateData(song.SongMetadata))
                    continue;
                
                SongRequestObject songRequestObject = SongRequestObject.FromSong(song);
                
                if (!DataValidator.ValidateData(songRequestObject))
                    continue;

                Shared.Structure.Artwork.Artwork artworkCache = Core.INSTANCE.CacheManager.GetArtworkByRequest(songRequestObject);

                if (!DataValidator.ValidateData(artworkCache))
                    continue;

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

                if (!DataValidator.ValidateData(artworkCache))
                    continue;
                
                if (this._oldArtwork == null || this._oldArtwork != artworkCache)
                    ArtworkFoundEvent(songRequestObject, artworkCache);

                this._oldArtwork = artworkCache;
            }
        }

        protected virtual void ArtworkFoundEvent(SongRequestObject songResponseObject, Shared.Structure.Artwork.Artwork artwork)
        {
            ArtworkFoundEventHandler artworkFound = ArtworkFoundHandler;
            artworkFound?.Invoke(this, new ArtworkFoundEventArgs(artwork, songResponseObject));
        }
        
        protected virtual void ArtworkAppliedEvent(Shared.Structure.Artwork.Artwork artwork)
        {
            ArtworkAppliedEventHandler artworkApplied = ArtworkAppliedHandler;
            artworkApplied?.Invoke(this, new ArtworkAppliedEventArgs(artwork));
        }
        
        public void Dispose()
        {
            this._disposed = true;
            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.APPLY_LYRICS_TO_SONG);
        }
    }
}
