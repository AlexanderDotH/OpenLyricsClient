using Avalonia.Media;
using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json;

public class JsonArtwork
{
    [JsonProperty("Artwork")]
    public string Artwork { get; set; }
    [JsonProperty("Color")]
    public Color ArtworkColor { get; set; }
}