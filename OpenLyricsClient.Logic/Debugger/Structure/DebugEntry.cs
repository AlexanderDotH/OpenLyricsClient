using Newtonsoft.Json;

namespace OpenLyricsClient.Logic.Debugger.Structure;

public class DebugEntry
{
    [JsonProperty("Timestamp")]
    public DateTimeOffset Timestamp { get; set; }
    
    [JsonProperty("Type")]
    public DebugType Type { get; set; }
    
    [JsonProperty("Message")]
    public string Message { get; set; }
    
    [JsonProperty("Component")]
    public string Component { get; set; }
}