using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch.Json
{
    public class MusixMatchMusicGenreList
    {
        [JsonProperty("music_genre")]
        public MusixMatchMusicGenre MusicGenre { get; set; }
    }
}