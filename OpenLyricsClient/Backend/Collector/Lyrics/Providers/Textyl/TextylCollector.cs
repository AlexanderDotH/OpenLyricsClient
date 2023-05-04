using System;
using System.Threading.Tasks;
using DevBase.Format.Structure;
using DevBase.Generics;
using DevBase.Web;
using DevBase.Web.ResponseData;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Json.Textyl.Json;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Textyl
{
    public class TextylCollector : ILyricsCollector
    {
        private readonly string _baseUrl;
        private Debugger<TextylCollector> _debugger;

        public TextylCollector()
        {
            this._debugger = new Debugger<TextylCollector>(this);

            this._baseUrl = "https://api.textyl.co/api";
        }

        public async Task<LyricData> GetLyrics(SongResponseObject songResponseObject)
        {
            TextylLyricReponse[] lyrics = await FetchLyrics(songResponseObject.SongRequestObject);

            if (!DataValidator.ValidateData(lyrics) || lyrics.IsNullOrEmpty())
            {
                /*this._debugger.Write("Could not find lyrics for " + songResponseObject.SongRequestObject.SongName + "!", DebugType.ERROR);*/
                return new LyricData();
            }

            if (lyrics.Length == 0)
                return new LyricData();

            AList<LyricElement> lyricElements = new AList<LyricElement>();

            for (int i = 0; i < lyrics.Length; i++)
            {
                TextylLyricReponse l = lyrics[i];
                lyricElements.Add(new LyricElement(l.Seconds * 1000, l.Lyrics));
            }

            this._debugger.Write("Fetched lyrics for " + songResponseObject.SongRequestObject.SongName + "!", DebugType.INFO);
            
            return await LyricData.ConvertToData(lyricElements, SongMetadata.ToSongMetadata(songResponseObject.SongRequestObject),
                CollectorName());
        }

        public async Task<TextylLyricReponse[]> FetchLyrics(SongRequestObject songRequestObject)
        {
            string requestUrl = Uri.EscapeUriString(string.Format("{0}/lyrics?q={1}", this._baseUrl,
                songRequestObject.FormattedSongName));

            /*this._debugger.Write("Textyl request: " + requestUrl, DebugType.INFO);*/

            try
            {
                Request request = new Request(requestUrl);
                ResponseData response = await request.GetResponseAsync();

                string content = response.GetContentAsString();
                
                if (content.Contains("No lyrics available"))
                    return null;

                TextylLyricReponse[] reponse =
                    new JsonDeserializer().Deserialize<TextylLyricReponse[]>(content);

                if (reponse != null)
                    return reponse;
            }
            catch (Exception e) { }

            return null;
        }

        public string CollectorName()
        {
            return "Textyl";
        }

        public int ProviderQuality()
        {
            return 1;
        }
    }
}
