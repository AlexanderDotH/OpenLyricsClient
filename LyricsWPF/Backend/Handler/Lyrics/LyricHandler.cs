using System;
using System.Threading.Tasks;
using LyricsWPF.Backend.Collector;
using LyricsWPF.Backend.Events.EventArgs;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Handler.Lyrics
{

    // TODOO:
    // Lyrics collector

    class LyricHandler : IHandler
    {
        private LyricCollector _lyricCollector;
        private LyricData _lyricData;

        private NewSongHandler _songHandler;

        private Task _manageLyricsTask;

        private bool _disposed;

        public LyricHandler(NewSongHandler songHandler)
        {
            this._songHandler = songHandler;
            songHandler.SongChanged += OnSongChanged;

            this._lyricCollector = new LyricCollector();

            this._disposed = false;
        }

        public void OnSongChanged(Object sender, SongChangedEventArgs songChangedEventArgs)
        {
        }

        private async Task CollectLyrics(SongRequestObject songRequestObject)
        {
            if (DataValidator.ValidateData(this._songHandler.CurrentSong) &&
                DataValidator.ValidateData(this._songHandler.CurrentSong.Title,
                    this._songHandler.CurrentSong.Artists, this._songHandler.CurrentSong.MaxTime))
            {
                await GetLyrics(songRequestObject);
            }
        }

        // ALle provider sollen nach nen song suchen und am ende wird verglichen, welcher song am besten ist, basierend an der provider qualität
        public async Task GetLyrics(SongRequestObject songRequestObject)
        {
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
