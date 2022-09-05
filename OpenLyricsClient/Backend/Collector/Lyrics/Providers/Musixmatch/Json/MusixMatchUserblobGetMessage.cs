using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchUserblobGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchUserblobGetMessageHeader Header { get; set; }
    }
}