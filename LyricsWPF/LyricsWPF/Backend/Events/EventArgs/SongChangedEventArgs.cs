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
        private EventType _eventType;

        public SongChangedEventArgs(Song song, EventType eventType)
        {
            this._song = song;
            this._eventType = eventType;
        }

        public EventType EventType
        {
            get { return _eventType; }
        }

        public Song Song
        {
            get { return this._song; }
        }
    }
}
