using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Handler.Song.SongProvider
{
    class SongProviderChooser
    {
        public SongProviderChooser() {}

        public EnumSongProvider GetSongProvider()
        {
            return EnumSongProvider.TIDAL;
        } 
    }
}
