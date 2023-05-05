using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchSubtitleList
    {
        [JsonProperty("subtitle")]
        public MusixMatchSubtitle Subtitle { get; set; }
    }
}