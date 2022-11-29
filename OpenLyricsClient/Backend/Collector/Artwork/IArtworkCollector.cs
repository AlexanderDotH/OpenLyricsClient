using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Song;

namespace OpenLyricsClient.Backend.Collector.Artwork
{
    interface IArtworkCollector
    {
        Task<Structure.Artwork.Artwork> GetArtwork(SongResponseObject songResponseObject);
        string CollectorName();

    }
}
