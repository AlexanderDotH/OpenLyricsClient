using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchUserblobGetMessage
    {
        [JsonProperty("header")]
        public MusixMatchUserblobGetMessageHeader Header { get; set; }
    }
}