using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchFetchResponseMessageBody
    {
        [JsonProperty("macro_calls")]
        public MusixMatchMacroCalls MacroCalls { get; set; }
    }
}