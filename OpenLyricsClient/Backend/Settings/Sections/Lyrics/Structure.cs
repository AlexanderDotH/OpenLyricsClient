using Newtonsoft.Json;
using OpenLyricsClient.Backend.Structure.Enum;

namespace OpenLyricsClient.Backend.Settings.Sections.Lyrics;

public class Structure
{
    [JsonProperty("Selection")]
    public EnumLyricsDisplayMode Selection { get; set; }
}