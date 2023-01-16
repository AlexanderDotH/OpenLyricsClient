using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBaseApi.Apis.Deezer.Structure.Json;
using DevBaseFormat;
using DevBaseFormat.Formats.LrcFormat;
using DevBaseFormat.Structure;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Deezer;

public class DeezerCollector : ILyricsCollector
{
    private DevBaseApi.Apis.Deezer.Deezer _deezerApi;
    private Debugger<DeezerCollector> _debugger;

    public DeezerCollector()
    {
        this._debugger = new Debugger<DeezerCollector>(this);
        this._deezerApi = new DevBaseApi.Apis.Deezer.Deezer();
    }
    
    public async Task<LyricData> GetLyrics(SongResponseObject songResponseObject)
    {
        if (!DataValidator.ValidateData(songResponseObject))
            return new LyricData();

        if (!DataValidator.ValidateData(songResponseObject.Track))
            return new LyricData();

        if (!DataValidator.ValidateData(songResponseObject.CollectorName))
            return new LyricData();
        
        if (!songResponseObject.CollectorName.Equals(this.CollectorName()))
            return new LyricData();

        if (!(songResponseObject.Track is JsonDeezerSearchDataResponse))
            return new LyricData();
        
        JsonDeezerSearchDataResponse track = (JsonDeezerSearchDataResponse)songResponseObject.Track;

        JsonDeezerLyricsResponse lyricsResponse = await _deezerApi.GetLyrics(Convert.ToString(track.id));

        if (!DataValidator.ValidateData(lyricsResponse))
            return new LyricData();
        
        if (!DataValidator.ValidateData(
                lyricsResponse.data, 
                lyricsResponse.data.track, 
                lyricsResponse.data.track.lyrics, 
                lyricsResponse.data.track.lyrics.synchronizedLines))
            return new LyricData();

        SongMetadata songMetadata = SongMetadata.ToSongMetadata(
            track.title,
            track.album.title,
            DataConverter.ToArtists(track.artist),
            track.duration);
        
        return await ParseLyricData(lyricsResponse.data.track.lyrics, songMetadata);
    }

    private async Task<LyricData> ParseLyricData(JsonDeezerLyricsTrackResponseLyricsResponse lyrics, SongMetadata songMetadata)
    {
        string lrcFile = ConvertToLRC(lyrics.synchronizedLines);

        FileFormatParser<LrcObject> fileFormatParser =
            new FileFormatParser<LrcObject>(
                new LrcParser<LrcObject>());

        if (DataValidator.ValidateData(fileFormatParser))
        {
            GenericList<LyricElement> lyricElements =
                fileFormatParser.FormatFromString(lrcFile).Lyrics;

            if (DataValidator.ValidateData(lyricElements))
            {
                return await LyricData.ConvertToData(lyricElements, songMetadata, this.CollectorName());
            }
        }

        return new LyricData();
    }

    private string ConvertToLRC(List<JsonDeezerLyricsTrackResponseLyricsSynchronizedLineResponse> synchronizedLines)
    {
        StringBuilder stringBuilder = new StringBuilder();

        for (int i = 0; i < synchronizedLines.Count; i++)
        {
            JsonDeezerLyricsTrackResponseLyricsSynchronizedLineResponse line = synchronizedLines[i];
            stringBuilder.AppendLine(string.Format("{0} {1}", line.lrcTimestamp, line.line));
        }

        return stringBuilder.ToString();
    }

    public string CollectorName()
    {
        return "Deezer";
    }

    public int ProviderQuality()
    {
        return 9;
    }
}