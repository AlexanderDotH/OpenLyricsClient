using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchFetchResponseMessage
    {
        [JsonProperty("header")]
        public MusixMatchFetchResponseMessageHeader Header { get; set; }

        [JsonProperty("body")]
        public MusixMatchFetchResponseMessageBody Body { get; set; }
    }
}