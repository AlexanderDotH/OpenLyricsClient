using Avalonia.Media;
using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json;

public class JsonArtwork
{
    [JsonProperty("Color")]
    public Color ArtworkColor { get; set; }
}