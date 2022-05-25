using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
using Newtonsoft.Json.Serialization;

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

                                            GenericList<Tuple<NetEaseSongResponse, NetEaseLyricResponse>> lyrics =
                                                new GenericList<Tuple<NetEaseSongResponse, NetEaseLyricResponse>>();

                                            if (SongFormatter.FormatSongAlbum(songResponse.NetEaseAlbumResponse.Name).Equals(SongFormatter.FormatSongAlbum(songRequestObject.FormattedSongAlbum)))
                                            {
                                                if (MatchDuration(songResponse, songRequestObject.SongDuration,
                                                        retryPercentage))
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

                                                        if (songRequestObject.SelectioMode == SelectionMode.QUALITY)
                                                        {
                                                            lyrics.Add(new Tuple<NetEaseSongResponse, NetEaseLyricResponse>(songResponse, lyricResponse));

                                                            for (int k = 0; k < lyrics.Length; k++)
                                                            {
                                                                Tuple<NetEaseSongResponse, NetEaseLyricResponse> lyricElement = lyrics.Get(i);
                                                                if (lyricElement.Item2.NetEaseLrcResponse.Lyric != "")
                                                                {
                                                                    return await ParseLyricResponse(lyricElement.Item2, songResponse.Name);
                                                                }
                                                            }
                                                        }
                                                        else if (songRequestObject.SelectioMode == SelectionMode.PERFORMANCE)
                                                        {
                                                            return await ParseLyricResponse(lyricResponse, songResponse.Name);
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

            return new LyricData(LyricReturnCode.Failed);
        }

        private async Task<LyricData> ParseLyricResponse(NetEaseLyricResponse lyricResponse, string songName)
        {
            if (DataValidator.ValidateData(lyricResponse) &&
                DataValidator.ValidateData(lyricResponse.Code, lyricResponse.NetEaseLrcResponse.Lyric))
            {
                if (lyricResponse.Code == 200)
                {
                    if (lyricResponse.NetEaseLrcResponse.Lyric != null &&
                        lyricResponse.NetEaseLrcResponse.Version != 0)
                    {
                        FileFormatParser<LrcObject> fileFormatParser =
                            new FileFormatParser<LrcObject>(
                                new LrcParser<LrcObject>());

                        if (DataValidator.ValidateData(fileFormatParser))
                        {
                            GenericList<LyricElement> lyricElements =
                                fileFormatParser.FormatFromString(lyricResponse.NetEaseLrcResponse.Lyric).Lyrics;

                            if (DataValidator.ValidateData(lyricElements))
                            {
                                return await LyricData.ConvertToData(lyricElements, songName, this.CollectorName());
                            }
                        }
                    }
                }
            }

            return new LyricData(LyricReturnCode.Failed);
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
                "{0}/search/get?s={2}&type=1&offset=0&sub=false&limit=25",
                this._baseUrl, songRequestObject.GetArtistsSplit(), songRequestObject.SongName));

            this._debugger.Write("Full track search URL: " + requestUrl, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestUrl);
            ResponseData responseData = await request.GetResponseAsync();

            this._debugger.Write(responseData.GetContentAsString(), DebugType.DEBUG);

            if (responseData.StatusCode == HttpStatusCode.OK)
            {
                return new JsonDeserializer<NetEaseSearchResponse>().Deserialize(responseData.GetContentAsString());
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
                return new JsonDeserializer<NetEaseLyricResponse>().Deserialize(responseData.GetContentAsString());
            }

            return null;
        }

        public string CollectorName()
        {
            return "NetEase";
        }

        public int ProviderQuality()
        {
            return (Core.INSTANCE.SettingManager.Settings.LyricSelectionMode == SelectionMode.PERFORMANCE ? 8 : 2);
        }
    }
}
