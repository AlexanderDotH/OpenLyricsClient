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
        private string _fullLyrics;
        private LyricType _lyricType;
        private string _songName;

        public LyricData(LyricReturnCode lyricReturnCode, string songName, LyricPart[] lyricParts, string lyricProvider, string fullLyrics, LyricType lyricType)
        {
            this._lyricReturnCode = lyricReturnCode;
            this._songName = songName;
            this._lyricParts = lyricParts;
            this._lyricProvider = lyricProvider;
            this._fullLyrics = fullLyrics;
            _lyricType = lyricType;
        }

        public LyricData(LyricReturnCode lyricReturnCode) : this(lyricReturnCode, null, null, null, null, LyricType.NONE) {}

        public LyricData(LyricReturnCode lyricReturnCode, string songName, LyricType lyricType) : this(lyricReturnCode, songName, null, null, null, lyricType) { }


        public static async Task<LyricData> ConvertToData(GenericList<LyricElement> lyrics, string songName, string lyricProvider)
        {
            if (lyrics == null || lyrics.Length == 0)
                return new LyricData(LyricReturnCode.Failed);

            Romanisation.Romanization romanization = new Romanisation.Romanization();

            LyricPart[] lyricParts = new LyricPart[lyrics.Length];

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < lyrics.Length; i++)
            {
                string currentLine = SongFormatter.FormatString(lyrics.Get(i).Line);

                //Make setting to prevent this
                currentLine = await romanization.Romanize(currentLine);

                lyricParts[i] = new LyricPart(lyrics.Get(i).TimeStamp, currentLine);

                stringBuilder.Append(currentLine + Environment.NewLine);
            }

            return new LyricData(LyricReturnCode.Success, songName, lyricParts, lyricProvider, stringBuilder.ToString(), LyricType.TEXT);
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

        public string FullLyrics
        {
            get => _fullLyrics;
        }

        public LyricType LyricType
        {
            get => _lyricType;
        }

        public string SongName
        {
            get => _songName;
        }
    }
}
