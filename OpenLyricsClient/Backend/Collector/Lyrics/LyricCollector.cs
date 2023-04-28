using System.Threading.Tasks;
using DevBase.Generics;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.Deezer;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEase;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEaseV2;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.OpenLyricsClient;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.Textyl;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Lyrics
{
    class LyricCollector
    {
        private AList<ILyricsCollector> _lyricCollectors;
        private SongResponseObject _last;
        private int _lastRetry;
        private bool _isBusy;
        
        public LyricCollector()
        {
            this._lyricCollectors = new AList<ILyricsCollector>();
            
            this._lyricCollectors.Add(new MusixmatchCollector());
            this._lyricCollectors.Add(new DeezerCollector());
            this._lyricCollectors.Add(new NetEaseCollector());
            this._lyricCollectors.Add(new NetEaseV2Collector());
            this._lyricCollectors.Add(new TextylCollector());
            this._lyricCollectors.Add(new OpenLyricsClientCollector());
        }

        public async Task CollectLyrics(SongResponseObject songResponseObject)
        {
            if (!DataValidator.ValidateData(songResponseObject))
                return;
            
            if (!DataValidator.ValidateData(songResponseObject.SongRequestObject))
                return;
            
            if (Core.INSTANCE.CacheManager.IsLyricsInCache(songResponseObject.SongRequestObject))
                return;

            for (int i = 0; i < this._lyricCollectors.Length; i++)
            {
                if (Core.INSTANCE.CacheManager.IsLyricsInCache(songResponseObject.SongRequestObject, true))
                    return;

                ILyricsCollector collector = this._lyricCollectors.Get(i);
                LyricData lyricData = await collector.GetLyrics(songResponseObject);

                if (!DataValidator.ValidateData(lyricData))
                    continue;

                if (lyricData.LyricReturnCode != LyricReturnCode.SUCCESS)
                    continue;

                Core.INSTANCE.CacheManager.WriteToCache(songResponseObject.SongRequestObject, lyricData);
                return;
            }

            //this._isBusy = false;
        }
    }
}
