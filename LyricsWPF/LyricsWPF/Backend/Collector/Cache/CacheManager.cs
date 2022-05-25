using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.IO;
using DevBase.Utilities;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Settings;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Cache
{
    public class CacheManager
    {

        private GenericList<CacheEntry> _cache;

        private const string CACHE_EXTENSION = ".cache";
        private const string CACHE_FOLDER_NAME = "Cache";
        private readonly string CACHE_PATH;

        public CacheManager()
        {
            CACHE_PATH = Core.INSTANCE.SettingManager.WorkingDirectory + "\\" + CACHE_FOLDER_NAME + "\\";

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

                long id = Convert.ToInt64(ifo.FileInfo.Name.Replace(CACHE_EXTENSION, string.Empty));

                JsonLyricData jsonLyricData =
                    new JsonDeserializer<JsonLyricData>().Deserialize(AFile.ReadFile(ifo.FileInfo).ToStringData());

                CacheEntry cacheEntry = new CacheEntry(id, ConvertToLyricData(jsonLyricData));
             
                this._cache.Add(cacheEntry);
            }
        }

        public void WriteToCache(SongRequestObject songRequestObject, LyricData cacheData)
        {
            long id = CalculateID(songRequestObject);
            string idAsString = Convert.ToString(id);

            string filePath = CACHE_PATH + idAsString + CACHE_EXTENSION;

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(ConvertToJsonLyricData(cacheData), Formatting.Indented));
                this._cache.Add(new CacheEntry(CalculateID(songRequestObject), cacheData));
            }
        }

        public LyricData GetDataByRequest(SongRequestObject songRequestObject)
        {
            for (int i = 0; i < this._cache.Length; i++)
            {
                CacheEntry cacheEntry = this._cache.Get(i);

                if (cacheEntry.Id == CalculateID(songRequestObject))
                {
                    return cacheEntry.CacheData;
                }
            }

            return null;
        }

        private long CalculateID(SongRequestObject songRequestObject)
        {
            string append = string.Empty;

            append += MemoryUtils.GetSize(songRequestObject.SongName).ToString();
            append += MemoryUtils.GetSize(songRequestObject.Album).ToString();
            append += MemoryUtils.GetSize(songRequestObject.Artists).ToString();
            append += MemoryUtils.GetSize(songRequestObject.FormattedSongName).ToString();
            append += MemoryUtils.GetSize(songRequestObject.FormattedSongAlbum).ToString();
            append += MemoryUtils.GetSize(songRequestObject.SongDuration).ToString();

            return Convert.ToInt64(append);
        }

        private LyricData ConvertToLyricData(JsonLyricData json)
        {
            LyricReturnCode lyricReturnCode = json.LyricReturnCode;
            LyricPart[] lyricParts = json.LyricParts;
            string lyricProvider = json.LyricProvider;
            string fullLyrics = json.FullLyrics;
            LyricType lyricType = json.LyricType;
            string songName = json.SongName;
            string album = json.Album;
            string[] artists = json.Artists;

            return new LyricData(lyricReturnCode, songName, album, artists, lyricParts, lyricProvider, fullLyrics, lyricType);
        }

        private JsonLyricData ConvertToJsonLyricData(LyricData lyricData)
        {
            return new JsonLyricData()
            {
                SongName = lyricData.SongName,
                LyricProvider = lyricData.LyricProvider,
                LyricReturnCode = lyricData.LyricReturnCode,
                LyricParts = lyricData.LyricParts,
                FullLyrics = lyricData.FullLyrics,
                LyricType = lyricData.LyricType,
                Album = lyricData.Album,
                Artists = lyricData.Artists
            };

        }
    }
}
