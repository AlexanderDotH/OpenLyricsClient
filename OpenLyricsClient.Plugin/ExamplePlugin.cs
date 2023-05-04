using OpenLyricsClient.Shared.Plugin;
using OpenLyricsClient.Shared.Structure.Artwork;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Plugin
{
    public class ExamplePlugin : IPlugin
    {
        public PluginScope Scope => PluginScope.LyricsCollector;

        public Task<Artwork?> CollectArtwork(SongResponseObject songResponseObject)
        {
            throw new NotImplementedException();
        }

        public async Task<LyricData?> CollectLyrics(SongResponseObject songResponseObject)
        {
            return new LyricData()
            {
                LyricParts = new LyricPart[] { new LyricPart(0, "PLUGINS ARE COMING") },
                LyricReturnCode = LyricReturnCode.SUCCESS
            };
        }

        public Task<SongResponseObject?> CollectSong(SongRequestObject songRequestObject)
        {
            throw new NotImplementedException();
        }

        public int GetCollectedArtworkQuality()
        {
            throw new NotImplementedException();
        }

        public int GetCollectedLyricsQuality()
        {
            return 1337;
        }

        public int GetCollectedSongQuality()
        {
            throw new NotImplementedException();
        }

        public Task<LyricData> ProcessLyrics(SongResponseObject songResponse, LyricData lyrics)
        {
            throw new NotImplementedException();
        }
    }
}