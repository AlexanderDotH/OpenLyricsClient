using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGet
    {
        [JsonProperty("message")]
        public MusixMatchTrackLyricsGetMessage Message { get; set; }
    }
}