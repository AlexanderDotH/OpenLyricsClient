using System;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.Web;
using DevBase.Web.RequestData;
using DevBase.Web.ResponseData;
using DevBaseFormat.Structure;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Json.Textyl.Json;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

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

            if (!DataValidator.ValidateData(lyrics))
                return new LyricData();

            if (lyrics.Length == 0)
                return new LyricData();

            GenericList<LyricElement> lyricElements = new GenericList<LyricElement>();

            for (int i = 0; i < lyrics.Length; i++)
            {
                TextylLyricReponse l = lyrics[i];
                lyricElements.Add(new LyricElement(l.Seconds * 1000, l.Lyrics));
            }

            return await LyricData.ConvertToData(lyricElements, SongMetadata.ToSongMetadata(songResponseObject.SongRequestObject),
                CollectorName());
        }

        public async Task<TextylLyricReponse[]> FetchLyrics(SongRequestObject songRequestObject)
        {
            string requestUrl = Uri.EscapeUriString(string.Format("{0}/lyrics?q={1}", this._baseUrl,
                songRequestObject.FormattedSongName));

            this._debugger.Write("Textyl request: " + requestUrl, DebugType.INFO);

            try
            {
                Request request = new Request(requestUrl);
                ResponseData response = await request.GetResponseAsync();

                if (response.GetContentAsString().Contains("No lyrics available"))
                    return null;

                TextylLyricReponse[] reponse =
                    new JsonDeserializer<TextylLyricReponse[]>().Deserialize(response.GetContentAsString());

                if (reponse != null)
                    return reponse;
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }

            return null;
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
