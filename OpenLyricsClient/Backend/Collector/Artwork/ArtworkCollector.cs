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
using OpenLyricsClient.Backend.Structure.Song;

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

        public async Task<Structure.Artwork.Artwork> CollectArtwork(SongRequestObject songRequestObject)
        {
            for (int i = 0; i < this._artworkCollectors.Length; i++)
            {
                
            }

            return null;
        }

    }
}
