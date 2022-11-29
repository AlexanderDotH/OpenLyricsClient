using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchFetchResponseMessage
    {
        [JsonProperty("header")]
        public MusixMatchFetchResponseMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchFetchResponseMessageBody Body { get; set; }
    }
}