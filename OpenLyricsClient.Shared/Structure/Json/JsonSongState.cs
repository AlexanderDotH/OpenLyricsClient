using Newtonsoft.Json;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Shared.Structure.Json;

public class JsonSongState
{
    [JsonProperty("State")]
    public SongState SongState { get; set; } 
}