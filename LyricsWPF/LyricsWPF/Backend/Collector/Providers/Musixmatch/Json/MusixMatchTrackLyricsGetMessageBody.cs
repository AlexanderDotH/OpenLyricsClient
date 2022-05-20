using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGetMessageBody
    {
        [JsonProperty("lyrics")]
        public MusixMatchLyrics Lyrics { get; set; }
    }
}