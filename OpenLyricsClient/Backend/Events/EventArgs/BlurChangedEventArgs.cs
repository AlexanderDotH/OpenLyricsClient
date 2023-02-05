using System;
using OpenLyricsClient.Backend.Structure.Lyrics;

namespace OpenLyricsClient.Backend.Events.EventArgs;

public class BlurChangedEventArgs
{
    private float _blurSigma;
    private LyricPart _lyricPart;

    public BlurChangedEventArgs(float blurSigma, LyricPart lyricPart)
    {
        _blurSigma = blurSigma;
        _lyricPart = lyricPart;
    }

    public float BlurSigma
    {
        get => _blurSigma;
        set => _blurSigma = value;
    }

    public LyricPart LyricPart
    {
        get => _lyricPart;
        set => _lyricPart = value;
    }
}
