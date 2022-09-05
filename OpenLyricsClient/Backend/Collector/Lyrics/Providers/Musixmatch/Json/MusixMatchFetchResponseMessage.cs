using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchFetchResponseMessage
    {
        [JsonProperty("header")]
        public MusixMatchFetchResponseMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchFetchResponseMessageBody Body { get; set; }
    }
}