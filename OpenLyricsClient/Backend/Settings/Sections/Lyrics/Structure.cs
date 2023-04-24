using Newtonsoft.Json;
using OpenLyricsClient.Backend.Structure.Enum;

namespace OpenLyricsClient.Backend.Settings.Sections.Lyrics;

public class Structure
{
    [JsonProperty("Selection Mode")] 
    public EnumLyricsDisplayMode Selection { get; set; }
    
    [JsonProperty("Artwork Background")] 
    public bool ArtworkBackground { get; set; }
    
    [JsonProperty("Blur Lyrics")] 
    public bool LyricsBlur { get; set; }
}