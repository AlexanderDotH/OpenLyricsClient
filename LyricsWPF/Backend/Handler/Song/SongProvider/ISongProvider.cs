using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Handler.Song.SongProvider
{
    interface ISongProvider
    {
        Song GetCurrentSong();
        EnumSongProvider GetEnum();

        void Dispose();
    }
}
