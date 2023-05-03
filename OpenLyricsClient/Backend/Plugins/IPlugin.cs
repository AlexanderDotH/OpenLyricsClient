using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Plugins
{
    public interface IPlugin
    {
        PluginScope Scope { get; }
        LyricData ProcessLyrics(SongResponseObject songResponse, LyricData lyrics);
    }
}
