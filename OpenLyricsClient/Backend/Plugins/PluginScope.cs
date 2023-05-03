using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Plugins
{
    [Flags]
    public enum PluginScope
    {
        Dummy = 1,
        LyricsPostprocess = 2,
        LyricsViewRendering = 4
    }
}
