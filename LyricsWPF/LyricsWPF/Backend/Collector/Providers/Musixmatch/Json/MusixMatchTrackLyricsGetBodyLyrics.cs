using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchTrackLyricsGetBodyLyrics
    {
        [JsonProperty("lyrics_id")]
        public long LyricsId { get; set; }

        [JsonProperty("can_edit")]
        public int CanEdit { get; set; }

        [JsonProperty("locked")]
        public int Locked { get; set; }

        [JsonProperty("published_status")]
        public int PublishedStatus { get; set; }

        [JsonProperty("action_requested")]
        public string ActionRequested { get; set; }

        [JsonProperty("verified")]
        public int Verified { get; set; }

        [JsonProperty("restricted")]
        public int Restricted { get; set; }

        [JsonProperty("instrumental")]
        public int Instrumental { get; set; }

        [JsonProperty("explicit")]
        public int Explicit { get; set; }

        [JsonProperty("lyrics_body")]
        public string LyricsBody { get; set; }

        [JsonProperty("lyrics_language")]
        public string LyricsLanguage { get; set; }

        [JsonProperty("lyrics_language_description")]
        public string LyricsLanguageDescription { get; set; }

        [JsonProperty("script_tracking_url")]
        public string ScriptTrackingUrl { get; set; }

        [JsonProperty("pixel_tracking_url")]
        public string PixelTrackingUrl { get; set; }

        [JsonProperty("html_tracking_url")]
        public string HtmlTrackingUrl { get; set; }

        [JsonProperty("lyrics_copyright")]
        public string LyricsCopyright { get; set; }

        [JsonProperty("writer_list")]
        public object[] WriterList { get; set; }

        [JsonProperty("publisher_list")]
        public object[] PublisherList { get; set; }

        [JsonProperty("backlink_url")]
        public string BacklinkUrl { get; set; }

        [JsonProperty("updated_time")]
        public DateTime UpdatedTime { get; set; }
    }
}
