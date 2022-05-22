using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kawazu;
using KoreanRomanisation;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Romanisation
{
    public class Romanisation
    {
        private KawazuConverter _kawazuConverter;
        private YaleRomanisation _koreanConverter;

        public Romanisation()
        {
            this._kawazuConverter = new KawazuConverter();

            this._koreanConverter = new YaleRomanisation();
            this._koreanConverter.PreserveNonKoreanText = true;
        }

        public async Task<string> Romanise(string text)
        {
            if (!DataValidator.ValidateData(text))
                return text;


            //Detect Japanese and Convert it
            if (Utilities.HasJapanese(text))
                return await this._kawazuConverter.Convert(text, To.Romaji, Mode.Spaced);

            return this._koreanConverter.RomaniseText(text);

        }
    }
}
