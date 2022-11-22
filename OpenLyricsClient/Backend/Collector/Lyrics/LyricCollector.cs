using System.Threading.Tasks;
using DevBase.Generic;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEase;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEaseV2;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.Textyl;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Lyrics
{
    class LyricCollector
    {
        private GenericList<ICollector> _lyricCollectors;

        public LyricCollector()
        {
            this._lyricCollectors = new GenericList<ICollector>();
            this._lyricCollectors.Add(new NetEaseCollector());
            this._lyricCollectors.Add(new NetEaseV2Collector());
            this._lyricCollectors.Add(new MusixMatchCollector());
            this._lyricCollectors.Add(new TextylCollector());
        }

        public async Task CollectLyrics(SongRequestObject songRequestObject)
        {
            if (Core.INSTANCE.CacheManager.IsLyricsInCache(songRequestObject))
                return;
            
            this._lyricCollectors.Sort(new CollectorComparer());

            for (int i = 0; i < this._lyricCollectors.Length; i++)
            {
                if (Core.INSTANCE.CacheManager.IsLyricsInCache(songRequestObject))
                    break;

                ICollector collector = this._lyricCollectors.Get(i);
                LyricData lyricData = await collector.GetLyrics(songRequestObject);

                if (!DataValidator.ValidateData(lyricData))
                    continue;

                if (lyricData.LyricReturnCode != LyricReturnCode.SUCCESS)
                    continue;

                if (!Core.INSTANCE.CacheManager.IsLyricsInCache(songRequestObject))
                {
                    Core.INSTANCE.CacheManager.WriteToCache(songRequestObject, lyricData);
                    return;
                }
            }

            // if (!Core.INSTANCE.CacheManager.IsInCache(songRequestObject))
            //     Core.INSTANCE.CacheManager.AddToCache(songRequestObject, new LyricData());
        }
    }
}
