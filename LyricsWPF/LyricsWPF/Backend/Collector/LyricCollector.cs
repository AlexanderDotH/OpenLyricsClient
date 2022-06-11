using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using LyricsWPF.Backend.Collector.Cache;
using LyricsWPF.Backend.Collector.Providers.Musixmatch;
using LyricsWPF.Backend.Collector.Providers.NetEase;
using LyricsWPF.Backend.Collector.Providers.NetEaseV2;
using LyricsWPF.Backend.Exceptions;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Collector
{
    class LyricCollector
    {
        private GenericList<ICollector> _lyricCollectors;
        private CacheManager _cacheManager;

        public LyricCollector()
        {
            this._lyricCollectors = new GenericList<ICollector>();
            this._lyricCollectors.Add(new NetEaseCollector());
            this._lyricCollectors.Add(new NetEaseV2Collector());
            this._lyricCollectors.Add(new MusixMatchCollector());
            
            this._cacheManager = new CacheManager();
        }

        public async Task CollectLyrics(SongRequestObject songRequestObject)
        {
            if (this._cacheManager.IsInCache(songRequestObject))
                return;
            
            this._lyricCollectors.Sort(new CollectorComparer());

            for (int i = 0; i < this._lyricCollectors.Length; i++)
            {
                ICollector collector = this._lyricCollectors.Get(i);
                LyricData lyricData = await collector.GetLyrics(songRequestObject);

                if (DataValidator.ValidateData(lyricData))
                {
                    if (lyricData.LyricReturnCode == LyricReturnCode.Success)
                    {
                        this._cacheManager.WriteToCache(songRequestObject, lyricData);
                    }
                }
            }
            
            GC.Collect();
        }

        public CacheManager CacheManager
        {
            get => _cacheManager;
        }
    }
}
