using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
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
