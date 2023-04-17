using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generics;
using OpenLyricsClient.Backend.Collector.Artwork.Providers.Deezer;
using OpenLyricsClient.Backend.Collector.Artwork.Providers.Musixmatch;
using OpenLyricsClient.Backend.Collector.Artwork.Providers.Spotify;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Events.EventHandler;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Artwork;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Artwork
{
    public class ArtworkCollector
    {
        private AList<IArtworkCollector> _artworkCollectors;

        public ArtworkCollector()
        {
            this._artworkCollectors = new AList<IArtworkCollector>();
            this._artworkCollectors.Add(new SpotifyCollector());
            this._artworkCollectors.Add(new DeezerCollector());
            //this._artworkCollectors.Add(new MusixMatchCollector());
        }

        public async Task CollectArtwork(SongResponseObject songResponseObject)
        {
            if (Core.INSTANCE.CacheManager.IsArtworkInCache(songResponseObject.SongRequestObject))
                return;
            
            this._artworkCollectors.Sort(new ArtworkCollectorComparer());
            
            for (int i = 0; i < this._artworkCollectors.Length; i++)
            {
                IArtworkCollector artworkCollector = this._artworkCollectors.Get(i);

                Structure.Artwork.Artwork artwork = await artworkCollector.GetArtwork(songResponseObject);

                if (!DataValidator.ValidateData(artwork))
                    continue;
                
                if (artwork.ReturnCode != ArtworkReturnCode.SUCCESS || artwork.Data == null)
                    continue;

                if (Core.INSTANCE.CacheManager.IsArtworkInCache(songResponseObject.SongRequestObject))
                    continue;

                if (artwork.ReturnCode == ArtworkReturnCode.SUCCESS)
                {
                    Core.INSTANCE.CacheManager.WriteToCache(songResponseObject.SongRequestObject, artwork);
                }
            }
        }
    }
}
