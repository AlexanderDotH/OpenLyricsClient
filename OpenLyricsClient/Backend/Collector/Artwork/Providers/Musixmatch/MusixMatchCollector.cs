using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using DevBase.Web;
using MusixmatchClientLib;
using MusixmatchClientLib.API.Model.Types;
using MusixmatchClientLib.Types;
using MusixmatchClientLib.Web.RequestData;
using MusixmatchClientLib.Web.ResponseData;
using OpenLyricsClient.Backend.Collector.Lyrics;
using OpenLyricsClient.Backend.Collector.Token.Provider.Musixmatch;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Structure.Artwork;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using ResponseData = DevBase.Web.ResponseData.ResponseData;

namespace OpenLyricsClient.Backend.Collector.Artwork.Providers.Musixmatch
{
    class MusixMatchCollector : IArtworkCollector
    {
        private Debugger<MusixMatchCollector> _debugger;

        public MusixMatchCollector()
        {
            this._debugger = new Debugger<MusixMatchCollector>(this);
        }

        public async Task<Structure.Artwork.Artwork> GetArtwork(SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(songRequestObject))
                return null;

            if (!DataValidator.ValidateData(songRequestObject))
                return null;

            string token = MusixmatchTokenCollector.Instance.GetToken().Token;

            if (!DataValidator.ValidateData(token))
                return null;

            MusixmatchClient musixmatchClient = new MusixmatchClient(token);

            if (!DataValidator.ValidateData(musixmatchClient))
                return null;

            List<Track> tracks = null;

            if (songRequestObject.SelectioMode == SelectionMode.PERFORMANCE)
            {
                tracks = await musixmatchClient.SongSearchAsync(
                    new TrackSearchParameters
                    {
                        Album = songRequestObject.Album,
                        Title = songRequestObject.SongName,
                        Artist = songRequestObject.GetArtistsSplit(),
                    });
            }
            else
            {
                tracks = await musixmatchClient.SongSearchAsync(
                    new TrackSearchParameters
                    {
                        Album = songRequestObject.Album,
                        Title = songRequestObject.SongName,
                        Artist = songRequestObject.GetArtistsSplit(),
                    });

                if (!DataValidator.ValidateData(tracks) || DataValidator.ValidateData(tracks) && tracks.Count == 0)
                {
                    tracks = await musixmatchClient.SongSearchAsync(
                        new TrackSearchParameters
                        {
                            Album = songRequestObject.FormattedSongAlbum,
                            Title = songRequestObject.SongName,
                        });
                }
            }

            if (!DataValidator.ValidateData(tracks))
            {
                this._debugger.Write("Track not found", DebugType.ERROR);
                return null;
            }

            this._debugger.Write(string.Format("Found {0} tracks", tracks.Count), DebugType.INFO);

            for (int i = 0; i < tracks.Count; i++)
            {
                Track track = tracks[i];
                
                if (!IsValidSong(track, songRequestObject))
                    continue;

                return await GetArtwork(track.AlbumCoverart800x800);
            }

            return new Structure.Artwork.Artwork(null, ArtworkReturnCode.FAILED);
        }

        private async Task<Structure.Artwork.Artwork> GetArtwork(string url)
        {
            byte[] artwork = await new WebClient().DownloadDataTaskAsync(url);
            return new Structure.Artwork.Artwork(artwork, ArtworkReturnCode.SUCCESS);
        }
        
        private bool IsValidSong(Track track, SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(track) ||
                !DataValidator.ValidateData(songRequestObject))
                return false;

            if (IsSimilar(songRequestObject.FormattedSongName, track.TrackName) != IsSimilar(songRequestObject.FormattedSongAlbum, track.AlbumName))
            {
                if (!IsSimilar(songRequestObject.FormattedSongAlbum, track.AlbumName))
                    return false;
            }

            //if ((track.TrackLength * 1000) != songRequestObject.SongDuration)
            //    return false;

            if (!IsSimilar(songRequestObject.FormattedSongName, track.TrackName))
                return false;

            if (!IsSimilar(songRequestObject.SongName, track.TrackName))
                return false;

            for (int i = 0; i < songRequestObject.Artists.Length; i++)
            {
                string artist = songRequestObject.Artists[i];

                if (track.ArtistName.Contains(artist))
                {
                    return true;
                }
            }

            return false;
        }
        
        private bool IsSimilar(string string1, string string2)
        {
            return MathUtils.CalculateLevenshteinDistance(string1, string2) >=
                   Math.Abs(string1.Length - string2.Length);
        }
    }
}
