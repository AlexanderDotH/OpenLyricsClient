using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Logic.Events.EventArgs;

public class LyricsPercentageUpdatedEventArgs
{
    private LyricPart _lyricPart;
    private double _percentage;

    public LyricsPercentageUpdatedEventArgs(LyricPart lyricPart, double percentage)
    {
        this._lyricPart = lyricPart;
        this._percentage = percentage;
    }

    public LyricPart LyricPart
    {
        get => _lyricPart;
        set => _lyricPart = value;
    }

    public double Percentage
    {
        get => this._percentage;
        set => this._percentage = value;
    }
}