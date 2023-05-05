using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchMusicGenreList
    {
        [JsonProperty("music_genre")]
        public MusixMatchMusicGenre MusicGenre { get; set; }
    }
}