using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevBase.Format;
using DevBase.Format.Formats.LrcFormat;
using DevBase.Format.Structure;
using DevBase.Generics;
using DevBase.Typography;
using DevBase.Utilities;
using DevBase.Web;
using DevBase.Web.ResponseData;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Shared.Structure;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Json.NetEase.Json;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEase
{
    class NetEaseCollector : ILyricsCollector
    {
        private readonly string _baseUrl;

        private Debugger<NetEaseCollector> _debugger;

        private const int RETRIES = 5;
        private const double RETRY_DURATION_MULTIPLIER = 20f;

        public NetEaseCollector()
        {
            this._baseUrl = "https://music.163.com/api";

            _debugger = new Debugger<NetEaseCollector>(this);
        }

        public async Task<LyricData> GetLyrics(SongResponseObject songResponseObject)
        {
            if (!(DataValidator.ValidateData(songResponseObject)))
                return new LyricData();

            if (!songResponseObject.CollectorName.Equals(this.CollectorName()))
                return new LyricData();
            
            if (!(DataValidator.ValidateData(songResponseObject.Track)))
                return new LyricData();

            if (!(songResponseObject.Track is AList<NetEaseSongResponse>))
                return new LyricData();
            
            AList<NetEaseSongResponse> songResponses = (AList<NetEaseSongResponse>)songResponseObject.Track;

            AList<Tuple<NetEaseSongResponse, NetEaseLyricResponse>> lyrics =
                new AList<Tuple<NetEaseSongResponse, NetEaseLyricResponse>>();
            
            for (int i = 0; i < songResponses.Length; i++)
            {
                NetEaseSongResponse songResponse = songResponses.Get(i);
                
                SongMetadata songMetadata = SongMetadata.ToSongMetadata(
                    songResponse.Name,
                    songResponse.NetEaseAlbumResponse.Name, 
                    DataConverter.ToArtists(songResponse.Artists),
                    songResponse.Duration);

                int songId = songResponse.Id;
                NetEaseLyricResponse lyricResponse = await GetLyricsFromEndpoint(songId);

                if (DataValidator.ValidateData(lyricResponse) &&
                    DataValidator.ValidateData(lyricResponse.Code,
                        lyricResponse.NetEaseLrcResponse))
                {

                    lyrics.Add(new Tuple<NetEaseSongResponse, NetEaseLyricResponse>(songResponse, lyricResponse));

                    for (int k = 0; k < lyrics.Length; k++)
                    {
                        Tuple<NetEaseSongResponse, NetEaseLyricResponse> lyricElement = lyrics.Get(k);
                        if (!IsGarbage(lyricElement.Item2))
                        {
                            this._debugger.Write("Fetched lyrics for " + songMetadata.Name + "!", DebugType.INFO);
                            return await ParseLyricResponse(lyricElement.Item2, songMetadata);
                        }
                    }
                }
            }

            this._debugger.Write("Could not find lyrics for " + songResponseObject.SongRequestObject.SongName + "!", DebugType.ERROR);
            return new LyricData();
        }

        private bool IsGarbage(NetEaseLyricResponse lyrics)
        {
            AString values = new AString(lyrics.NetEaseLrcResponse.Lyric);

            AList<string> lines = values.AsList();

            for (int i = 0; i < lines.Length; i++)
            {
                string element = lines.Get(i);

                if (!Regex.IsMatch(element, 
                        DevBase.Format.Structure.RegexHolder.REGEX_GARBAGE))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<LyricData> ParseLyricResponse(NetEaseLyricResponse lyricResponse, SongMetadata songMetadata)
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
                            AList<LyricElement> lyricElements =
                                fileFormatParser.FormatFromString(lyricResponse.NetEaseLrcResponse.Lyric).Lyrics;

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

        private async Task<NetEaseLyricResponse> GetLyricsFromEndpoint(int songId)
        {
            string requestURL = Uri.EscapeUriString(string.Format("{0}/song/lyric?tv=-1&kv=-1&lv=-1&os=pc&id={1}", this._baseUrl, songId));

            //this._debugger.Write("Full lyric fetch URL: " + requestURL, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestURL);
            ResponseData responseData = await request.GetResponseAsync();
            
            if (responseData.StatusCode == HttpStatusCode.OK)
            {
                return new JsonDeserializer().Deserialize<NetEaseLyricResponse>(responseData.GetContentAsString());
            }

            return null;
        }

        public string CollectorName()
        {
            return "NetEase";
        }

        public int ProviderQuality()
        {
            return 4;
        }
    }
}
