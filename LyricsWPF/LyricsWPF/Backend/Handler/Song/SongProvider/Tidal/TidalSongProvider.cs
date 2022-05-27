using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Events.EventArgs;

namespace LyricsWPF.Backend.Handler.Song.SongProvider.Tidal
{
    class TidalSongProvider : ISongProvider
    {

        private Song song;

        public TidalSongProvider()
        {
            Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;
        }

        private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
        {
            throw new NotImplementedException();
        }

        public Song GetCurrentSong()
        {
            throw new NotImplementedException();
        }

        public EnumSongProvider GetEnum()
        {
            return EnumSongProvider.TIDAL;
        }

        public Task<Song> UpdateCurrentPlaybackTrack()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
