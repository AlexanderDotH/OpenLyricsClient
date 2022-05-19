using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchTrackSnippetGetMessageBodySnippet
    {
        [JsonProperty("snippet_id")]
        public long SnippetId { get; set; }

        [JsonProperty("snippet_language")]
        public string SnippetLanguage { get; set; }

        [JsonProperty("restricted")]
        public int Restricted { get; set; }

        [JsonProperty("instrumental")]
        public int Instrumental { get; set; }

        [JsonProperty("snippet_body")]
        public string SnippetBody { get; set; }

        [JsonProperty("script_tracking_url")]
        public string ScriptTrackingUrl { get; set; }

        [JsonProperty("pixel_tracking_url")]
        public string PixelTrackingUrl { get; set; }

        [JsonProperty("html_tracking_url")]
        public string HtmlTrackingUrl { get; set; }

        [JsonProperty("updated_time")]
        public DateTime UpdatedTime { get; set; }
    }
}
