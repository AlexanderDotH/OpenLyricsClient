using System;
using System.Collections.Generic;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Lyrics;

namespace OpenLyricsClient.Backend.Collector.Lyrics
{
    class LyricsCollectorComparer : IComparer<Tuple<ICollector, LyricData>>
    {
        public int Compare(Tuple<ICollector, LyricData> x, Tuple<ICollector, LyricData> y)
        {
            if (x == null || y == null)
                return 0;

            return x.Item1.ProviderQuality().CompareTo(y.Item1.ProviderQuality());
        }
    }
}
