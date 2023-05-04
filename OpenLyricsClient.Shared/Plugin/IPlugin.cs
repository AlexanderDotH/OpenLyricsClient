using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLyricsClient.Shared.Plugin
{
    public interface IPlugin
    {
        PluginScope Scope { get; }
        Task<LyricData> ProcessLyrics(SongResponseObject songResponse, LyricData lyrics);
        Task<LyricData?> CollectLyrics(SongResponseObject songResponseObject);
        int GetCollectedLyricsQuality();
        Task<SongResponseObject?> CollectSong(SongRequestObject songRequestObject);
        int GetCollectedSongQuality();
        Task<Structure.Artwork.Artwork?> CollectArtwork(SongResponseObject songResponseObject);
        int GetCollectedArtworkQuality();
    }
}
