using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Utils;
using Opportunity.LrcParser;

namespace LyricsWPF.Backend.Structure
{
    class LyricData
    {
        private LyricReturnCode _lyricReturnCode;
        private LyricPart[] _lyricParts;

        public LyricData(LyricReturnCode lyricReturnCode, LyricPart[] lyricParts)
        {
            _lyricReturnCode = lyricReturnCode;
            _lyricParts = lyricParts;
        }

        public LyricReturnCode LyricReturnCode
        {
            get => _lyricReturnCode;
            set => _lyricReturnCode = value;
        }

        public LyricPart[] LyricParts
        {
            get => _lyricParts;
            set => _lyricParts = value;
        }

        public static LyricData ConvertToData(Lyrics<Line> lyrics)
        {
            if (lyrics == null || lyrics.Lines == null)
                return new LyricData(LyricReturnCode.Failed, null);

            LyricPart[] lyricParts = new LyricPart[lyrics.Lines.Count];

            for (int i = 0; i < lyrics.Lines.Count; i++)
            {
                Line line = lyrics.Lines[i];

                lyricParts[i] = new LyricPart(
                    (long)line.Timestamp.TotalMilliseconds, line.Content);
            }

            return new LyricData(LyricReturnCode.Success, lyricParts);
        }
    }
}
