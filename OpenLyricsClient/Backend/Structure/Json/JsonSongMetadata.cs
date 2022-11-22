using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json;

public class JsonSongMetadata
{
    [JsonProperty("Name")]
    public string Name { get; set; }
    
    [JsonProperty("Artists")]
    public string[] Artists { get; set; }

    [JsonProperty("Album")]
    public string Album { get; set; }
    
    [JsonProperty("Duration")]
    public long Duration { get; set; }
}