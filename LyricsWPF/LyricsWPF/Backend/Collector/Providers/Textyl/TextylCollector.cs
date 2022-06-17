using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.Web;
using DevBase.Web.RequestData;
using DevBase.Web.ResponseData;
using DevBaseFormat.Structure;
using LyricsWPF.Backend.Collector.Providers.Textyl.Json;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Collector.Providers.Textyl
{
    public class TextylCollector : ICollector
    {
        private string _baseUrl;
        private Debugger<TextylCollector> _debugger;

        public TextylCollector()
        {
            this._debugger = new Debugger<TextylCollector>(this);
            
            this._baseUrl = "https://api.textyl.co/api";
        }

        public async Task<LyricData> GetLyrics(SongRequestObject songRequestObject)
        {
            TextylLyricReponse[] lyrics = await FetchLyrics(songRequestObject);

            if (lyrics == null)
                return new LyricData(LyricReturnCode.Failed, SongMetadata.ToSongMetadata(songRequestObject));

            GenericList<LyricElement> lyricElements = new GenericList<LyricElement>();

            for (int i = 0; i < lyrics.Length; i++)
            {
                TextylLyricReponse l = lyrics[i];
                lyricElements.Add(new LyricElement(l.Seconds * 1000, l.Lyrics));
            }

            return await LyricData.ConvertToData(lyricElements, SongMetadata.ToSongMetadata(songRequestObject),
                CollectorName());
        }

        public async Task<TextylLyricReponse[]> FetchLyrics(SongRequestObject songRequestObject)
        {
            string requestUrl = Uri.EscapeUriString(string.Format("{0}/lyrics?q={1}", this._baseUrl,
                songRequestObject.FormattedSongName));

            this._debugger.Write("Textyl request: " + requestUrl, DebugType.INFO);

            RequestData requestData = new RequestData(requestUrl);
            Request request = new Request(requestData);
            ResponseData response = await request.GetResponseAsync();

            if (response.GetContentAsString().Contains("No lyrics available"))
                return null;

            return new JsonDeserializer<TextylLyricReponse[]>().Deserialize(response.GetContentAsString());
        }

        public string CollectorName()
        {
            return "Textyl";
        }

        public int ProviderQuality()
        {
            return (Core.INSTANCE.SettingManager.Settings.LyricSelectionMode == SelectionMode.PERFORMANCE ? 2 : 1);
        }
    }
}
