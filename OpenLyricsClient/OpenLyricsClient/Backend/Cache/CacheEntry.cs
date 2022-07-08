using System;
using DevBase.Utilities;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;

namespace OpenLyricsClient.Backend.Cache
{
    public class CacheEntry
    {
        private long _id;
        private LyricData cacheData;

        public CacheEntry(SongRequestObject songRequestObject, LyricData cacheData)
        {
            this.cacheData = cacheData;

            if (songRequestObject != null)
            {
                string append = string.Empty;

                append += MemoryUtils.GetSize(songRequestObject.SongName).ToString();
                append += MemoryUtils.GetSize(songRequestObject.Album).ToString();
                append += MemoryUtils.GetSize(songRequestObject.Artists).ToString();
                append += MemoryUtils.GetSize(songRequestObject.FormattedSongName).ToString();
                append += MemoryUtils.GetSize(songRequestObject.FormattedSongAlbum).ToString();
                append += MemoryUtils.GetSize(songRequestObject.SongDuration).ToString();

                this._id = Convert.ToInt64(append);
            }
        }

        public CacheEntry(long id, LyricData cacheData)
        {
            this.cacheData = cacheData;
            this._id = id;
        }

        public long Id
        {
            get => _id;
        }

        public LyricData CacheData
        {
            get => cacheData;
        }
    }
}
