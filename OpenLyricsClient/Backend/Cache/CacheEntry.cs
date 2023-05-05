using System;
using DevBase.Utilities;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Shared.Structure;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Backend.Cache
{
    [Serializable]
    public class CacheEntry
    {
        private string _id;
        private CacheData cacheData;
        private long _expirationDate;

        public CacheEntry(SongRequestObject songRequestObject, CacheData cacheData, long expirationDate)
        {
            this.cacheData = cacheData;
            this._expirationDate = expirationDate;

            if (DataValidator.ValidateData(songRequestObject))
            {
                string append = string.Empty;

                append += songRequestObject.SongName;
                append += songRequestObject.Album;
                append += songRequestObject.Artists;
                append += songRequestObject.SongDuration;

                this._id = CryptoUtils.ToMD5(append);
            }
        }

        public CacheEntry(string id, CacheData cacheData, long expirationDate)
        {
            this.cacheData = cacheData;
            this._expirationDate = expirationDate;
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

        public long ExpirationDate
        {
            get => _expirationDate;
        }
    }
}