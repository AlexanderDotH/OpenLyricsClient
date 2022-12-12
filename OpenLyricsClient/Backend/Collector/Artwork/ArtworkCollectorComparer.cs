using System.Collections.Generic;
using OpenLyricsClient.Backend.Collector.Lyrics;

namespace OpenLyricsClient.Backend.Collector.Artwork;

class ArtworkCollectorComparer : IComparer<IArtworkCollector>
{
    public int Compare(IArtworkCollector x, IArtworkCollector y)
    {
        if (x == null || y == null)
            return 0;

        return y.Quality().CompareTo(x.Quality());
    }
}