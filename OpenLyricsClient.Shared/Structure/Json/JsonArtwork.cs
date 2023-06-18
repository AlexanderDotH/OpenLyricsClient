using Newtonsoft.Json;
using OpenLyricsClient.Shared.Structure.Palette;

namespace OpenLyricsClient.Shared.Structure.Json;

public class JsonArtwork
{
    [JsonProperty("Color")]
    public ColorPalette ArtworkColor { get; set; }
    
    [JsonProperty("Calculated")]
    public bool ArtworkCalculated { get; set; }

    [JsonProperty("FilePath")]
    public string FilePath { get; set; }
}