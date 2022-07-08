using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Handler.Song.SongProvider
{
    interface ISongProvider
    {
        Song GetCurrentSong();
        EnumSongProvider GetEnum();
        Task<Song> UpdateCurrentPlaybackTrack();
        void Dispose();
    }
}
