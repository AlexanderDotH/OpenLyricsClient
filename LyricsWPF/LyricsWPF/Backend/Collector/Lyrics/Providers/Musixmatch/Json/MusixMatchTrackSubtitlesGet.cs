using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchTrackSubtitlesGet
    {
        [JsonProperty("message")]
        public MusixMatchTrackSubtitlesGetMessage Message { get; set; }
    }
}