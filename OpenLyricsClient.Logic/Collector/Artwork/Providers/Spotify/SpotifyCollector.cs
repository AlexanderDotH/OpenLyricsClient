using System.Net;
using System.Net.Mime;
using OpenLyricsClient.Shared.Structure.Artwork;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using SpotifyAPI.Web;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Logic.Collector.Artwork.Providers.Spotify;

public class SpotifyCollector : IArtworkCollector
{
    public async Task<Shared.Structure.Artwork.Artwork> GetArtwork(SongResponseObject songResponseObject)
    {
        if (!DataValidator.ValidateData(songResponseObject))
            return new Shared.Structure.Artwork.Artwork();

        if (!DataValidator.ValidateData(songResponseObject.SongRequestObject))
            return new Shared.Structure.Artwork.Artwork();

        if (!DataValidator.ValidateData(songResponseObject.SongRequestObject.Song))
            return new Shared.Structure.Artwork.Artwork();

        if (!DataValidator.ValidateData(songResponseObject.SongRequestObject.Song.TrackObject,
                songResponseObject.SongRequestObject.Song.TrackObject))
            return new Shared.Structure.Artwork.Artwork();

        if (!songResponseObject.CollectorName.Equals(this.CollectorName()))
            return new Shared.Structure.Artwork.Artwork();

        if (!(songResponseObject.Track is FullTrack))
            return new Shared.Structure.Artwork.Artwork();

        FullTrack track = (FullTrack)songResponseObject.Track;

        if (track.Album.Images.IsNullOrEmpty())
            return new Shared.Structure.Artwork.Artwork();

        track.Album.Images.Reverse();
        Image smallest = track.Album.Images.Find(p => p.Height > 0);
        
        return await GetArtwork(smallest.Url, songResponseObject.SongRequestObject);
    }

    private async Task<Shared.Structure.Artwork.Artwork> GetArtwork(string url, SongRequestObject songRequestObject)
    {
        byte[] artwork = await new WebClient().DownloadDataTaskAsync(url);
        return new Shared.Structure.Artwork.Artwork(artwork, Core.INSTANCE.CacheManager.ArtworkPath(songRequestObject), ArtworkReturnCode.SUCCESS);
    }
    
    public string CollectorName()
    {
        return "Spotify";
    }

    public int Quality()
    {
        return 10;
    }
}