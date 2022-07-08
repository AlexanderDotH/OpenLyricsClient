using System.Collections.Generic;

namespace OpenLyricsClient.Backend.Collector.Lyrics
{
    class CollectorComparer : IComparer<ICollector>
    {
        public int Compare(ICollector x, ICollector y)
        {
            if (x == null || y == null)
                return 0;

            return y.ProviderQuality().CompareTo(x.ProviderQuality());
        }
    }
}
