using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using DevBase.Generic;
using DevBase.Typography;
using DevBase.Web;
using DevBase.Web.ResponseData;
using DevBaseFormat;
using DevBaseFormat.Formats.LrcFormat;
using DevBaseFormat.Structure;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEase.Json;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEaseV2.Json;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEaseV2
{
    class NetEaseV2Collector : ICollector
    {
        private readonly string _baseUrl;

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

                            if (response.Result.Songs.Length > 0)
                            {
                                GenericList<Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse>> lyrics = 
                                    new GenericList<Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse>>();

                                double retryPercentage = 5;

                                for (int i = 0; i < RETRIES; i++)
                                {
                                    for (int j = 0; j < response.Result.Songs.Length; j++)
                                    {
                                        NetEaseV2SongResponse songResponse = response.Result.Songs[j];

                                        SongMetadata songMetadata = SongMetadata.ToSongMetadata(
                                            songResponse.Name,
                                            songResponse.Album.Name,
                                            DataConverter.ToArtists(songResponse.Artists),
                                            songResponse.Duration);

                                        if (Core.INSTANCE.CacheManager.IsInCache(songRequestObject))
                                            break;

                                        if (IsValidSong(songResponse, songRequestObject, retryPercentage))
                                        {
                                            int songId = songResponse.Id;
                                            NetEaseV2LyricResponse lyricResponse = await GetLyricsFromEndpoint(songId);

                                            if (songRequestObject.SelectioMode == SelectionMode.QUALITY)
                                            {
                                                lyrics.Add(new Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse>(songResponse, lyricResponse));

                                                for (int k = 0; k < lyrics.Length; k++)
                                                {
                                                    Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse> lyricElement = lyrics.Get(i);
                                                    if (lyricElement.Item2.Lrc.Lyric != "")
                                                    {
                                                        if (!IsGarbage(lyricElement.Item2))
                                                            continue;

                                                        return await ParseLyricResponse(lyricElement.Item2, songMetadata);
                                                    }
                                                }

                                            }
                                            else if (songRequestObject.SelectioMode == SelectionMode.PERFORMANCE)
                                            {
                                                return await ParseLyricResponse(lyricResponse, songMetadata);
                                            }
                                        }

                                        retryPercentage += (int)Math.Ceiling(i * RETRY_DURATION_MULTIPLIER);
                                    }

                                    return new LyricData();
                                }

                                return new LyricData();
                            }
                            else
                            {
                                return new LyricData();
                            }
                        }
                    }
                }

                return new LyricData();
            }

            return new LyricData();
        }

        private bool IsValidSong(NetEaseV2SongResponse songResponse, SongRequestObject songRequestObject, double percentage)
        {
            if (!DataValidator.ValidateData(songResponse) || 
                  !DataValidator.ValidateData(songRequestObject))
                return false;

            if (IsSimilar(songRequestObject.FormattedSongName, songResponse.Name) != IsSimilar(songRequestObject.FormattedSongAlbum, songResponse.Album.Name))
            {
                if (!IsSimilar(songRequestObject.FormattedSongAlbum, songResponse.Album.Name))
                    return false;
            }

            if (!MatchDuration(songResponse, songRequestObject.SongDuration, percentage))
                return false;

            if (!MatchArtists(songResponse, songRequestObject.Artists, 70))
                return false;

            if (!IsSimilar(songRequestObject.FormattedSongName, songResponse.Name))
                return false;

            return true;
        }

        //Untested it should make everything a bit more strict
        private bool IsSimilar(string string1, string string2)
        {
            return MathUtils.CalculateLevenshteinDistance(string1, string2) >=
                   Math.Abs(string1.Length - string2.Length);
        }

        private async Task<LyricData> ParseLyricResponse(NetEaseV2LyricResponse lyricResponse, SongMetadata songMetadata)
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
                                fileFormatParser.FormatFromString(lyricResponse.Lrc.Lyric).Lyrics;

                            if (DataValidator.ValidateData(lyricElements))
                            {
                                return await LyricData.ConvertToData(lyricElements, songMetadata, this.CollectorName());
                            }
                        }
                    }
                }
            }

            return new LyricData();
        }

        private bool IsGarbage(NetEaseV2LyricResponse lyrics)
        {
            AString value = new AString(lyrics.Lrc.Lyric);

            GenericList<string> lines = value.AsList();

            int checksConfirmed = 0;

            if (lines.Length < 2)
                return false;

            if (Regex.IsMatch(lines.Get(0), DevBaseFormat.Structure.RegexHolder.REGEX_GARBAGE))
                checksConfirmed++;

            int lastValid = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string s = lines.Get(i);

                if (Regex.IsMatch(s, DevBaseFormat.Structure.RegexHolder.REGEX_GARBAGE))
                    continue;

                lastValid = i;
            }

            if (lastValid >= lines.Length)
                return false;

            if (!lines.Get(lastValid).Equals(string.Empty))
                checksConfirmed++;

            return checksConfirmed == 2;
        }

        private async Task<NetEaseV2SearchResponse> SearchTrack(SongRequestObject songRequestObject)
        {
            string requestUrl = Uri.EscapeUriString(string.Format("{0}/search?limit=100&type=1&keywords={2}",
                this._baseUrl,
                songRequestObject.GetArtistsSplit(), songRequestObject.FormattedSongName));

            this._debugger.Write("Full track search URL: " + requestUrl, DebugType.DEBUG);
            DevBase.Web.Request request = new Request(requestUrl);
            ResponseData responseData = await request.GetResponseAsync();

            if (responseData.StatusCode == HttpStatusCode.OK)
            { 
                return new JsonDeserializer<NetEaseV2SearchResponse>().Deserialize(responseData.GetContentAsString());
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
                return new JsonDeserializer<NetEaseV2LyricResponse>().Deserialize(responseData.GetContentAsString());
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
                for (int i = 0; i < netEaseSongResponse.Artists.Length; i++)
                {
                    for (int j = 0; j < artists.Length; j++)
                    {
                        string artist = SongFormatter.FormatString(netEaseSongResponse.Artists[i].Name);

                        if (MathUtils.CalculateLevenshteinDistance(artist, artists[j]) <= Math.Abs(artist.Length - artists[j].Length))
                        {
                            artistsMatch++;
                        } 
                        else if (MathUtils.CalculateLevenshteinDistance(artist, artists[j]) <= 5)
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
                    string artist = SongFormatter.FormatString(netEaseSongResponse.Artists[i].Name);

                    if (MathUtils.CalculateLevenshteinDistance(artist, artists[0]) <=  Math.Abs(artist.Length - artists[0].Length))
                    {
                        artistsMatch++;
                    }
                    else if (MathUtils.CalculateLevenshteinDistance(artist, artists[0]) <= 5)
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
            return (Core.INSTANCE.SettingManager.Settings.LyricSelectionMode == SelectionMode.PERFORMANCE ? 2 : 3);

        }
    }
}
