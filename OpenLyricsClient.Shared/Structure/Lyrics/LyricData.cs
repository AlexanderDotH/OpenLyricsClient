using DevBase.Format.Structure;
using DevBase.Generics;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Shared.Structure.Lyrics
{
    [Serializable]
    public class LyricData
    {
        private LyricReturnCode _lyricReturnCode;
        private LyricPart[] _lyricParts;
        private string _lyricProvider;
        private LyricType _lyricType;
        private SongMetadata _songMetadata;
        private double _lyricSpeed;

        public LyricData(LyricReturnCode lyricReturnCode, SongMetadata songMetadata, LyricPart[] lyricParts, string lyricProvider, LyricType lyricType, double speed)
        {
            this._lyricReturnCode = lyricReturnCode;
            this._lyricParts = lyricParts;
            this._lyricProvider = lyricProvider;
            this._lyricType = lyricType;
            this._songMetadata = songMetadata;
            this._lyricSpeed = speed;
        }

        public LyricData(LyricReturnCode lyricReturnCode = LyricReturnCode.FAILED) : this(lyricReturnCode, null, null, null, LyricType.NONE, 15) { }

        public LyricData(LyricReturnCode lyricReturnCode, SongMetadata songMetadata) : this(lyricReturnCode, songMetadata, null, null, LyricType.NONE, 15) {}

        public LyricData(LyricReturnCode lyricReturnCode, SongMetadata songMetadata, LyricType lyricType) : this(lyricReturnCode, songMetadata, null, null, lyricType, 15) { }

        public static async Task<LyricData> ConvertToData(AList<LyricElement> lyrics, SongMetadata songMetadata, string lyricProvider)
        {
            if (lyrics == null || lyrics.Length == 0)
                return new LyricData();

            LyricPart[] lyricParts = new LyricPart[lyrics.Length];

            LyricPart lastPart = null;
            
            for (int i = 0; i < lyrics.Length; i++)
            {
                string currentLine = FormatStringInternal(lyrics.Get(i).Line);

                lyricParts[i] = new LyricPart(lyrics.Get(i).TimeStamp, currentLine);
            }

            LyricData data = new LyricData(LyricReturnCode.SUCCESS, songMetadata, lyricParts, lyricProvider,
                LyricType.TEXT, CalcSpeedInternal(lyricParts));
            return data;
        }
        
        private static float CalcSpeedInternal(LyricPart[] parts)
        {
            if (!(DataValidator.ValidateData(parts)))
                return 15;

            LyricPart lastPart = null;
            float sum = 0;
        
            float highest = 0;
            int hSum = 0;
        
            for (int i = 0; i < parts.Length; i++)
            {
                LyricPart currentPart = parts[i];
            
                if (lastPart == null)
                {
                    lastPart = currentPart;
                    continue;
                }
                else
                {
                    float value = (currentPart.Time - lastPart.Time);
                
                    sum += value;

                    if (value > highest)
                    {
                        highest += value;
                        hSum++;
                    }

                    lastPart = currentPart;
                    continue;
                }
            }

            float speed = (sum / parts.Length);

            float hSA = highest / hSum;

            hSA *= 1.1f;
            hSA *= 1.1f;
        
            float percentage = 100 / hSA * speed;
        
            return 100.0F - percentage;
        }
        
        private static string FormatStringInternal(string s)
        {
            s = s.Replace("Λ", "/\\");
            s = s.Replace("（", "(");
            s = s.Replace("）", ")");
            s = s.Replace("【", "[");
            s = s.Replace("】", "]");
            s = s.Replace("。", ".");
            s = s.Replace("；", ";");
            s = s.Replace("：", ":");
            s = s.Replace("？", "?");
            s = s.Replace("！", "!");
            s = s.Replace("、", ",");
            s = s.Replace("，", ",");
            s = s.Replace("‘", "'");
            s = s.Replace("’", "'");
            s = s.Replace("′", "'");
            s = s.Replace("＇", "'");
            s = s.Replace("“", "\"");
            s = s.Replace("”", "\"");
            s = s.Replace("〜", "~");
            s = s.Replace("·", "•");
            s = s.Replace("・", "•");
            s = s.Replace("’", "´");
            return s;
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

        public double LyricSpeed
        {
            get => _lyricSpeed;
            set => _lyricSpeed = value;
        }

        public SongMetadata SongMetadata
        {
            get => this._songMetadata;
        }
    }
}
