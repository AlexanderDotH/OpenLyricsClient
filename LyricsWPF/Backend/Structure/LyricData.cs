using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            this._lyricReturnCode = lyricReturnCode;
            this._lyricParts = lyricParts;
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

        private static LyricPart[] ImproveData(LyricPart[] lyricParts)
        {
            if (DataValidator.ValidateData(lyricParts))
            {
                List<LyricPart> lyricPartsList = new List<LyricPart>();

                LyricPart[] lyrics = null;

                for (int i = 0; i < lyricParts.Length; i++)
                {
                    LyricPart lyricPart = lyricParts[i];

                    //Match

                    //if (lyricPart.Part.Contains())
                }
            }

            return null;
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
