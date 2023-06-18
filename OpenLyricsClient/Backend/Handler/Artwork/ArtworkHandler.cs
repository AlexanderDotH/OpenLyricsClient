﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Async.Task;
using Material.Colors.ColorManipulation;
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
