using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchFetchResponseMessageBody
    {
        [JsonProperty("macro_calls")]
        public MusixMatchMacroCalls MacroCalls { get; set; }
    }
}