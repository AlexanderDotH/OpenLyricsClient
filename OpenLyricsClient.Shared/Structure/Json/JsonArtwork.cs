using Avalonia.Media;
using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json;

public class JsonArtwork
{
    [JsonProperty("Color")]
    public Color ArtworkColor { get; set; }
    
    [JsonProperty("Applied")]
    public bool ArtworkApplied { get; set; }

}