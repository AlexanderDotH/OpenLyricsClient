using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kawazu;
using LyricsWPF.Backend.Settings;
//using KoreanRomanisation;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Romanisation
{
    public class Romanization
    {
        private KawazuConverter _kawazuConverter;

        public Romanization()
        {
            this._kawazuConverter = new KawazuConverter();
        }

        public async Task<string> Romanize(string text)
        {
            if (!DataValidator.ValidateData(text))
                return text;

            ////Detect Japanese and Convert it
            if (Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.JAPANESE_TO_ROMANJI))
            {
                if (Utilities.HasJapanese(text))
                {
                    string romanized = await this._kawazuConverter.Convert(text, To.Romaji, Mode.Spaced);
                    return romanized.Trim();
                }
            }

            return text;
        }
    }
}
