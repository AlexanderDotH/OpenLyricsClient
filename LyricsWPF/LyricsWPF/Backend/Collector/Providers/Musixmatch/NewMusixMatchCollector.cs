using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBaseFormat;
using DevBaseFormat.Formats.LrcFormat;
using DevBaseFormat.Structure;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;
using MusixmatchClientLib;
using MusixmatchClientLib.API.Model.Types;
using MusixmatchClientLib.Auth;
using MusixmatchClientLib.Types;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch
{
    public class NewMusixMatchCollector : ICollector
    {
        private Debugger<NewMusixMatchCollector> _debugger;

        private MusixmatchToken _musixmatchToken;
        private MusixmatchClient _musixmatchClient;

        public NewMusixMatchCollector()
        {
            this._debugger = new Debugger<NewMusixMatchCollector>(this);

            this._musixmatchToken = new MusixmatchToken();
            this._musixmatchClient = new MusixmatchClient(this._musixmatchToken);
        }

        public async Task<LyricData> GetLyrics(SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(songRequestObject))
                return new LyricData(LyricReturnCode.Failed);

            if (!DataValidator.ValidateData(this._musixmatchToken))
                return new LyricData(LyricReturnCode.Failed);

            if (!DataValidator.ValidateData(this._musixmatchClient))
                return new LyricData(LyricReturnCode.Failed);

            GenericList<Track> tracks = await this._musixmatchClient.SongSearchAsync(
                new TrackSearchParameters
            {
                Album = songRequestObject.FormattedSongAlbum, // Album name
                Artist = songRequestObject.GetArtistsSplit(), // Artist name
                Title = songRequestObject.FormattedSongName, // Track name
                HasSubtitles = true, // Only search for tracks with synced lyrics
                Sort = TrackSearchParameters.SortStrategy.TrackRatingAsc // List sorting strategy 
            });

            for (int i = 0; i < tracks.Length; i++)
            {
                Track track = tracks[i];

                if (IsTrackValid(track, songRequestObject))
                {
                    SubtitleRawResponse response = await this._musixmatchClient.GetTrackSubtitlesRawAsync(track.TrackId, MusixmatchClient.SubtitleFormat.Lrc);

                    FileFormatParser<LrcObject> fileFormatParser =
                        new FileFormatParser<LrcObject>(
                            new LrcParser<LrcObject>());

                    if (DataValidator.ValidateData(fileFormatParser))
                    {
                        GenericList<LyricElement> lyricElements =
                            fileFormatParser.FormatFromString(response.SubtitleBody).Lyrics;

                        if (DataValidator.ValidateData(lyricElements))
                        {
                            return await LyricData.ConvertToData(lyricElements, track.TrackName, track.AlbumName, songRequestObject.Artists, this.CollectorName());
                        }
                    }
                }
            }

            return new LyricData(LyricReturnCode.Failed);
        }

        private bool IsTrackValid(Track track, SongRequestObject songRequestObject)
        {

            if (!track.TrackName.Equals(songRequestObject.SongName) ||
                !track.TrackName.Equals(songRequestObject.FormattedSongName))
                return false;

            if (!track.AlbumName.Equals(songRequestObject.FormattedSongAlbum) ||
                !track.AlbumName.Equals(songRequestObject.FormattedSongAlbum))
                return false;

            return true;
        }

        public string CollectorName()
        {
            return "MusixMatch2";
        }

        public int ProviderQuality()
        {
            return 10;
        }
    }
}
