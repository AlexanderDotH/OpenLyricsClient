using System;
using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchSubtitle
    {
        [JsonProperty("subtitle_id")]
        public long SubtitleId { get; set; }

        [JsonProperty("restricted")]
        public long Restricted { get; set; }

        [JsonProperty("published_status")]
        public long PublishedStatus { get; set; }

        [JsonProperty("subtitle_body")]
        public string SubtitleBody { get; set; }

        [JsonProperty("subtitle_avg_count")]
        public long SubtitleAvgCount { get; set; }

        [JsonProperty("lyrics_copyright")]
        public string LyricsCopyright { get; set; }

        [JsonProperty("subtitle_length")]
        public long SubtitleLength { get; set; }

        [JsonProperty("subtitle_language")]
        public string SubtitleLanguage { get; set; }

        [JsonProperty("subtitle_language_description")]
        public string SubtitleLanguageDescription { get; set; }

        [JsonProperty("script_tracking_url")]
        public Uri ScriptTrackingUrl { get; set; }

        [JsonProperty("pixel_tracking_url")]
        public Uri PixelTrackingUrl { get; set; }

        [JsonProperty("html_tracking_url")]
        public Uri HtmlTrackingUrl { get; set; }

        [JsonProperty("writer_list")]
        public object[] WriterList { get; set; }

        [JsonProperty("publisher_list")]
        public object[] PublisherList { get; set; }

        [JsonProperty("updated_time")]
        public DateTimeOffset UpdatedTime { get; set; }
    }
}