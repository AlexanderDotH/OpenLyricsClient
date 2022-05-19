using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBaseFormat.Structure;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Structure
{
    public class LyricData
    {
        private LyricReturnCode _lyricReturnCode;
        private LyricPart[] _lyricParts;

        public LyricData(LyricReturnCode lyricReturnCode, LyricPart[] lyricParts)
        {
            this._lyricReturnCode = lyricReturnCode;
            this._lyricParts = lyricParts;
        }

        public static LyricData ConvertToData(GenericList<LyricElement> lyrics)
        {
            if (lyrics == null || lyrics.Length == 0)
                return new LyricData(LyricReturnCode.Failed, null);

            LyricPart[] lyricParts = new LyricPart[lyrics.Length];

            for (int i = 0; i < lyrics.Length; i++)
            {
                lyricParts[i] = new LyricPart(lyrics.Get(i).TimeStamp, SongFormatter.FormatString(lyrics.Get(i).Line));
            }

            return new LyricData(LyricReturnCode.Success, lyricParts);
        }

        public LyricReturnCode LyricReturnCode
        {
            get => this._lyricReturnCode;
            set => this._lyricReturnCode = value;
        }

        public LyricPart[] LyricParts
        {
            get => this._lyricParts;
            set => this._lyricParts = value;
        }
    }
}
