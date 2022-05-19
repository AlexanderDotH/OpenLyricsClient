using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchTrackSubtitlesGetMessageBodySubtitleListSubtitle
    {
        [JsonProperty("subtitle_id")]
        public long SubtitleId { get; set; }

        [JsonProperty("restricted")]
        public int Restricted { get; set; }

        [JsonProperty("published_status")]
        public int PublishedStatus { get; set; }

        [JsonProperty("subtitle_body")]
        public string SubtitleBody { get; set; }

        [JsonProperty("subtitle_avg_count")]
        public int SubtitleAvgCount { get; set; }

        [JsonProperty("lyrics_copyright")]
        public string LyricsCopyright { get; set; }

        [JsonProperty("subtitle_length")]
        public int SubtitleLength { get; set; }

        [JsonProperty("subtitle_language")]
        public string SubtitleLanguage { get; set; }

        [JsonProperty("subtitle_language_description")]
        public string SubtitleLanguageDescription { get; set; }

        [JsonProperty("script_tracking_url")]
        public string ScriptTrackingUrl { get; set; }

        [JsonProperty("pixel_tracking_url")]
        public string PixelTrackingUrl { get; set; }

        [JsonProperty("html_tracking_url")]
        public string HtmlTrackingUrl { get; set; }

        [JsonProperty("writer_list")]
        public object[] WriterList { get; set; }

        [JsonProperty("publisher_list")]
        public object[] PublisherList { get; set; }

        [JsonProperty("updated_time")]
        public DateTime UpdatedTime { get; set; }
    }
}
