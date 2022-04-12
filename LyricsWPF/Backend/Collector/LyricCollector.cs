using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Collector.Providers.NetEase;
using LyricsWPF.Backend.Exceptions;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;

namespace LyricsWPF.Backend.Collector
{
    class LyricCollector
    {
        private List<ICollector> _lyricCollectors;

        public LyricCollector()
        {
            this._lyricCollectors = new List<ICollector>();
            this._lyricCollectors.Add(new NetEaseCollector());
        }

        public LyricData CollectLyrics(SongRequestObject songRequestObject, string collectorName)
        {
            ICollector collector = GetCollectorByName(collectorName);
            return collector.GetLyrics(songRequestObject);
        }

        public ICollector GetCollectorByName(string collectorName)
        {
            foreach (ICollector collector in _lyricCollectors)
            {
                if (collector.CollectorName().Equals(collectorName))
                {
                    return collector;
                }
            }

            return this._lyricCollectors[0];
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
