using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchAryGenres
    {
        [JsonProperty("music_genre_list")]
        public MusixMatchMusicGenreList[] MusicGenreList { get; set; }
    }
}