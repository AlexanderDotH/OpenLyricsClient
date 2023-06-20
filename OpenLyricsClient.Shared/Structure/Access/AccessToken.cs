using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Access;

public class AccessToken
{
    [JsonProperty("AccessToken")]
    public string Access { get; set; }
    
    [JsonProperty("RefreshToken")]
    public string Refresh { get; set; }
}