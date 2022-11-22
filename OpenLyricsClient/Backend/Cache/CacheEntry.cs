using System;
using DevBase.Utilities;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Cache
{
    [Serializable]
    public class CacheEntry
    {
        private string _id;
        private CacheData cacheData;

        public CacheEntry(SongRequestObject songRequestObject, CacheData cacheData)
        {
            this.cacheData = cacheData;

            if (DataValidator.ValidateData(songRequestObject))
            {
                string append = string.Empty;

                append += songRequestObject.SongName;
                append += songRequestObject.Album;
                append += songRequestObject.Artists;
                append += songRequestObject.FormattedSongName;
                append += songRequestObject.FormattedSongAlbum;
                append += songRequestObject.SongDuration;

                this._id = CryptoUtils.ToMD5(append);
            }
        }

        public CacheEntry(string id, CacheData cacheData)
        {
            this.cacheData = cacheData;
            this._id = id;
        }

        public string Id
        {
            get => _id;
        }

        public CacheData CacheData
        {
            get => cacheData;
        }
    }
}