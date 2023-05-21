using System;
using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Backend.Events.EventArgs;

public class LyricsFoundEventArgs
{
    private LyricData _lyricData;

    public LyricsFoundEventArgs(LyricData lyricData)
    {
        this._lyricData = lyricData;
    }

    public LyricData LyricData
    {
        get => _lyricData;
        set => _lyricData = value;
    }
}