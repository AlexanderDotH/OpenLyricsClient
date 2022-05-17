using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.Web;
using DevBase.Web.ResponseData;
using DevBaseFormat;
using DevBaseFormat.Formats.LrcFormat;
using DevBaseFormat.Structure;
using LyricsWPF.Backend.Collector.Providers.NetEaseV2.Json;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.NetEaseV2
{
    class NetEaseV2Collector : ICollector
    {

        private string _baseUrl;

        private Debugger<NetEaseV2Collector> _debugger;

        private const int RETRIES = 5;
        private const double RETRY_DURATION_MULTIPLIER = 15f;

        public NetEaseV2Collector()
        {
            this._debugger = new Debugger<NetEaseV2Collector>(this);
            this._baseUrl = "https://music.xianqiao.wang/neteaseapiv2";
        }

        public async Task<LyricData> GetLyrics(SongRequestObject songRequestObject)
        {
            if (DataValidator.ValidateData(songRequestObject) &&
                DataValidator.ValidateData(songRequestObject.Artists, songRequestObject.SongDuration, songRequestObject.SongName, songRequestObject.Album))
            {
                NetEaseV2SearchResponse response = await SearchTrack(songRequestObject);

                if (DataValidator.ValidateData(response) &&
                    DataValidator.ValidateData(response.Code, response.Result))
                {
                    if (response.Code == 200)
                    {
                        if (DataValidator.ValidateData(response.Result) &&
                            DataValidator.ValidateData(response.Result.HasMore, response.Result.SongCount, response.Result.Songs))
                        {

                            if (response.Result.Songs.Count > 0)
                            {
                                GenericList<Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse>> lyrics = 
                                    new GenericList<Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse>>();

                                double retryPercentage = 5;

                                for (int i = 0; i < RETRIES; i++)
                                {
                                    for (int j = 0; j < response.Result.Songs.Count; j++)
                                    {
                                        NetEaseV2SongResponse songResponse = response.Result.Songs[j];

                                        if (IsValidSong(songResponse, songRequestObject, retryPercentage))
                                        {
                                            int songId = songResponse.Id;
                                            NetEaseV2LyricResponse lyricResponse = await GetLyricsFromEndpoint(songId);

                                            if (songRequestObject.SelectioMode == SelectionMode.QUALITY)
                                            {
                                                for (int k = 0; k < lyrics.Count; k++)
                                                {
                                                    Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse> lyricElement = lyrics[i];
                                                    if (lyricElement.Item2.Lrc.Lyric != "")
                                                    {
                                                        return ParseLyricResponse(lyricElement.Item2);
                                                    }
                                                }

                                                lyrics.Add(new Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse>(songResponse, lyricResponse));
                                            }
                                            else if (songRequestObject.SelectioMode == SelectionMode.PERFORMANCE)
                                            {
                                                return ParseLyricResponse(lyricResponse);
                                            }
                                        }

                                        retryPercentage = (int)Math.Ceiling(i * RETRY_DURATION_MULTIPLIER);
                                    }
                                }

                            //    if (songRequestObject.SelectioMode != SelectionMode.QUALITY)
                            //        return null;

                            //    retryPercentage = 5;

                            //    for (int i = 0; i < lyrics.Count; i++)
                            //    {
                            //        Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse> songResponse = lyrics[i];

                            //        if (IsValidSong(songResponse.Item1, songRequestObject, retryPercentage))
                            //        {
                            //            if (songResponse.Item2.Lrc.Lyric != "")
                            //            {
                            //                return ParseLyricResponse(songResponse.Item2);
                            //            }
                            //        }

                            //        retryPercentage = (int)Math.Ceiling(i * RETRY_DURATION_MULTIPLIER);

                            //    }
                            }
                        }
                    }
                }
            }

            return null;
        }

        private bool IsValidSong(NetEaseV2SongResponse songResponse, SongRequestObject songRequestObject, double percentage)
        {
            if (!(DataValidator.ValidateData(songResponse) || 
                  DataValidator.ValidateData(songRequestObject) ||
                percentage > 0))
                return false;

            if (DataTransformer.CapitalizeFirstLetter(songRequestObject.Album) !=
                DataTransformer.CapitalizeFirstLetter(songResponse.Album.Name))
                return false;

            if (!MatchDuration(songResponse, songRequestObject.SongDuration, percentage))
                return false;

            if (!MatchArtists(songResponse, songRequestObject.Artists, 70))
                return false;

            if (songRequestObject.SongName != songResponse.Name)
                return false;

            //if (MathUtils.CalculateLevenshteinDistance(songRequestObject.SongName, songResponse.Name) > 3)
                //return false;

            return true;
        }

        private LyricData ParseLyricResponse(NetEaseV2LyricResponse lyricResponse)
        {
            if (DataValidator.ValidateData(lyricResponse) && 
                DataValidator.ValidateData(lyricResponse.Code, lyricResponse.Klyric,
                                                            lyricResponse.Lrc))
            {
                if (lyricResponse.Code == 200)
                {
                    if (lyricResponse.Lrc.Lyric != null &&
                        lyricResponse.Lrc.Version != 0)
                    {
                        FileFormatParser<LrcObject> fileFormatParser =
                            new FileFormatParser<LrcObject>(
                                new LrcParser<LrcObject>());

                        if (DataValidator.ValidateData(fileFormatParser))
                        {
                            GenericList<LyricElement> lyricElements =
                                fileFormatParser.FormatFromString(lyricResponse.Lrc
                                    .Lyric).Lyrics;

                            if (DataValidator.ValidateData(lyricElements))
                            {
                                return LyricData.ConvertToData(lyricElements);
                            }
                        }
                    }
                }
            }

            return new LyricData(LyricReturnCode.Failed, null);
        }

        //Makes too many track search request: why the hell?
        private async Task<NetEaseV2SearchResponse> SearchTrack(SongRequestObject songRequestObject)
        {
            string requestUrl = Uri.EscapeUriString(string.Format("{0}/search?limit=10&type=1&keywords={2}",
                this._baseUrl,
                songRequestObject.GetArtistsSplit(), songRequestObject.SongName));

            this._debugger.Write("Full track search URL: " + requestUrl, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestUrl);
            ResponseData responseData = await request.GetResponseAsync();

            if (responseData.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<NetEaseV2SearchResponse>(responseData.GetContentAsString());
            }

            return null;
        }

        private async Task<NetEaseV2LyricResponse> GetLyricsFromEndpoint(int songId)
        {
            string requestURL = Uri.EscapeUriString(string.Format("{0}/lyric?id={1}", this._baseUrl, songId));

            this._debugger.Write("Full lyric fetch URL: " + requestURL, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestURL);
            ResponseData responseData = await request.GetResponseAsync();

            this._debugger.Write(responseData.GetContentAsString(), DebugType.DEBUG);

            if (responseData.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<NetEaseV2LyricResponse>(
                    responseData.GetContentAsString());
            }

            return null;
        }

        private bool MatchDuration(NetEaseV2SongResponse netEaseSongResponse, long duration, double percentage)
        {
            long songDurationThreshold = (long)((duration * 0.01) * percentage);
            return MathUtils.IsInRange(duration - songDurationThreshold, duration + songDurationThreshold, netEaseSongResponse.Duration);
        }

        private bool MatchArtists(NetEaseV2SongResponse netEaseSongResponse, string[] artists, double percentage)
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


        public string CollectorName()
        {
            return "NetEaseV2";
        }

        public int ProviderQuality()
        {
            return 6;
        }
    }
}
