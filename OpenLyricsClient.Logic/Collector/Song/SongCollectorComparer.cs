namespace OpenLyricsClient.Logic.Collector.Song;

class SongCollectorComparer : IComparer<ISongCollector>
{
    public int Compare(ISongCollector x, ISongCollector y)
    {
        if (x == null || y == null)
            return 0;

        return y.ProviderQuality().CompareTo(x.ProviderQuality());
    }
}