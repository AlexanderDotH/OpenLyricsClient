using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchFetchResponseMessageHeader
    {
        [JsonProperty("status_code")]
        public long StatusCode { get; set; }

        [JsonProperty("execute_time")]
        public double ExecuteTime { get; set; }

        [JsonProperty("pid")]
        public long Pid { get; set; }

        [JsonProperty("surrogate_key_list")]
        public object[] SurrogateKeyList { get; set; }
    }
}