using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Logic.Cache
{
    [Serializable]
    public class CacheEntry
    {
        private string _id;
        private CacheData _cacheData;
        private long _expirationDate;

        public CacheEntry(SongRequestObject songRequestObject, CacheData cacheData, long expirationDate)
        {
            this._cacheData = cacheData;
            this._expirationDate = expirationDate;

            if (DataValidator.ValidateData(songRequestObject))
            {
                string append = string.Empty;

                append += songRequestObject.SongName;
                append += songRequestObject.Album;
                append += songRequestObject.Artists;
                append += songRequestObject.SongDuration;

                this._id = DevBase.Cryptography.MD5.MD5.ToMD5String(append);
            }
        }

        public CacheEntry(string id, CacheData cacheData, long expirationDate)
        {
            this._cacheData = cacheData;
            this._expirationDate = expirationDate;
            this._id = id;
        }

        public string Id
        {
            get => _id;
        }

        public CacheData CacheData
        {
            get => _cacheData;
        }

        public long ExpirationDate
        {
            get => _expirationDate;
        }
    }
}