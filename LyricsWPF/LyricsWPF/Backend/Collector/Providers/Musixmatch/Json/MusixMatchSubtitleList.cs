using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchSubtitleList
    {
        [JsonProperty("subtitle")]
        public MusixMatchSubtitle Subtitle { get; set; }
    }
}