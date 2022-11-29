using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchMusicGenreList
    {
        [JsonProperty("music_genre")]
        public MusixMatchMusicGenre MusicGenre { get; set; }
    }
}