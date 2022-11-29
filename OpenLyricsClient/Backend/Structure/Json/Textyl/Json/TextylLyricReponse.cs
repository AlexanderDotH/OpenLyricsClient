using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Textyl.Json
{
    public class TextylLyricReponse
    {
        [JsonProperty("seconds")]
        public int Seconds { get; set; }

        [JsonProperty("lyrics")]
        public string Lyrics { get; set; }
    }
}
