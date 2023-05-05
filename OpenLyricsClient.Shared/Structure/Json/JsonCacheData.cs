using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json;

public class JsonCacheData
{
    [JsonProperty("Song")]
    public JsonSongMetadata SongMetadata { get; set; }

    [JsonProperty("Lyrics")]
    public JsonLyricData LyricData { get; set; }
    
    [JsonProperty("Artwork")]
    public JsonArtwork Artwork { get; set; }
}