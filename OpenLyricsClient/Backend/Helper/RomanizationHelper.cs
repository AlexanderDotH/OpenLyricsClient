using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Backend.Helper
{
    public class RomanizationHelper
    {

        private Romanization.Romanization _romanization;

        public RomanizationHelper()
        {
            this._romanization = new Romanization.Romanization();
        }

        public async Task<string> RomanizeFullLyrics(LyricPart[] lyrics)
        {
            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < lyrics.Length; i++)
            {
                LyricPart part = lyrics[i];
                builder.AppendLine(await this._romanization.Romanize(part.Part));
            }

            return builder.ToString();
        }
        
        public async Task<LyricPart[]> RomanizeArray(LyricPart[] lyrics)
        {
            for (int i = 0; i < lyrics.Length; i++)
            {
               lyrics[i].Part = await this._romanization.Romanize(lyrics[i].Part);
            }

            return lyrics;
        }

        public async Task<string> RomanizeString(string lyric)
        {
            return await this._romanization.Romanize(lyric);
        }
    }
}
