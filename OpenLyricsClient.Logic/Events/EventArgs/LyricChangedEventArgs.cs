using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Logic.Events.EventArgs;

public class LyricChangedEventArgs : System.EventArgs
{
    private LyricPart _lyricPart;

    public LyricChangedEventArgs(LyricPart lyricPart)
    {
        _lyricPart = lyricPart;
    }

    public LyricPart LyricPart
    {
        get => _lyricPart;
        set => _lyricPart = value;
    }
}