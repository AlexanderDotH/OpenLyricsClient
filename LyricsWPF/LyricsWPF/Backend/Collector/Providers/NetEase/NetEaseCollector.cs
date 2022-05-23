using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.Web;
using DevBase.Web.RequestData;
using DevBase.Web.ResponseData;
using DevBaseFormat;
using DevBaseFormat.Formats.LrcFormat;
using DevBaseFormat.Structure;
using Kawazu;
using LyricsWPF.Backend.Collector.Providers.NetEase.Json;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;
using Newtonsoft.Json;

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

        public async Task<LyricData> GetLyrics(SongRequestObject songRequestObject)
        {
            if (DataValidator.ValidateData(songRequestObject) &&
                DataValidator.ValidateData(songRequestObject.Artists, songRequestObject.SongDuration,
                    songRequestObject.SongName, songRequestObject.Album))
            {
                NetEaseSearchResponse response = await SearchTrack(songRequestObject);

                if (DataValidator.ValidateData(response) &&
                    DataValidator.ValidateData(response.Code, response.NetEaseResultDataResponse))
                {
                    if (response.Code == 200)
                    {
                        if (DataValidator.ValidateData(response.NetEaseResultDataResponse) &&
                            DataValidator.ValidateData(response.NetEaseResultDataResponse.HasMore,
                                response.NetEaseResultDataResponse.SongCount, response.NetEaseResultDataResponse.Songs))
                        {
                            if (response.NetEaseResultDataResponse.Songs.Length > 0)
                            {
                                int retryPercentage = 5;

                                for (int i = 0; i < RETRIES; i++)
                                {
                                    for (int j = 0; j < response.NetEaseResultDataResponse.Songs.Length; j++)
                                    {
                                        NetEaseSongResponse songResponse = response.NetEaseResultDataResponse.Songs[j];

                                        if (DataValidator.ValidateData(songResponse) && DataValidator.ValidateData(
                                                songResponse.Name, songResponse.Artists, songResponse.Alias,
                                                songResponse.CopyrightId, songResponse.Duration, songResponse.Fee,
                                                songResponse.Ftype, songResponse.Id, songResponse.Mark,
                                                songResponse.Mvid, songResponse.NetEaseAlbumResponse,
                                                songResponse.Alias, songResponse.Status))
                                        {
                                            if (SongFormatter.FormatSongName(songResponse.Name).Equals(SongFormatter.FormatSongName(songRequestObject.SongName)) &&
                                                SongFormatter.FormatSongAlbum(songResponse.NetEaseAlbumResponse.Name).Equals(SongFormatter.FormatSongAlbum(songRequestObject.Album)))
                                            {
                                                if (MatchDuration(songResponse, songRequestObject.SongDuration, retryPercentage))
                                                {
                                                    if (MatchArtists(songResponse, songRequestObject.Artists, 100))
                                                    {
                                                        int songId = songResponse.Id;
                                                        NetEaseLyricResponse lyricResponse = await GetLyricsFromEndpoint(songId);

                                                        if (DataValidator.ValidateData(lyricResponse) &&
                                                            DataValidator.ValidateData(lyricResponse.Code,
                                                                lyricResponse.NetEaseKlyricResponse,
                                                                lyricResponse.NetEaseLrcResponse,
                                                                lyricResponse.NetEaseLyricUserResponse,
                                                                lyricResponse.NetEaseTlyricResponse,
                                                                lyricResponse.NetEaseTransUserResponse, lyricResponse.Qfy,
                                                                lyricResponse.Sfy, lyricResponse.Sgc))
                                                        {
                                                            FileFormatParser<LrcObject> fileFormatParser =
                                                                new FileFormatParser<LrcObject>(
                                                                    new LrcParser<LrcObject>());

                                                            if (DataValidator.ValidateData(fileFormatParser))
                                                            {
                                                                GenericList<LyricElement> lyrics =
                                                                    fileFormatParser.FormatFromString(lyricResponse.NetEaseLrcResponse.Lyric).Lyrics;

                                                                if (DataValidator.ValidateData(lyrics) && lyrics.Length > 0)
                                                                {
                                                                    this._debugger.Write("Found new Lyrics", DebugType.DEBUG);
                                                                    return await LyricData.ConvertToData(lyrics, this.CollectorName());
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    retryPercentage = (int)Math.Ceiling(i * RETRY_DURATION_MULTIPLIER);
                                }
                            }
                        }
                    }
                }
            }

            return new LyricData(LyricReturnCode.Failed, null, this.CollectorName());
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
                for (int i = 0; i < netEaseSongResponse.Artists.Length; i++)
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
                for (int i = 0; i < netEaseSongResponse.Artists.Length; i++)
                {
                    if (netEaseSongResponse.Artists[i].Name == artists[0])
                    {
                        artistsMatch++;
                    }
                }
            }

            return artistsMatch >= minArtistCount;
        }

        private async Task<NetEaseSearchResponse> SearchTrack(SongRequestObject songRequestObject)
        {
            string requestUrl = Uri.EscapeUriString(string.Format(
                "{0}/search/get?s={1}+{2}&type=1&offset=0&sub=false&limit=25",
                this._baseUrl, songRequestObject.GetArtistsSplit(), songRequestObject.SongName));

            this._debugger.Write("Full track search URL: " + requestUrl, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestUrl);
            ResponseData responseData = await request.GetResponseAsync();

            this._debugger.Write(responseData.GetContentAsString(), DebugType.DEBUG);

            if (responseData.StatusCode == HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<NetEaseSearchResponse>(
                    responseData.GetContentAsString());
            }

            return null;
        }

        private async Task<NetEaseLyricResponse> GetLyricsFromEndpoint(int songId)
        {
            string requestURL = Uri.EscapeUriString(string.Format("{0}/song/lyric?tv=-1&kv=-1&lv=-1&os=pc&id={1}", this._baseUrl, songId));

            this._debugger.Write("Full lyric fetch URL: " + requestURL, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestURL);
            ResponseData responseData = await request.GetResponseAsync();

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

        public int ProviderQuality()
        {
            return 10;
        }
    }
}
