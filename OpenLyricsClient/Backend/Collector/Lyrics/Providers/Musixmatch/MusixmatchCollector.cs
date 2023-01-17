using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using DevBaseFormat;
using DevBaseFormat.Formats.LrcFormat;
using DevBaseFormat.Formats.MmlFormat;
using DevBaseFormat.Structure;
using MusixmatchClientLib;
using MusixmatchClientLib.API.Model.Types;
using MusixmatchClientLib.Auth;
using MusixmatchClientLib.Types;
using OpenLyricsClient.Backend.Collector.Token.Provider.Musixmatch;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch
{
    public class MusixmatchCollector : ILyricsCollector
    {
        private Debugger<MusixmatchCollector> _debugger;

        public MusixmatchCollector()
        {
            this._debugger = new Debugger<MusixmatchCollector>(this);
        }

        public async Task<LyricData> GetLyrics(SongResponseObject songResponseObject)
        {
            if (!DataValidator.ValidateData(songResponseObject))
                return new LyricData();

            if (!songResponseObject.CollectorName.Equals(this.CollectorName()))
                return new LyricData();
            
            string token = MusixmatchTokenCollector.Instance.GetToken().Token;

            if (!DataValidator.ValidateData(token))
                return new LyricData();

            MusixmatchClient musixmatchClient = new MusixmatchClient(token);

            if (!DataValidator.ValidateData(musixmatchClient))
                return new LyricData();

            if (!(songResponseObject.Track is Track))
                return new LyricData();

            Track track = (Track)songResponseObject.Track;
                
            if (track.Instrumental == 1)
            {
                return new LyricData(
                    LyricReturnCode.SUCCESS,
                    SongMetadata.ToSongMetadata(track.TrackName,
                        track.AlbumName,
                        new string[] { track.ArtistName }, 
                        track.TrackLength),
                    LyricType.INSTRUMENTAL);
            }

            if (track.HasSubtitles == 0) 
                return new LyricData();

            try
            {
                SubtitleRawResponse response = await musixmatchClient.GetTrackSubtitlesRawAsync(track.TrackId, MusixmatchClient.SubtitleFormat.Lrc);

                if (!DataValidator.ValidateData(response, response.SubtitleBody))
                {
                    this._debugger.Write("Could not find lyrics for " + track.TrackName + "!", DebugType.ERROR);
                    return new LyricData();
                }
                
                FileFormatParser<LrcObject> fileFormatParser =
                    new FileFormatParser<LrcObject>(
                        new LrcParser<LrcObject>());

                if (DataValidator.ValidateData(fileFormatParser))
                {
                    GenericList<LyricElement> lyricElements =
                        fileFormatParser.FormatFromString(response.SubtitleBody).Lyrics;

                    this._debugger.Write(string.Format("Found lyrics for {0}", track.TrackName), DebugType.INFO);

                    if (DataValidator.ValidateData(lyricElements))
                    {
                        return await LyricData.ConvertToData(
                            lyricElements,
                            SongMetadata.ToSongMetadata(track.TrackName,
                                track.AlbumName,
                                new string[] { track.ArtistName },
                                track.TrackLength),
                            this.CollectorName());
                    }
                }
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }

            return new LyricData();
        }

        public string CollectorName()
        {
            return "MusixMatch";
        }

        public int ProviderQuality()
        {
            return (Core.INSTANCE.SettingManager.Settings.LyricSelectionMode == SelectionMode.PERFORMANCE ? 10 : 10); 
        }
    }
}
