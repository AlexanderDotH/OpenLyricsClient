using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGet
    {
        [JsonProperty("message")]
        public MusixMatchTrackLyricsGetMessage Message { get; set; }
    }
}