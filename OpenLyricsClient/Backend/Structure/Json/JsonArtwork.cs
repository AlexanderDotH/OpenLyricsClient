using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json;

public class JsonArtwork
{
    [JsonProperty("Artwork")]
    public string Artwork { get; set; }
}