using System.Threading.Tasks;
using DevBase.Api.Apis.Deezer.Structure.Json;
using DevBase.Web;
using DevBase.Web.ResponseData;
using MusixmatchClientLib.API.Model.Types;
using OpenLyricsClient.Shared.Structure.Artwork;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Backend.Collector.Artwork.Providers.Deezer;

public class DeezerCollector : IArtworkCollector
{
    public async Task<Shared.Structure.Artwork.Artwork> GetArtwork(SongResponseObject songResponseObject)
    {
        if (!DataValidator.ValidateData(songResponseObject))
            return new Shared.Structure.Artwork.Artwork();

        if (!songResponseObject.CollectorName.Equals(this.CollectorName()))
            return new Shared.Structure.Artwork.Artwork();
                
        if (!DataValidator.ValidateData(songResponseObject.SongRequestObject))
            return new Shared.Structure.Artwork.Artwork();

        if (!(songResponseObject.Track is JsonDeezerSearchDataResponse))
            return new Shared.Structure.Artwork.Artwork();

        JsonDeezerSearchDataResponse track = (JsonDeezerSearchDataResponse)songResponseObject.Track;

        string artworkUrl = GetArtworkUrl(track);

        if (artworkUrl.IsNullOrEmpty())
            return new Shared.Structure.Artwork.Artwork();
            
        Shared.Structure.Artwork.Artwork artwork = await GetArtwork(artworkUrl);

        if (DataValidator.ValidateData(artwork))
            return artwork;
            
        return new Shared.Structure.Artwork.Artwork();
    }
    
    private string GetArtworkUrl(JsonDeezerSearchDataResponse track)
    {
        if (!track.album.cover_xl.IsNullOrEmpty())
        {
            return track.album.cover_xl;
        } 
        else if (!track.album.cover_big.IsNullOrEmpty())
        {
            return track.album.cover_big;
        }
        else if (!track.album.cover.IsNullOrEmpty())
        {
            return track.album.cover;
        }
        else if (!track.album.cover_medium.IsNullOrEmpty())
        {
            return track.album.cover_medium;
        }
        else if (!track.album.cover_small.IsNullOrEmpty())
        {
            return track.album.cover_small;
        }

        return string.Empty;
    }
    
    private async Task<Shared.Structure.Artwork.Artwork> GetArtwork(string url)
    {
        ResponseData artwork = await new Request(url).GetResponseAsync();
        return new Shared.Structure.Artwork.Artwork(artwork.Content, string.Empty, ArtworkReturnCode.SUCCESS);
    }

    public string CollectorName()
    {
        return "Deezer";
    }

    public int Quality()
    {
        return 8;
    }
}