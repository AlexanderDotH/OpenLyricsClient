using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Song;

namespace OpenLyricsClient.Backend.Collector.Artwork.Providers.Genius
{
    class GeniusCollector : IArtworkCollector
    {
        private Debugger<GeniusCollector> _debugger;
        private readonly string _baseUrl;

        public GeniusCollector()
        {
            this._debugger = new Debugger<GeniusCollector>(this);
            this._baseUrl = "";
        }
        

        public async Task<Structure.Artwork.Artwork> GetArtwork(SongRequestObject songRequestObject)
        {
            throw new NotImplementedException();
        }
    }
}
