using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Textyl.Json
{
    public class TextylLyricReponse
    {
        [JsonProperty("seconds")]
        public int Seconds { get; set; }

        [JsonProperty("lyrics")]
        public string Lyrics { get; set; }
    }
}
