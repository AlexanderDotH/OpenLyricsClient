using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchAryGenres
    {
        [JsonProperty("music_genre_list")]
        public MusixMatchMusicGenreList[] MusicGenreList { get; set; }
    }
}