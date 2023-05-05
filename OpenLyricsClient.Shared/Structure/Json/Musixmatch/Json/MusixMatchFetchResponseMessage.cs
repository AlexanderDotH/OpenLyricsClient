using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchFetchResponseMessage
    {
        [JsonProperty("header")]
        public MusixMatchFetchResponseMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchFetchResponseMessageBody Body { get; set; }
    }
}