using System;

namespace OpenLyricsClient.Backend.Plugins
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
