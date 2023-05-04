using OpenLyricsClient.Shared.Structure.Enum;

namespace OpenLyricsClient.Shared.Structure;

public class LyricsDisplayPreferences
{
    public EnumLyricsDisplayMode DisplayMode { get; set; }
    public bool ArtworkBackground { get; set; }
    public bool LyricsBlur { get; set; }
}