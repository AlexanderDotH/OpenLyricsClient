using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using DevBase.Web;
using DevBase.Web.RequestData;
using DevBase.Web.ResponseData;
using LyricsWPF.Backend.Collector.Providers.NetEase.Json;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;
using Newtonsoft.Json;
using Opportunity.LrcParser;

namespace LyricsWPF.Backend.Collector.Providers.NetEase
{
    class NetEaseCollector : ICollector
    {
        private string _baseUrl;

        private Debugger<NetEaseCollector> _debugger;

        private const int RETRIES = 5;
        private const double RETRY_DURATION_MULTIPLIER = 15f;

        public NetEaseCollector()
        {
            this._baseUrl = "https://music.163.com/api";

            _debugger = new Debugger<NetEaseCollector>(this);
        }

        public LyricData GetLyrics(SongRequestObject songRequestObject)
        {
            if (DataValidator.ValidateData(songRequestObject, songRequestObject.SongName, songRequestObject.Artists, songRequestObject.SongDuration))
            {
                NetEaseSearchResponse response = SearchTrack(songRequestObject);

                if (response != null)
                {
                    if (response.Code == 200)
                    {
                        if (DataValidator.ValidateData(response.NetEaseResultDataResponse))
                        {
                            if (DataValidator.ValidateData(response.NetEaseResultDataResponse.Songs))
                            {
                                if (response.NetEaseResultDataResponse.Songs.Count > 0)
                                {
                                    //Sicherstellen, dass der richtige song geholt wird
                                    //Hier überprüfe ich, ober der artist unter den song artists ist

                                    //Was getan wird:
                                    //Wenn x% an artists vorhanden sind
                                    //Wenn die song duration mit einem gewissen +- prozensatz vorhanden ist,
                                    //dieser prozentsatz wird nach jedem versuch multipliziert, um trotzdem einen song erhalten zu können

                                    int retryPercentage = 5;

                                    for (int i = 0; i < RETRIES; i++)
                                    {
                                        for (int j = 0; j < response.NetEaseResultDataResponse.Songs.Count; j++)
                                        {
                                            NetEaseSongResponse songResponse = response.NetEaseResultDataResponse.Songs[j];

                                            if (MatchDuration(songResponse, songRequestObject.SongDuration, retryPercentage))
                                            {
                                                if (MatchArtists(songResponse, songRequestObject.Artists, 100))
                                                {
                                                    int songId = songResponse.Id;
                                                    NetEaseLyricResponse lyricResponse = GetLyricsFromEndpoint(songId);
                                                    if (lyricResponse != null)
                                                    {
                                                        //Parsed die lyrics
                                                        if (lyricResponse.NetEaseLrcResponse != null)
                                                        {
                                                            NetEaseLrcResponse netEaseLrcResponse = lyricResponse.NetEaseLrcResponse;

                                                            Lyrics<Line> lyricParts = Opportunity.LrcParser.Lyrics.Parse(netEaseLrcResponse.Lyric).Lyrics;

                                                            this._debugger.Write("Found new Lyrics", DebugType.DEBUG);

                                                            return LyricData.ConvertToData(lyricParts);
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        retryPercentage = (int)Math.Ceiling(i * RETRY_DURATION_MULTIPLIER);
                                    }

                                    //for (int i = 0; i < response.NetEaseResultDataResponse.Songs.Count; i++)
                                    //{
                                    //    NetEaseSongResponse songResponse = response.NetEaseResultDataResponse.Songs[i];

                                    //    if (MatchDuration(songResponse, songRequestObject.SongDuration, 5))
                                    //    {
                                    //        if (MatchArtists(songResponse, songRequestObject.Artists, 100))
                                    //        {
                                    //            int songId = songResponse.Id;
                                    //            NetEaseLyricResponse lyricResponse = GetLyricsFromEndpoint(songId);
                                    //            if (lyricResponse != null)
                                    //            {
                                    //                //Parsed die lyrics
                                    //                if (lyricResponse.NetEaseLrcResponse != null)
                                    //                {
                                    //                    NetEaseLrcResponse netEaseLrcResponse = lyricResponse.NetEaseLrcResponse;
                                    //                    Lyrics<Line> lyricParts = Opportunity.LrcParser.Lyrics.Parse(netEaseLrcResponse.Lyric).Lyrics;

                                    //                    this._debugger.Write("Found new Lyrics", DebugType.DEBUG);

                                    //                    return LyricData.ConvertToData(lyricParts);
                                    //                }
                                    //            }
                                    //        }
                                    //    }
                                    ////}
                                }
                            }
                        }
                    }
                }
            }

            return new LyricData(LyricReturnCode.Failed, null);
        }

        private bool MatchDuration(NetEaseSongResponse netEaseSongResponse, long duration, int percentage)
        {
            long songDurationThreshold = (long)((duration * 0.01) * percentage);
            return MathUtils.IsInRange(duration - songDurationThreshold, duration + songDurationThreshold, netEaseSongResponse.Duration);
        }

        private bool MatchArtists(NetEaseSongResponse netEaseSongResponse, string[] artists, double percentage)
        {
            double minArtistCount = Math.Floor((artists.Length * 0.01) * percentage);
            int artistsMatch = 0;

            if (artists.Length > 0)
            {
                for (int i = 0; i < netEaseSongResponse.Artists.Count; i++)
                {
                    for (int j = 0; j < artists.Length; j++)
                    {
                        if (netEaseSongResponse.Artists[i].Name == artists[j])
                        {
                            artistsMatch++;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < netEaseSongResponse.Artists.Count; i++)
                {
                    if (netEaseSongResponse.Artists[i].Name == artists[0])
                    {
                        artistsMatch++;
                    }
                }
            }

            return artistsMatch >= minArtistCount;
        }

        private NetEaseSearchResponse SearchTrack(SongRequestObject songRequestObject)
        {
            string requestUrl = Uri.EscapeUriString(string.Format(
                "{0}/search/get?s={1}+{2}&type=1&offset=0&sub=false&limit=50",
                this._baseUrl, songRequestObject.GetArtistsSplit(), songRequestObject.SongName));

            this._debugger.Write("Full track search URL: " + requestUrl, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestUrl);
            ResponseData responseData = request.GetResponse();

            this._debugger.Write(responseData.GetContentAsString(), DebugType.DEBUG);

            if (responseData.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<NetEaseSearchResponse>(
                    responseData.GetContentAsString());
            }

            return null;
        }

        private NetEaseLyricResponse GetLyricsFromEndpoint(int songId)
        {
            string requestURL = Uri.EscapeUriString(string.Format("{0}/song/lyric?tv=-1&kv=-1&lv=-1&os=pc&id={1}", this._baseUrl, songId));

            this._debugger.Write("Full lyric fetch URL: " + requestURL, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestURL);
            ResponseData responseData = request.GetResponse();

            this._debugger.Write(responseData.GetContentAsString(), DebugType.DEBUG);

            if (responseData.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<NetEaseLyricResponse>(
                    responseData.GetContentAsString());
            }

            return null;
        }

        public string CollectorName()
        {
            return "NetEase";
        }
    }
}
