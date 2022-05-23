using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBaseFormat.Structure;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Utils;
using LyricsWPF.Backend.Romanisation;

namespace LyricsWPF.Backend.Structure
{
    public class LyricData
    {
        private LyricReturnCode _lyricReturnCode;
        private LyricPart[] _lyricParts;
        private string _lyricProvider;

        public LyricData(LyricReturnCode lyricReturnCode, LyricPart[] lyricParts, string lyricProvider)
        {
            this._lyricReturnCode = lyricReturnCode;
            this._lyricParts = lyricParts;
            _lyricProvider = lyricProvider;
        }

        public static async Task<LyricData> ConvertToData(GenericList<LyricElement> lyrics, string lyricProvider)
        {
            if (lyrics == null || lyrics.Length == 0)
                return new LyricData(LyricReturnCode.Failed, null, lyricProvider);

            Romanisation.Romanization romanization = new Romanisation.Romanization();

            LyricPart[] lyricParts = new LyricPart[lyrics.Length];

            for (int i = 0; i < lyrics.Length; i++)
            {
                string currentLine = SongFormatter.FormatString(lyrics.Get(i).Line);

                //Make setting to prevent this
                currentLine = await romanization.Romanize(currentLine);

                lyricParts[i] = new LyricPart(lyrics.Get(i).TimeStamp, currentLine);
            }

            return new LyricData(LyricReturnCode.Success, lyricParts, lyricProvider);
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

        public string LyricProvider
        {
            get => _lyricProvider;
        }
    }
}
