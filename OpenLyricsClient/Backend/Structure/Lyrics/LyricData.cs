using System;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBaseFormat.Structure;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure.Song;

namespace OpenLyricsClient.Backend.Structure.Lyrics
{
    [Serializable]
    public class LyricData
    {
        private LyricReturnCode _lyricReturnCode;
        private LyricPart[] _lyricParts;
        private string _lyricProvider;
        private LyricType _lyricType;
        private SongMetadata _songMetadata;

        public LyricData(LyricReturnCode lyricReturnCode, SongMetadata songMetadata, LyricPart[] lyricParts, string lyricProvider, LyricType lyricType)
        {
            this._lyricReturnCode = lyricReturnCode;
            this._lyricParts = lyricParts;
            this._lyricProvider = lyricProvider;
            this._lyricType = lyricType;
            this._songMetadata = songMetadata;
        }

        public LyricData(LyricReturnCode lyricReturnCode = LyricReturnCode.FAILED) : this(lyricReturnCode, null, null, null, LyricType.NONE) { }

        public LyricData(LyricReturnCode lyricReturnCode, SongMetadata songMetadata) : this(lyricReturnCode, songMetadata, null, null, LyricType.NONE) {}

        public LyricData(LyricReturnCode lyricReturnCode, SongMetadata songMetadata, LyricType lyricType) : this(lyricReturnCode, songMetadata, null, null, lyricType) { }

        public static async Task<LyricData> ConvertToData(GenericList<LyricElement> lyrics, SongMetadata songMetadata, string lyricProvider)
        {
            if (lyrics == null || lyrics.Length == 0)
                return new LyricData();

            LyricPart[] lyricParts = new LyricPart[lyrics.Length];

            LyricPart lastPart = null;
            
            for (int i = 0; i < lyrics.Length; i++)
            {
                string currentLine = SongFormatter.FormatString(lyrics.Get(i).Line);

                lyricParts[i] = new LyricPart(lyrics.Get(i).TimeStamp, currentLine);
            }

            return new LyricData(LyricReturnCode.SUCCESS, songMetadata, lyricParts, lyricProvider, LyricType.TEXT);
        }

        public LyricReturnCode LyricReturnCode
        {
            get => this._lyricReturnCode;
            set => this._lyricReturnCode = value;
        }

        public LyricPart[]? LyricParts
        {
            get => this._lyricParts;
            set => this._lyricParts = value;
        }

        public string LyricProvider
        {
            get => _lyricProvider;
        }
        public LyricType LyricType
        {
            get => _lyricType;
        }

        public SongMetadata SongMetadata
        {
            get => this._songMetadata;
        }
    }
}
