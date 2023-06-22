using DevBase.Generics;
using OpenLyricsClient.Logic.Collector.Lyrics.Providers.Deezer;
using OpenLyricsClient.Logic.Collector.Lyrics.Providers.Musixmatch;
using OpenLyricsClient.Logic.Collector.Lyrics.Providers.NetEase;
using OpenLyricsClient.Logic.Collector.Lyrics.Providers.NetEaseV2;
using OpenLyricsClient.Logic.Collector.Lyrics.Providers.OpenLyricsClient;
using OpenLyricsClient.Logic.Collector.Lyrics.Providers.Plugin;
using OpenLyricsClient.Logic.Collector.Lyrics.Providers.Textyl;
using OpenLyricsClient.Shared.Plugin;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Logic.Collector.Lyrics
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
            this._lyricCollectors.Add(new PluginLyricsCollector()); // i think OLC is always a fallback option that always works, so put it here
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
                if (Core.INSTANCE.CacheManager.IsLyricsInCache(songResponseObject.SongRequestObject))
                    continue;

                ILyricsCollector collector = this._lyricCollectors.Get(i);
                LyricData lyricData = await collector.GetLyrics(songResponseObject);

                if (!(DataValidator.ValidateData(lyricData) && 
                      DataValidator.ValidateData(lyricData.LyricParts, lyricData.LyricReturnCode)))
                    continue;

                if (lyricData.LyricReturnCode != LyricReturnCode.SUCCESS)
                    continue;

                foreach (IPlugin plugin in Core.INSTANCE.PluginManager.GetPluginsByScope(PluginScope.LyricsPostprocess))
                    lyricData = await plugin.ProcessLyrics(songResponseObject, lyricData);

                Core.INSTANCE.CacheManager.WriteToCache(songResponseObject.SongRequestObject, lyricData);
                return;
            }

            //this._isBusy = false;
        }
    }
}
