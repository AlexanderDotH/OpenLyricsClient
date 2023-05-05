using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchFetchResponse
    {
        [JsonProperty("message")]
        public MusixMatchFetchResponseMessage Message { get; set; }
    }
}
