using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using OpenLyricsClient.Backend.Collector.Artwork.Providers.Musixmatch;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Artwork;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Artwork
{
    public class ArtworkCollector
    {

        private GenericList<IArtworkCollector> _artworkCollectors;
        private Debugger<ArtworkCollector> _debugger;

        public ArtworkCollector()
        {
            this._debugger = new Debugger<ArtworkCollector>(this);
            
            this._artworkCollectors = new GenericList<IArtworkCollector>();
            this._artworkCollectors.Add(new MusixMatchCollector());
        }

        public async Task CollectArtwork(SongRequestObject songRequestObject)
        {
            if (Core.INSTANCE.CacheManager.IsArtworkInCache(songRequestObject))
                return;
            
            for (int i = 0; i < this._artworkCollectors.Length; i++)
            {
                IArtworkCollector artworkCollector = this._artworkCollectors.Get(i);

                Structure.Artwork.Artwork artwork = await artworkCollector.GetArtwork(songRequestObject);

                if (!DataValidator.ValidateData(artwork))
                    continue;
                
                if (artwork.ReturnCode != ArtworkReturnCode.SUCCESS && artwork.Data == null)
                    continue;

                if (Core.INSTANCE.CacheManager.IsArtworkInCache(songRequestObject))
                    continue;
                
                Core.INSTANCE.CacheManager.WriteToCache(songRequestObject, artwork);
            }

        }

    }
}
