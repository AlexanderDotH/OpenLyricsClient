using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Handler.Song;

namespace LyricsWPF.Backend.Events.EventArgs
{
    public class SongChangedEventArgs : System.EventArgs
    {
        private Song _song;

        public SongChangedEventArgs(Song song)
        {
            this._song = song;
        }

        public Song Song
        {
            get { return this._song; }
        }
    }
}
