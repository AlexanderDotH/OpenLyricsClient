using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Utils;

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
        
        public async Task<LyricsRoll> RomanizeRoll(LyricsRoll roll)
        {
            string[] romanized = await this._romanization.Romanize(roll.PartOne.Part, roll.PartTwo.Part, roll.PartThree.Part, roll.PartFour.Part, roll.PartFive.Part);

            LyricPart one = new LyricPart(roll.PartOne.Time, romanized[0]);
            LyricPart two = new LyricPart(roll.PartTwo.Time, romanized[1]);
            LyricPart three = new LyricPart(roll.PartThree.Time, romanized[2]);
            LyricPart four = new LyricPart(roll.PartFour.Time, romanized[3]);
            LyricPart five = new LyricPart(roll.PartFive.Time, romanized[4]);

            return new LyricsRoll(one, two, three, four, five);
        }
    }
}
