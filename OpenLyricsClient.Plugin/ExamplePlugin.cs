using DevBase.Format.Structure;
using DevBase.Generics;
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
            AList<LyricElement> lyrics = new AList<LyricElement>();
            lyrics.Add(new LyricElement(533, "Here's my line"));
            lyrics.Add(new LyricElement(536, "Here's another"));
            lyrics.Add(new LyricElement(573, "Here's my line"));
            lyrics.Add(new LyricElement(83, "Here's another"));
            lyrics.Add(new LyricElement(5563, "Here's my line"));
            lyrics.Add(new LyricElement(333, "Here's another"));
            lyrics.Add(new LyricElement(43, "Here's another"));
            lyrics.Add(new LyricElement(5323, "Here's another"));
            lyrics.Add(new LyricElement(6333, "Here's another"));
            lyrics.Add(new LyricElement(7453, "Here's another"));
            lyrics.Add(new LyricElement(84583, "Here's another"));
            lyrics.Add(new LyricElement(45743, "Here's another"));
            return await LyricData.ConvertToData(lyrics, songResponseObject.SongRequestObject.Song.SongMetadata, "Plugin");
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