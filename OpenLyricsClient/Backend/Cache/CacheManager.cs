using System;
using System.IO;
using DevBase.Generic;
using DevBase.IO;
using DevBase.Utilities;
using Newtonsoft.Json;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
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

                JsonLyricData jsonLyricData =
                    new JsonDeserializer<JsonLyricData>().Deserialize(AFile.ReadFile(ifo.FileInfo).ToStringData());

                CacheEntry cacheEntry = new CacheEntry(id, ConvertToLyricData(jsonLyricData));
             
                this._cache.Add(cacheEntry);
            }
        }

        public void WriteToCache(SongRequestObject songRequestObject, LyricData cacheData)
        {
            string id = CalculateID(songRequestObject);
            string idAsString = Convert.ToString(id);

            string filePath = CACHE_PATH + idAsString + CACHE_EXTENSION;

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(ConvertToJsonLyricData(cacheData), Formatting.Indented));
            }
            
            this._cache.Add(new CacheEntry(id, cacheData));
        }

        public void AddToCache(SongRequestObject songRequestObject, LyricData cacheData)
        {
            string id = CalculateID(songRequestObject);
            this._cache.Add(new CacheEntry(id, cacheData));
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

        public LyricData GetDataByRequest(SongRequestObject songRequestObject)
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
            string append = string.Empty;

            append += songRequestObject.SongName;
            append += songRequestObject.Album;
            append += songRequestObject.Artists;
            append += songRequestObject.FormattedSongName;
            append += songRequestObject.FormattedSongAlbum;
            append += songRequestObject.SongDuration;

            return CryptoUtils.ToMD5(append);
        }

        private LyricData ConvertToLyricData(JsonLyricData json)
        {
            LyricPart[] lyricParts = json.LyricParts;
            string lyricProvider = json.LyricProvider;
            LyricType lyricType = json.LyricType;
            string songName = json.SongName;
            string album = json.Album;
            string[] artists = json.Artists;
            long duration = json.Duration;

            return new LyricData(LyricReturnCode.SUCCESS, SongMetadata.ToSongMetadata(songName, album, artists, duration), lyricParts, lyricProvider, lyricType);
        }

        private JsonLyricData ConvertToJsonLyricData(LyricData lyricData)
        {
            return new JsonLyricData()
            {
                SongName = lyricData.SongMetadata == null ? string.Empty : lyricData.SongMetadata.Name,
                Duration = lyricData.SongMetadata == null ? 0 : lyricData.SongMetadata.MaxTime,
                Album = lyricData.SongMetadata == null ? string.Empty : lyricData.SongMetadata.Album,
                Artists = lyricData.SongMetadata == null ? new string[] {""} : lyricData.SongMetadata.Artists,
                LyricProvider = lyricData.LyricProvider,
                LyricParts = lyricData.LyricParts,
                LyricType = lyricData.LyricType
            };
        }
    }
}
