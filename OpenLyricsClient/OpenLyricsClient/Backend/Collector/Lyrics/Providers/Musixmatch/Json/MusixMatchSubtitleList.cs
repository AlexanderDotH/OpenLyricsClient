using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchSubtitleList
    {
        [JsonProperty("subtitle")]
        public MusixMatchSubtitle Subtitle { get; set; }
    }
}