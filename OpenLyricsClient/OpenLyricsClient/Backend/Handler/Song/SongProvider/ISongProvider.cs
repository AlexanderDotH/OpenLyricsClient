using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Handler.Song.SongProvider
{
    interface ISongProvider
    {
        Structure.Song.Song GetCurrentSong();
        EnumSongProvider GetEnum();
        Task<Structure.Song.Song> UpdateCurrentPlaybackTrack();
        void Dispose();
    }
}
