using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HangeulRomaniser.Services;
using Kawazu;
using LyricsWPF.Backend.Settings;
//using KoreanRomanisation;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Romanisation
{
    public class Romanization
    {
        private KawazuConverter _kawazuConverter;
        private RomaniserService _koreanConverter;

        public Romanization()
        {
            try
            {
                this._kawazuConverter = new KawazuConverter();
                this._koreanConverter = new RomaniserService();
            }
            catch (Exception e)
            { }
        }

        public async Task<string> Romanize(string text)
        {
            if (!DataValidator.ValidateData(text))
                return text;

            if (Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.JAPANESE_TO_ROMANJI) && DataValidator.ValidateData(this._kawazuConverter))
            {
                if (Utilities.HasJapanese(text))
                {
                    string romanized = await this._kawazuConverter.Convert(text, To.Romaji, Mode.Spaced);
                    return romanized.Trim();
                }
            }

            if (Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.KOREAN_TO_ROMANJI) && DataValidator.ValidateData(this._koreanConverter))
            {
                if (LanguageUtils.IsKorean(text))
                {
                    string romanized = this._koreanConverter.Romanise(text);
                    return romanized.Trim();
                }
            }

            return text;
        }
    }
}
