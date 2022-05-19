using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch.Json
{
    public class MusixMatchMatcherTrackGetMessageBodyTrack
    {
        [JsonProperty("track_id")]
        public long TrackId { get; set; }

        [JsonProperty("track_mbid")]
        public string TrackMbid { get; set; }

        [JsonProperty("track_isrc")]
        public string TrackIsrc { get; set; }

        [JsonProperty("commontrack_isrcs")]
        public List<List<string>> CommontrackIsrcs { get; set; }

        [JsonProperty("track_spotify_id")]
        public string TrackSpotifyId { get; set; }

        [JsonProperty("commontrack_spotify_ids")]
        public string[] CommontrackSpotifyIds { get; set; }

        [JsonProperty("track_soundcloud_id")]
        public int TrackSoundcloudId { get; set; }

        [JsonProperty("track_xboxmusic_id")]
        public string TrackXboxmusicId { get; set; }

        [JsonProperty("track_name")]
        public string TrackName { get; set; }

        [JsonProperty("track_name_translation_list")]
        public object[] TrackNameTranslationList { get; set; }

        [JsonProperty("track_rating")]
        public int TrackRating { get; set; }

        [JsonProperty("track_length")]
        public int TrackLength { get; set; }

        [JsonProperty("commontrack_id")]
        public long CommontrackId { get; set; }

        [JsonProperty("instrumental")]
        public int Instrumental { get; set; }

        [JsonProperty("explicit")]
        public int Explicit { get; set; }

        [JsonProperty("has_lyrics")]
        public int HasLyrics { get; set; }

        [JsonProperty("has_lyrics_crowd")]
        public int HasLyricsCrowd { get; set; }

        [JsonProperty("has_subtitles")]
        public int HasSubtitles { get; set; }

        [JsonProperty("has_richsync")]
        public int HasRichsync { get; set; }

        [JsonProperty("has_track_structure")]
        public int HasTrackStructure { get; set; }

        [JsonProperty("num_favourite")]
        public int NumFavourite { get; set; }

        [JsonProperty("lyrics_id")]
        public int LyricsId { get; set; }

        [JsonProperty("subtitle_id")]
        public int SubtitleId { get; set; }

        [JsonProperty("album_id")]
        public int AlbumId { get; set; }

        [JsonProperty("album_name")]
        public string AlbumName { get; set; }

        [JsonProperty("artist_id")]
        public int ArtistId { get; set; }

        [JsonProperty("artist_mbid")]
        public string ArtistMbid { get; set; }

        [JsonProperty("artist_name")]
        public string ArtistName { get; set; }

        [JsonProperty("album_coverart_100x100")]
        public string AlbumCoverart100x100 { get; set; }

        [JsonProperty("album_coverart_350x350")]
        public string AlbumCoverart350x350 { get; set; }

        [JsonProperty("album_coverart_500x500")]
        public string AlbumCoverart500x500 { get; set; }

        [JsonProperty("album_coverart_800x800")]
        public string AlbumCoverart800x800 { get; set; }

        [JsonProperty("track_share_url")]
        public string TrackShareUrl { get; set; }

        [JsonProperty("track_edit_url")]
        public string TrackEditUrl { get; set; }

        [JsonProperty("commontrack_vanity_id")]
        public string CommontrackVanityId { get; set; }

        [JsonProperty("restricted")]
        public int Restricted { get; set; }

        [JsonProperty("first_release_date")]
        public DateTime FirstReleaseDate { get; set; }

        [JsonProperty("updated_time")]
        public DateTime UpdatedTime { get; set; }

        [JsonProperty("primary_genres")]
        public MusixMatchMatcherTrackGetMessageBodyTrackPrimaryGenres MatcherTrackGetMessageBodyTrackPrimaryGenres { get; set; }

        [JsonProperty("secondary_genres")]
        public MusixMatchMatcherTrackGetMessageBodyTrackSecondaryGenres MatcherTrackGetMessageBodyTrackSecondaryGenres { get; set; }
    }
}
