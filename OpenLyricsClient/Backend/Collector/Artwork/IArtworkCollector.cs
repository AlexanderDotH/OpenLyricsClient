using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Shared.Structure;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Backend.Collector.Artwork
{
    interface IArtworkCollector
    {
        Task<Shared.Structure.Artwork.Artwork> GetArtwork(SongResponseObject songResponseObject);
        string CollectorName();
        int Quality();

    }
}
