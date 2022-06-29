using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchMusicGenreList
    {
        [JsonProperty("music_genre")]
        public MusixMatchMusicGenre MusicGenre { get; set; }
    }
}