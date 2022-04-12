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

        public async Task GetLyrics(SongRequestObject songRequestObject)
        {
            this._lyricCollector = new LyricCollector();
            this._lyricData = this._lyricCollector.CollectLyrics(songRequestObject, "NetEase");
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
