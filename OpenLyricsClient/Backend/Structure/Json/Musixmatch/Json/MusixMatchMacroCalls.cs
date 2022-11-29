using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Structure.Json.Musixmatch.Json
{
    public class MusixMatchMacroCalls
    {
        [JsonProperty("track.lyrics.get")]
        public MusixMatchTrackLyricsGet TrackLyricsGet { get; set; }

        [JsonProperty("track.snippet.get")]
        public MusixMatchTrackSnippetGet TrackSnippetGet { get; set; }

        [JsonProperty("track.subtitles.get")]
        public MusixMatchTrackSubtitlesGet TrackSubtitlesGet { get; set; }

        [JsonProperty("userblob.get")]
        public MusixMatchUserblobGet UserblobGet { get; set; }

        [JsonProperty("matcher.track.get")]
        public MusixMatchMatcherTrackGet MatcherTrackGet { get; set; }
    }
}