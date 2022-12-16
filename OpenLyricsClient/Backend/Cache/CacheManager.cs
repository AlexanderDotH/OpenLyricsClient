using System;
using System.IO;
using System.Runtime.InteropServices;
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
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Backend.Cache
{
    public class CacheManager
    {
        private GenericList<CacheEntry> _cache;

        private const string CACHE_EXTENSION = ".cache";
        private const string CACHE_FOLDER_NAME = "Cache";
        private readonly string CACHE_PATH;

        private readonly int _maxCapacity;
        private readonly int _expirationMS;

        private Debugger<CacheManager> _debugger;

        public CacheManager(int maxCapacity, int expirationMs)
        {
            CACHE_PATH = string.Format("{1}{2}{0}", Path.DirectorySeparatorChar, Core.INSTANCE.SettingManager.WorkingDirectory,
                CACHE_FOLDER_NAME);
            
            this._debugger = new Debugger<CacheManager>(this);

            if (!Directory.Exists(CACHE_PATH))
                Directory.CreateDirectory(CACHE_PATH);

            this._cache = new GenericList<CacheEntry>();

            this._maxCapacity = maxCapacity;
            this._expirationMS = expirationMs;

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
                    new JsonDeserializer<JsonCacheData>().Deserialize(FileUtils.ReadFileString(ifo.FileInfo));

                if (this._cache.Length + 1 > this._maxCapacity)
                    continue;
                
                CacheEntry cacheEntry = new CacheEntry(id, ConvertToCacheData(jsonLyricData), CalculateExpirationDate());
                this._cache.Add(cacheEntry);
            }
        }

        public void WriteToCache(SongRequestObject songRequestObject, CacheData cacheData, bool addToCache = false)
        {
            string id = CalculateID(songRequestObject);
            string idAsString = Convert.ToString(id);

            string filePath = CACHE_PATH + idAsString + CACHE_EXTENSION;

            FileUtils.WriteFileString(filePath, JsonConvert.SerializeObject(ConvertToJsonCacheData(cacheData), Formatting.Indented));

            if (addToCache && this._cache.Length + 1 < this._maxCapacity)
                this._cache.Add(new CacheEntry(id, cacheData, CalculateExpirationDate()));
            
            RefreshExpirationEntries();
        }

        public void WriteToCache(SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(songRequestObject))
                return;
            
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
            this._cache.Add(new CacheEntry(id, cacheData, CalculateExpirationDate()));
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

            return TryGetDataFromDisk(songRequestObject);
        }

        private CacheData TryGetDataFromDisk(SongRequestObject songRequestObject)
        {
            string fileName = CACHE_PATH + CalculateID(songRequestObject) + CACHE_EXTENSION;

            if (File.Exists(fileName))
            {
                string data = FileUtils.ReadFileString(fileName);

                if (!DataValidator.ValidateData(data))
                    return null;
                
                JsonCacheData jsonLyricData =
                    new JsonDeserializer<JsonCacheData>().Deserialize(data);
                
                GCHandle.Alloc(data).Free();
                
                return ConvertToCacheData(jsonLyricData);
            }

            return null;
        }
        
        private void RefreshExpirationEntries()
        {
            for (int i = 0; i < this._cache.Length; i++)
            {
                CacheEntry entry = this._cache.Get(i);

                if (DateTimeOffset.Now.ToUnixTimeMilliseconds() > entry.ExpirationDate)
                    this._cache.SafeRemove(entry);
            }
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

            if (!DataValidator.ValidateData(cacheData.Artwork))
                return false;

            return cacheData.Artwork.ReturnCode == ArtworkReturnCode.SUCCESS;
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

        private long CalculateExpirationDate()
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() + this._expirationMS;
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
            artwork.ArtworkColor = cacheData.Artwork.ArtworkColor;

            return new CacheData(metadata, lyrics, artwork);
        }

        private JsonCacheData ConvertToJsonCacheData(CacheData cacheData)
        {
            if (!DataValidator.ValidateData(cacheData) &&
                !DataValidator.ValidateData(
                    cacheData.Artwork, 
                    cacheData.LyricData, 
                    cacheData.SongMetadata) &&
                !DataValidator.ValidateData(
                    cacheData.SongMetadata.Album, 
                    cacheData.SongMetadata.Artists, 
                    cacheData.SongMetadata.Artwork, 
                    cacheData.SongMetadata.Name, 
                    cacheData.SongMetadata.FullArtists, 
                    cacheData.SongMetadata.MaxTime) && 
                !DataValidator.ValidateData(
                    cacheData.Artwork.Data, 
                    cacheData.Artwork.ReturnCode, 
                    cacheData.Artwork.ArtworkColor) &&
                !DataValidator.ValidateData(
                    cacheData.LyricData.LyricParts, 
                    cacheData.LyricData.LyricProvider,
                    cacheData.LyricData.LyricType,
                    cacheData.LyricData.SongMetadata,
                    cacheData.LyricData.LyricReturnCode))
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
            jsonArtwork.ArtworkColor = artwork.ArtworkColor;

            JsonCacheData jsonCacheData = new JsonCacheData();
            jsonCacheData.SongMetadata = jsonSongMetadata;
            jsonCacheData.LyricData = jsonLyricData;
            jsonCacheData.Artwork = jsonArtwork;

            return jsonCacheData;
        }
    }
}
