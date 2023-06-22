using Avalonia.Media;
using Newtonsoft.Json;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Visual;

namespace OpenLyricsClient.Logic.Settings.Sections.Lyrics;

public class Structure
{
    [JsonProperty("Selection Mode")] 
    public EnumLyricsDisplayMode Selection { get; set; }
    
    [JsonProperty("Lyrics Size")] 
    public double LyricsSize { get; set; }
    
    [JsonProperty("Lyrics Weight")] 
    public FontWeight LyricsWeight { get; set; }
    
    [JsonProperty("Lyrics Alignment")] 
    public TextAlignment LyricsAlignment { get; set; }
    
    [JsonProperty("Lyrics Margin")] 
    public Margin LyricsMargin { get; set; }
    
    [JsonProperty("Artwork Background")] 
    public bool ArtworkBackground { get; set; }
    
    [JsonProperty("Blur Lyrics")] 
    public bool LyricsBlur { get; set; }
}