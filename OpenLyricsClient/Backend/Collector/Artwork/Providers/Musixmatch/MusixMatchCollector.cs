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
using OpenLyricsClient.Shared.Structure.Artwork;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
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

        public async Task<Shared.Structure.Artwork.Artwork> GetArtwork(SongResponseObject songResponseObject)
        {
            if (!DataValidator.ValidateData(songResponseObject))
                return new Shared.Structure.Artwork.Artwork();

            if (!songResponseObject.CollectorName.Equals(this.CollectorName()))
                return new Shared.Structure.Artwork.Artwork();
                
            if (!DataValidator.ValidateData(songResponseObject.SongRequestObject))
                return new Shared.Structure.Artwork.Artwork();

            if (!(songResponseObject.Track is Track))
                return new Shared.Structure.Artwork.Artwork();

            Track track = (Track)songResponseObject.Track;

            string artworkUrl = GetArtworkUrl(track);

            if (artworkUrl.IsNullOrEmpty())
                return new Shared.Structure.Artwork.Artwork();
            
            Shared.Structure.Artwork.Artwork artwork = await GetArtwork(artworkUrl);

            if (DataValidator.ValidateData(artwork))
                return artwork;
            
            return new Shared.Structure.Artwork.Artwork();
        }

        private async Task<Shared.Structure.Artwork.Artwork> GetArtwork(string url)
        {
            ResponseData artwork = await new Request(url).GetResponseAsync();
            return new Shared.Structure.Artwork.Artwork(artwork.Content, string.Empty, ArtworkReturnCode.SUCCESS);
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
