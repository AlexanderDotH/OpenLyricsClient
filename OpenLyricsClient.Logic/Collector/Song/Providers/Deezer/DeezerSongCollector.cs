using DevBase.Api.Apis.Deezer.Structure.Json;
using OpenLyricsClient.Logic.Debugger;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Logic.Collector.Song.Providers.Deezer;

public class DeezerSongCollector : ISongCollector
{
    private DevBase.Api.Apis.Deezer.Deezer _deezerApi;
    private Debugger<DeezerSongCollector> _debugger;

    public DeezerSongCollector()
    {
        this._debugger = new Debugger<DeezerSongCollector>(this);
        this._deezerApi = new DevBase.Api.Apis.Deezer.Deezer();
    }
    
    public async Task<SongResponseObject> GetSong(SongRequestObject songRequestObject)
    {
        if (songRequestObject.SongName.IsNullOrEmpty())
            return null;

        JsonDeezerSearchResponse searchResponse = await this._deezerApi.Search(songRequestObject.SongName);

        if (!DataValidator.ValidateData(searchResponse))
            return null;
        
        this._debugger.Write("Found " + searchResponse.total + " songs!", DebugType.INFO);

        for (int i = 0; i < searchResponse.data.Count; i++)
        {
            JsonDeezerSearchDataResponse track = searchResponse.data[i];

            if (IsValidSong(track, songRequestObject))
            {
                SongResponseObject songResponseObject = new SongResponseObject()
                {
                    SongRequestObject = songRequestObject,
                    Track = track,
                    CollectorName = this.CollectorName()
                };
                
                this._debugger.Write("Got current song " + track.title + "!", DebugType.INFO);

                return songResponseObject;
            }
        }

        this._debugger.Write("Could not find current song", DebugType.ERROR);
        
        return null;
    }

    private bool IsValidSong(JsonDeezerSearchDataResponse track, SongRequestObject songRequestObject)
    {
        if (!DataValidator.ValidateData(track) ||
            !DataValidator.ValidateData(songRequestObject))
            return false;

        if (IsSimilar(songRequestObject.FormattedSongName, track.title) != IsSimilar(songRequestObject.FormattedSongAlbum, track.album.title))
        {
            if (!IsSimilar(songRequestObject.FormattedSongAlbum, track.album.title))
                return false;
        }

        //if ((track.TrackLength * 1000) != songRequestObject.SongDuration)
        //    return false;

        if (!IsSimilar(songRequestObject.FormattedSongName, track.title))
            return false;

        if (!IsSimilar(songRequestObject.SongName, track.title))
            return false;

        for (int i = 0; i < songRequestObject.Artists.Length; i++)
        {
            string artist = songRequestObject.Artists[i];

            if (track.artist.name.Contains(artist))
            {
                return true;
            }
        }

        return false;
    }

    //Untested! I should make everything a bit more strict
    private bool IsSimilar(string string1, string string2)
    {
        return MathUtils.CalculateLevenshteinDistance(string1, string2) >=
               Math.Abs(string1.Length - string2.Length);
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