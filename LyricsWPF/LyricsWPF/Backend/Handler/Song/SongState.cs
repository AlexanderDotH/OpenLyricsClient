using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Handler.Song
{
    public enum SongState
    {
        HAS_LYRICS_AVAILABLE, NO_LYRICS_AVAILABLE, SEARCHING_LYRICS, SEARCHING_FINISHED
    }
}
