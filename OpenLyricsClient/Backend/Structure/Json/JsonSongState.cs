
using Newtonsoft.Json;
using OpenLyricsClient.Backend.Structure.Song;

namespace OpenLyricsClient.Backend.Structure.Json;

public class JsonSongState
{
    [JsonProperty("State")]
    public SongState SongState { get; set; } 
}