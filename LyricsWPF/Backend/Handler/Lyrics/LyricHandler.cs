using System.Threading.Tasks;
using LyricsWPF.Backend.Collector;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;

namespace LyricsWPF.Backend.Handler.Lyrics
{
    class LyricHandler : IHandler
    {
        private LyricCollector _lyricCollector;
        private LyricData _lyricData;
        private bool _disposed;

        public LyricHandler()
        {
            this._disposed = false;
        }

        // ALle provider sollen nach nen song suchen und am ende wird verglichen, welcher song am besten ist, basierend an der provider qualität
        public async Task GetLyrics(SongRequestObject songRequestObject)
        {
            this._lyricCollector = new LyricCollector();
            this._lyricData = this._lyricCollector.CollectLyrics(songRequestObject, "NetEaseV2");
        }

        public LyricData FullLyrics
        {
            get { return this._lyricData; }
        }

        public void Dispose()
        {
            this._disposed = true;
        }
    }
}
