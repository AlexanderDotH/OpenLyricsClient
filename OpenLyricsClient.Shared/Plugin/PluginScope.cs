using System;

namespace OpenLyricsClient.Shared.Plugin
{
    [Flags]
    public enum PluginScope
    {
        Dummy = 1,
        LyricsPostprocess = 2,
        LyricsViewRendering = 4,
        LyricsCollector = 8,
        SongCollector = 16,
        ArtworkCollector = 32
    }
}
