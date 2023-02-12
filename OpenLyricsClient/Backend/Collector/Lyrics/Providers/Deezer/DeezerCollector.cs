using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DevBase.Api.Apis.Deezer.Structure.Json;
using DevBase.Format;
using DevBase.Format.Formats.LrcFormat;
using DevBase.Format.Structure;
using DevBase.Generics;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Deezer;

public class DeezerCollector : ILyricsCollector
{
    private DevBase.Api.Apis.Deezer.Deezer _deezerApi;
    private Debugger<DeezerCollector> _debugger;

    public DeezerCollector()
    {
        this._debugger = new Debugger<DeezerCollector>(this);
        this._deezerApi = new DevBase.Api.Apis.Deezer.Deezer();
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

        SongMetadata songMetadata = SongMetadata.ToSongMetadata(
            track.title,
            track.album.title,
            DataConverter.ToArtists(track.artist),
            track.duration);
        
        if (!DataValidator.ValidateData(lyricsResponse))
        {
            this._debugger.Write("Could not find lyrics for " + songMetadata.Name + "!", DebugType.ERROR);
            return new LyricData();
        }

        if (!DataValidator.ValidateData(
                lyricsResponse.data,
                lyricsResponse.data.track,
                lyricsResponse.data.track.lyrics,
                lyricsResponse.data.track.lyrics.synchronizedLines))
        {
            this._debugger.Write("Could not find lyrics for " + songMetadata.Name + "!", DebugType.ERROR);
            return new LyricData();
        }
        
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
            AList<LyricElement> lyricElements =
                fileFormatParser.FormatFromString(lrcFile).Lyrics;

            if (DataValidator.ValidateData(lyricElements))
            {
                this._debugger.Write("Fetched lyrics for " + songMetadata.Name + "!", DebugType.INFO);
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
        return 10;
    }
}