using System.Collections.Generic;
using OpenLyricsClient.Backend.Collector.Lyrics;

namespace OpenLyricsClient.Backend.Collector.Media;

class MediaCollectorComparer : IComparer<IMediaCollector>
{
    public int Compare(IMediaCollector x, IMediaCollector y)
    {
        if (x == null || y == null)
            return 0;

        return y.ProviderQuality().CompareTo(x.ProviderQuality());
    }
}