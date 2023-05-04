using System;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Shared.Structure;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Backend.Events.EventArgs
{
    [Serializable]
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
