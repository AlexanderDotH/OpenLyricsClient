using System.Collections.Generic;

namespace OpenLyricsClient.Backend.Collector.Lyrics
{
    class CollectorComparer : IComparer<ILyricsCollector>
    {
        public int Compare(ILyricsCollector x, ILyricsCollector y)
        {
            if (x == null || y == null)
                return 0;

            return y.ProviderQuality().CompareTo(x.ProviderQuality());
        }
    }
}
