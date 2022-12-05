using System.Net;
using System.Threading.Tasks;
using OpenLyricsClient.Backend.Structure.Artwork;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using SpotifyAPI.Web;

namespace OpenLyricsClient.Backend.Collector.Artwork.Providers.Spotify;

public class SpotifyCollector : IArtworkCollector
{
    public async Task<Structure.Artwork.Artwork> GetArtwork(SongResponseObject songResponseObject)
    {
        if (!DataValidator.ValidateData(songResponseObject))
            return new Structure.Artwork.Artwork();

        if (!DataValidator.ValidateData(songResponseObject.SongRequestObject))
            return new Structure.Artwork.Artwork();

        if (!DataValidator.ValidateData(songResponseObject.SongRequestObject.Song))
            return new Structure.Artwork.Artwork();

        if (!DataValidator.ValidateData(songResponseObject.SongRequestObject.Song.TrackObject,
                songResponseObject.SongRequestObject.Song.TrackObject))
            return new Structure.Artwork.Artwork();

        Structure.Song.Song song = songResponseObject.SongRequestObject.Song;

        if (song.DataOrigin != DataOrigin.SPOTIFY)
            return new Structure.Artwork.Artwork();

        if (!(song.TrackObject is FullTrack))
            return new Structure.Artwork.Artwork();

        FullTrack track = (FullTrack)song.TrackObject;

        for (int i = 0; i < track.Album.Images.Count; i++)
        {
            Image image = track.Album.Images[i];
            return await GetArtwork(image.Url);
        }

        return new Structure.Artwork.Artwork();
    }

    private async Task<Structure.Artwork.Artwork> GetArtwork(string url)
    {
        byte[] artwork = await new WebClient().DownloadDataTaskAsync(url);
        return new Structure.Artwork.Artwork(artwork, ArtworkReturnCode.SUCCESS);
    }
    
    public string CollectorName()
    {
        return "Spotify";
    }
}