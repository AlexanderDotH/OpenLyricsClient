using System;
using System.Threading.Tasks;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Structure.Song;

namespace OpenLyricsClient.Backend.Collector.Artwork.Providers.Musixmatch
{
    class MusixMatchCollector : IArtworkCollector
    {
        private Debugger<MusixMatchCollector> _debugger;

        public MusixMatchCollector()
        {
            this._debugger = new Debugger<MusixMatchCollector>(this);
        }

        public async Task<Structure.Artwork.Artwork> GetArtwork(SongRequestObject songRequestObject)
        {
            throw new NotImplementedException();
        }
    }
}
