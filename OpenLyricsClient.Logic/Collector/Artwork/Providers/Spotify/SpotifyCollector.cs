using System.Net;
using OpenLyricsClient.Shared.Structure.Artwork;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using SpotifyAPI.Web;

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

        Image maxImage = new Image();
        maxImage.Height = 0;
        maxImage.Width = 0;
        
        for (int i = 0; i < track.Album.Images.Count; i++)
        {
            Image image = track.Album.Images[i];

            int size = image.Height * image.Width;
            int imageSize = maxImage.Height * maxImage.Width;

            if (size > imageSize)
            {
                maxImage = image;
            }
        }

        return await GetArtwork(maxImage.Url);
    }

    private async Task<Shared.Structure.Artwork.Artwork> GetArtwork(string url)
    {
        byte[] artwork = await new WebClient().DownloadDataTaskAsync(url);
        return new Shared.Structure.Artwork.Artwork(artwork, ArtworkReturnCode.SUCCESS);
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