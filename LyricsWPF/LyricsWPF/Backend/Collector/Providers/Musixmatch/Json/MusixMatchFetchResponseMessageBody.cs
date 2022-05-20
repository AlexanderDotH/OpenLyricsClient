using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchFetchResponseMessageBody
    {
        [JsonProperty("macro_calls")]
        public MusixMatchMacroCalls MacroCalls { get; set; }
    }
}