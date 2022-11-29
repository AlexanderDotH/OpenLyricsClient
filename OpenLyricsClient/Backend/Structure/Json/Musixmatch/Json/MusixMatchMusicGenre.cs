using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchMusicGenre
    {
        [JsonProperty("music_genre_id")]
        public long MusicGenreId { get; set; }

        [JsonProperty("music_genre_parent_id")]
        public long MusicGenreParentId { get; set; }

        [JsonProperty("music_genre_name")]
        public string MusicGenreName { get; set; }

        [JsonProperty("music_genre_name_extended")]
        public string MusicGenreNameExtended { get; set; }

        [JsonProperty("music_genre_vanity")]
        public string MusicGenreVanity { get; set; }
    }
}