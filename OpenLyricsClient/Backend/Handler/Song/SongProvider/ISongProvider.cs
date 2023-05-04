using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Handler.Song.SongProvider
{
    public interface ISongProvider
    {
        Shared.Structure.Song.Song GetCurrentSong();
        EnumSongProvider GetEnum();
        Task<Shared.Structure.Song.Song> UpdateCurrentPlaybackTrack();
        void Dispose();
    }
}
