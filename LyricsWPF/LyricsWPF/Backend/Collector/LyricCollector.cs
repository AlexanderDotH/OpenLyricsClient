using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
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

        public LyricCollector()
        {
            this._lyricCollectors = new GenericList<ICollector>();
            this._lyricCollectors.Add(new NetEaseCollector());
            this._lyricCollectors.Add(new NetEaseV2Collector());
            this._lyricCollectors.Add(new MusixMatchCollector());

            this._lyricCollectors.Sort(new CollectorComparer());
        }

        public async Task<LyricData> CollectLyrics(SongRequestObject songRequestObject)
        {
            for (int i = 0; i < this._lyricCollectors.Length; i++)
            {
                ICollector collector = this._lyricCollectors.Get(i);
                LyricData lyricData = await collector.GetLyrics(songRequestObject);

                if (lyricData != null)
                    return lyricData;
            }

            //if (songRequestObject.SelectioMode == SelectionMode.QUALITY)
            //{
            //    GenericList<Tuple<ICollector, LyricData>> lyrics = new GenericList<Tuple<ICollector, LyricData>>();

            //    for (int i = 0; i < this._lyricCollectors.Count; i++)
            //    {
            //        ICollector collector = this._lyricCollectors[i];
            //        LyricData lyricData = await collector.GetLyrics(songRequestObject);

            //        lyrics.Add(new Tuple<ICollector, LyricData>(collector, lyricData));
            //    }

            //    lyrics.Sort(new LyricsCollectorComparer());

            //    for (int i = 0; i < lyrics.Count; i++)
            //    {
            //        Tuple<ICollector, LyricData> lyricData = lyrics[i];
            //        if (lyricData.Item2 != null)
            //        {
            //            return lyricData.Item2;
            //        }
            //    }
            //}
            //else if (songRequestObject.SelectioMode == SelectionMode.PERFORMANCE)
            //{
            //for (int i = 0; i < this._lyricCollectors.Length; i++)
            //{
            //    ICollector collector = this._lyricCollectors.Get(i);
            //    LyricData lyricData = null;

            //    while ((lyricData = await collector.GetLyrics(songRequestObject)) == null)
            //    {
            //        if (i + 1 < this._lyricCollectors.Length)
            //        {
            //            collector = this._lyricCollectors[i++];
            //        }
            //    }

            //    return lyricData;
            //}
            //}



            GC.Collect();
            return null;
        }

        public ICollector GetCollector()
        {
            for (int i = 0; i < this._lyricCollectors.Length; i++)
            {
                ICollector collector = this._lyricCollectors.Get(i);

                if (collector != null)
                    return collector;
            }

            return this._lyricCollectors.Get(0);
        }

        //public string CollectLyrics()
        //{
        //    if (this._songName == null)
        //        throw new LyricNotCollectableException();

        //    HttpWebRequest httpWebRequest = HttpWebRequest.CreateHttp(new Uri(_baseUrl + _songName));
        //    httpWebRequest.Method = "GET";
        //    httpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:97.0) Gecko/20100101 Firefox/97.0";
        //    httpWebRequest.Accept = "application/json;text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8";

        //    Stream stream = httpWebRequest.GetResponse().GetResponseStream();
        //    StreamReader streamReader = new StreamReader(stream);

        //    return streamReader.ReadToEnd();
        //}

        //public string SongName
        //{
        //    get { return this._songName; }
        //}
    }
}
