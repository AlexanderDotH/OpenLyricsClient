using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure
{
    public class MusixMatchToken
    {
        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("ExpirationDate")]
        public long ExpirationDate { get; set; }
        
        [JsonProperty("Usage")]
        public short Usage { get; set; }

        public static MusixMatchToken ToToken(string token, long expiresIn)
        {
            MusixMatchToken musixMatchToken = new MusixMatchToken();
            musixMatchToken.Token = token;
            musixMatchToken.ExpirationDate = expiresIn;
            musixMatchToken.Usage = 5;
            return musixMatchToken;
        }
    }
}
