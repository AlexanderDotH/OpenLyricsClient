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
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using Squalr.Engine.Utils.Extensions;
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

        public async Task<Structure.Artwork.Artwork> GetArtwork(SongResponseObject songResponseObject)
        {
            if (!DataValidator.ValidateData(songResponseObject))
                return new Structure.Artwork.Artwork();

            if (!songResponseObject.CollectorName.Equals(this.CollectorName()))
                return new Structure.Artwork.Artwork();
                
            if (!DataValidator.ValidateData(songResponseObject.SongRequestObject))
                return new Structure.Artwork.Artwork();

            if (!(songResponseObject.Track is Track))
                return new Structure.Artwork.Artwork();

            Track track = (Track)songResponseObject.Track;

            string artworkUrl = GetArtworkUrl(track);

            if (artworkUrl.IsNullOrEmpty())
                return new Structure.Artwork.Artwork();
            
            Structure.Artwork.Artwork artwork = await GetArtwork(artworkUrl);

            if (DataValidator.ValidateData(artwork))
                return artwork;
            
            return new Structure.Artwork.Artwork();
        }

        private async Task<Structure.Artwork.Artwork> GetArtwork(string url)
        {
            byte[] artwork = await new WebClient().DownloadDataTaskAsync(url);
            return new Structure.Artwork.Artwork(artwork, ArtworkReturnCode.SUCCESS);
        }

        private string GetArtworkUrl(Track track)
        {
            if (!track.AlbumCoverart800x800.IsNullOrEmpty())
            {
                return track.AlbumCoverart800x800;
            } 
            else if (!track.AlbumCoverart500x500.IsNullOrEmpty())
            {
                return track.AlbumCoverart500x500;
            }
            else if (!track.AlbumCoverart350x350.IsNullOrEmpty())
            {
                return track.AlbumCoverart350x350;
            }
            else if (!track.AlbumCoverart100x100.IsNullOrEmpty())
            {
                return track.AlbumCoverart100x100;
            }

            return string.Empty;
        }

        public string CollectorName()
        {
            return "MusixMatch";
        }

        public int Quality()
        {
            return 5;
        }
    }
    
    
}
