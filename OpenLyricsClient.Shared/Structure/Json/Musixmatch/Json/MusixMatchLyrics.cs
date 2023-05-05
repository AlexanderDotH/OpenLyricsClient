using System;
using Newtonsoft.Json;

namespace OpenLyricsClient.Shared.Structure.Json.Musixmatch.Json
{
    public class MusixMatchLyrics
    {
        [JsonProperty("lyrics_id")]
        public long LyricsId { get; set; }

        [JsonProperty("can_edit")]
        public long CanEdit { get; set; }

        [JsonProperty("locked")]
        public long Locked { get; set; }

        [JsonProperty("published_status")]
        public long PublishedStatus { get; set; }

        [JsonProperty("action_requested")]
        public string ActionRequested { get; set; }

        [JsonProperty("verified")]
        public long Verified { get; set; }

        [JsonProperty("restricted")]
        public long Restricted { get; set; }

        [JsonProperty("instrumental")]
        public long Instrumental { get; set; }

        [JsonProperty("explicit")]
        public long Explicit { get; set; }

        [JsonProperty("lyrics_body")]
        public string LyricsBody { get; set; }

        [JsonProperty("lyrics_language")]
        public string LyricsLanguage { get; set; }

        [JsonProperty("lyrics_language_description")]
        public string LyricsLanguageDescription { get; set; }

        [JsonProperty("script_tracking_url")]
        public Uri ScriptTrackingUrl { get; set; }

        [JsonProperty("pixel_tracking_url")]
        public Uri PixelTrackingUrl { get; set; }

        [JsonProperty("html_tracking_url")]
        public Uri HtmlTrackingUrl { get; set; }

        [JsonProperty("lyrics_copyright")]
        public string LyricsCopyright { get; set; }

        [JsonProperty("writer_list")]
        public object[] WriterList { get; set; }

        [JsonProperty("publisher_list")]
        public object[] PublisherList { get; set; }

        [JsonProperty("backlink_url")]
        public Uri BacklinkUrl { get; set; }

        [JsonProperty("updated_time")]
        public DateTimeOffset UpdatedTime { get; set; }
    }
}