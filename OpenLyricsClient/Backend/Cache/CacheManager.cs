using System;
using System.IO;
using DevBase.Generic;
using DevBase.IO;
using DevBase.Utilities;
using Newtonsoft.Json;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Artwork;
using OpenLyricsClient.Backend.Structure.Json;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Cache
{
    public class CacheManager
    {
        private GenericList<CacheEntry> _cache;

        private const string CACHE_EXTENSION = ".cache";
        private const string CACHE_FOLDER_NAME = "Cache";
        private readonly string CACHE_PATH;

        private Debugger<CacheManager> _debugger;

        public CacheManager()
        {
            CACHE_PATH = string.Format("{1}{2}{0}", Path.DirectorySeparatorChar, Core.INSTANCE.SettingManager.WorkingDirectory,
                CACHE_FOLDER_NAME);
            
            this._debugger = new Debugger<CacheManager>(this);

            if (!Directory.Exists(CACHE_PATH))
                Directory.CreateDirectory(CACHE_PATH);

            this._cache = new GenericList<CacheEntry>();

            ReadCache();
        }

        private void ReadCache()
        {
            GenericList<AFileObject> files = AFile.GetFiles(CACHE_PATH, false, "*" + CACHE_EXTENSION);

            for (int i = 0; i < files.Length; i++)
            {
                AFileObject ifo = files.Get(i);

                string id = ifo.FileInfo.Name.Replace(CACHE_EXTENSION, string.Empty);

                JsonCacheData jsonLyricData =
                    new JsonDeserializer<JsonCacheData>().Deserialize(AFile.ReadFile(ifo.FileInfo).ToStringData());

                CacheEntry cacheEntry = new CacheEntry(id, ConvertToCacheData(jsonLyricData));
                this._cache.Add(cacheEntry);
            }
        }

        public void WriteToCache(SongRequestObject songRequestObject, CacheData cacheData, bool addToCache = false)
        {
            string id = CalculateID(songRequestObject);
            string idAsString = Convert.ToString(id);

            string filePath = CACHE_PATH + idAsString + CACHE_EXTENSION;

            File.WriteAllText(filePath, JsonConvert.SerializeObject(ConvertToJsonCacheData(cacheData), Formatting.Indented));
            
            if (addToCache)
                this._cache.Add(new CacheEntry(id, cacheData));
        }
        
        public void WriteToCache(SongRequestObject songRequestObject)
        {
            CacheData data = GetDataByRequest(songRequestObject);
            
            if (DataValidator.ValidateData(data))
                return;
            
            CacheData newCacheData = new CacheData(SongMetadata.ToSongMetadata(songRequestObject), new LyricData(), new Artwork());
            WriteToCache(songRequestObject, newCacheData, true);
        }
        
        public void WriteToCache(SongRequestObject songRequestObject, LyricData lyricData)
        {
            CacheData data = GetDataByRequest(songRequestObject);
            
            if (!DataValidator.ValidateData(data))
                return;
            
            data.LyricData = lyricData;
            WriteToCache(songRequestObject, data, true);
        }
        
        public void WriteToCache(SongRequestObject songRequestObject, Artwork artwork)
        {
            CacheData data = GetDataByRequest(songRequestObject);
            
            if (!DataValidator.ValidateData(data))
                return;
            
            data.Artwork = artwork;
            WriteToCache(songRequestObject, data);
        }

        public void AddToCache(SongRequestObject songRequestObject, CacheData cacheData)
        {
            string id = CalculateID(songRequestObject);
            this._cache.Add(new CacheEntry(id, cacheData));
        }
        
        //maybe add recover by SongRequestObject
        public void AddToCache(SongRequestObject songRequestObject, LyricData lyricData)
        {
            CacheData data = GetDataByRequest(songRequestObject);
            
            if (!DataValidator.ValidateData(data))
                return;

            data.LyricData = lyricData;
        }
        
        public void AddToCache(SongRequestObject songRequestObject, Artwork artwork)
        {
            CacheData data = GetDataByRequest(songRequestObject);
            
            if (!DataValidator.ValidateData(data))
                return;

            data.Artwork = artwork;
        }

        public void ClearCache()
        {
            Core.INSTANCE.SongHandler.RequestNewSong();

            this._cache.Clear();

            if (Directory.Exists(CACHE_PATH))
            {
                GenericList<AFileObject> files = AFile.GetFiles(CACHE_PATH, false, "*" + CACHE_EXTENSION);

                try
                {
                    files.ForEach(f => f.FileInfo.Delete());
                }
                catch (Exception e)
                {
                    this._debugger.Write("Could not delete cache file: " + e.Message, DebugType.ERROR);
                }
            }
        }

        public void RemoveDataByRequest(SongRequestObject songRequestObject)
        {
            for (int i = 0; i < this._cache.Length; i++)
            {
                CacheEntry cacheEntry = this._cache.Get(i);

                if (!DataValidator.ValidateData(cacheEntry))
                    continue;

                if (cacheEntry.Id == CalculateID(songRequestObject))
                {
                    this._cache.Remove(cacheEntry);
                }
            }
        }

        public CacheData GetDataByRequest(SongRequestObject songRequestObject)
        {
            for (int i = 0; i < this._cache.Length; i++)
            {
                CacheEntry cacheEntry = this._cache.Get(i);

                if (!DataValidator.ValidateData(cacheEntry))
                    continue;

                if (cacheEntry.Id.Equals(CalculateID(songRequestObject)))
                {
                    return cacheEntry.CacheData;
                }
            }

            return null;
        }

        public LyricData GetLyricsByRequest(SongRequestObject songRequestObject)
        {
            CacheData data = GetDataByRequest(songRequestObject);

            if (!DataValidator.ValidateData(data))
                return null;

            if (!DataValidator.ValidateData(data.LyricData))
                return null;

            return data.LyricData;
        }
        
        public Artwork GetArtworkByRequest(SongRequestObject songRequestObject)
        {
            CacheData data = GetDataByRequest(songRequestObject);

            if (!DataValidator.ValidateData(data))
                return null;

            if (!DataValidator.ValidateData(data.Artwork))
                return null;

            return data.Artwork;
        }

        public bool IsLyricsInCache(SongRequestObject songRequestObject)
        {
            CacheData cacheData = GetDataByRequest(songRequestObject);

            if (!DataValidator.ValidateData(cacheData))
                return true;

            if (!DataValidator.ValidateData(cacheData.LyricData))
                return true;
            
            return cacheData.LyricData.LyricReturnCode == LyricReturnCode.SUCCESS;
        }
        
        public bool IsArtworkInCache(SongRequestObject songRequestObject)
        {
            CacheData cacheData = GetDataByRequest(songRequestObject);

            if (!DataValidator.ValidateData(cacheData))
                return false;

            return cacheData.Artwork != null;
        }
        
        public bool IsInCache(SongRequestObject songRequestObject)
        {
            for (int i = 0; i < this._cache.Length; i++)
            {
                CacheEntry cacheEntry = this._cache.Get(i);

                if (!DataValidator.ValidateData(cacheEntry))
                    continue;

                if (cacheEntry.Id.Equals(CalculateID(songRequestObject)))
                {
                    return true;
                }
            }

            return false;
        }

        private string CalculateID(SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(songRequestObject))
                return null;
            
            string append = string.Empty;

            append += songRequestObject.SongName;
            append += songRequestObject.Album;
            append += songRequestObject.Artists;
            append += songRequestObject.FormattedSongName;
            append += songRequestObject.FormattedSongAlbum;
            append += songRequestObject.SongDuration;

            return CryptoUtils.ToMD5(append);
        }

        private CacheData ConvertToCacheData(JsonCacheData cacheData)
        {
            JsonSongMetadata songMetadata = cacheData.SongMetadata;
            SongMetadata metadata = new SongMetadata(songMetadata.Name, songMetadata.Album, songMetadata.Artists,
                songMetadata.Duration);

            JsonLyricData lyricData = cacheData.LyricData;
            LyricData lyrics = new LyricData(lyricData.ReturnCode, metadata, lyricData.LyricParts,
                lyricData.LyricProvider, lyricData.LyricType);

            JsonArtwork artworkData = cacheData.Artwork;
            Artwork artwork = new Artwork();
            artwork.ArtworkAsBase64String = artworkData.Artwork;

            return new CacheData(metadata, lyrics, artwork);
        }

        private JsonCacheData ConvertToJsonCacheData(CacheData cacheData)
        {
            if (!DataValidator.ValidateData(cacheData) &&
                !DataValidator.ValidateData(
                    cacheData.Artwork, 
                    cacheData.LyricData, 
                    cacheData.SongMetadata))
                return null;

            SongMetadata metadata = cacheData.SongMetadata;
            JsonSongMetadata jsonSongMetadata = new JsonSongMetadata();
            jsonSongMetadata.Name = metadata.Name;
            jsonSongMetadata.Artists = metadata.Artists;
            jsonSongMetadata.Album = metadata.Album;
            jsonSongMetadata.Duration = metadata.MaxTime;

            LyricData lyricData = cacheData.LyricData;
            JsonLyricData jsonLyricData = new JsonLyricData();
            jsonLyricData.LyricType = lyricData.LyricType;
            jsonLyricData.ReturnCode = lyricData.LyricReturnCode;
            jsonLyricData.LyricProvider = lyricData.LyricProvider;
            jsonLyricData.LyricParts = lyricData.LyricParts;

            Artwork artwork = cacheData.Artwork;
            JsonArtwork jsonArtwork = new JsonArtwork();
            jsonArtwork.Artwork = artwork.ArtworkAsBase64String;

            JsonCacheData jsonCacheData = new JsonCacheData();
            jsonCacheData.SongMetadata = jsonSongMetadata;
            jsonCacheData.LyricData = jsonLyricData;
            jsonCacheData.Artwork = jsonArtwork;

            return jsonCacheData;
        }
    }
}
