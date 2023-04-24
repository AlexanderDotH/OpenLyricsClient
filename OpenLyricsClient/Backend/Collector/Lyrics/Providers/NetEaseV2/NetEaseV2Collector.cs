using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevBase.Format;
using DevBase.Format.Formats.LrcFormat;
using DevBase.Format.Structure;
using DevBase.Generics;
using DevBase.Typography;
using DevBase.Web;
using DevBase.Web.ResponseData;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Json.NetEaseV2.Json;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEaseV2
{
    class NetEaseV2Collector : ILyricsCollector
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

        public async Task<LyricData> GetLyrics(SongResponseObject songResponseObject)
        {
            if (!(DataValidator.ValidateData(songResponseObject)))
                return new LyricData();

            if (!songResponseObject.CollectorName.Equals(this.CollectorName()))
                return new LyricData();

            if (!(songResponseObject.Track is AList<NetEaseV2SongResponse>))
                return new LyricData();

            AList<NetEaseV2SongResponse> response = (AList<NetEaseV2SongResponse>)songResponseObject.Track;

            AList<Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse>> lyrics = 
                new AList<Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse>>();

            for (int j = 0; j < response.Length; j++)
            {
                NetEaseV2SongResponse songResponse = response[j];

                SongMetadata songMetadata = SongMetadata.ToSongMetadata(
                    songResponse.Name,
                    songResponse.Album.Name,
                    DataConverter.ToArtists(songResponse.Artists),
                    songResponse.Duration);

                int songId = songResponse.Id;
                
                NetEaseV2LyricResponse lyricResponse = await GetLyricsFromEndpoint(songId);

                lyrics.Add(new Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse>(songResponse, lyricResponse));

                for (int k = 0; k < lyrics.Length; k++)
                {
                    Tuple<NetEaseV2SongResponse, NetEaseV2LyricResponse> lyricElement = lyrics.Get(k);
                    if (!IsGarbage(lyricElement.Item2))
                    {
                        this._debugger.Write("Fetched lyrics for " + songMetadata.Name + "!", DebugType.INFO);
                        return await ParseLyricResponse(lyricElement.Item2, songMetadata);
                    }
                }
            }
            
            this._debugger.Write("Could not find lyrics for " + songResponseObject.SongRequestObject.SongName + "!", DebugType.ERROR);
            return new LyricData();
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
                            AList<LyricElement> lyricElements =
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
            AString values = new AString(lyrics.Lrc.Lyric);

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

        private async Task<NetEaseV2LyricResponse> GetLyricsFromEndpoint(int songId)
        {
            string requestURL = Uri.EscapeUriString(string.Format("{0}/lyric?id={1}", this._baseUrl, songId));

            //this._debugger.Write("Full lyric fetch URL: " + requestURL, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestURL);
            ResponseData responseData = await request.GetResponseAsync();

            //this._debugger.Write(responseData.GetContentAsString(), DebugType.DEBUG);

            if (responseData.StatusCode == HttpStatusCode.OK)
            {
                return new JsonDeserializer().Deserialize<NetEaseV2LyricResponse>(responseData.GetContentAsString());
            }

            return null;
        }
        
        public string CollectorName()
        {
            return "NetEaseV2";
        }

        public int ProviderQuality()
        {
            return 5;

        }
    }
}
