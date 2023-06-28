using Newtonsoft.Json;

namespace OpenLyricsClient.Logic.Debugger.Structure;

public class DebugFile
{
    [JsonProperty("DebugLog")]
    public List<DebugEntry> Entries { get; set; }
}