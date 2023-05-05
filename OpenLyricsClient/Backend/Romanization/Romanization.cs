using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HangeulRomanizer;
using Kawazu;
using OpenLyricsClient.Backend.Settings.Sections.Romanization;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Utils;
using Romanization;

//using KoreanRomanisation;

namespace OpenLyricsClient.Backend.Romanization
{
    public class Romanization
    {
        private KawazuConverter _kawazuConverter;
        private RomanizerService _koreanConverter;
        private Russian.IsoR9 _russiaConverter;

        public Romanization()
        {
            try
            {
                this._kawazuConverter = new KawazuConverter();
                this._koreanConverter = new RomanizerService();
                this._russiaConverter = new Russian.IsoR9();
            }
            catch (Exception e)
            { }

        }

        public async Task<string> Romanize(string text)
        {
            if (!DataValidator.ValidateData(text))
                return text;

            List<RomanizeSelection> selections = Core.INSTANCE.SettingsHandler.Settings<RomanizationSection>()
                .GetValue<List<RomanizeSelection>>("Selections");

            if (selections.Count == 0)
                return text;

            if (selections.Contains(RomanizeSelection.JAPANESE_TO_ROMANJI) && DataValidator.ValidateData(this._kawazuConverter))
            {
                if (Utilities.HasJapanese(text))
                {
                    string romanized = await this._kawazuConverter.Convert(text, To.Romaji, Mode.Spaced);
                    return romanized.Trim();
                }
            }

            if (selections.Contains(RomanizeSelection.KOREAN_TO_ROMANJI) && DataValidator.ValidateData(this._koreanConverter))
            {
                if (LanguageUtils.IsKorean(text))
                {
                    string romanized = this._koreanConverter.Romanise(text);
                    return romanized.Trim();
                }
            }

            if (selections.Contains(RomanizeSelection.RUSSIA_TO_LATIN) && DataValidator.ValidateData(this._russiaConverter))
            {
                if (this._russiaConverter.IsPartOfCulture(text))
                {
                    return this._russiaConverter.Process(text);
                }
            }

            return text;
        }

        public async Task<string[]> Romanize(params string[] texts)
        {
            if (!DataValidator.ValidateData(texts))
                return null;

            string[] romanized = new string[texts.Length];

            for (int i = 0; i < texts.Length; i++)
            {
                romanized[i] = await this.Romanize(texts[i]);
            }

            return romanized;
        }
        
        public async Task<LyricPart[]> Romanize(LyricPart[] parts)
        {
            if (!DataValidator.ValidateData(parts))
                return null;

            LyricPart[] romanized = new LyricPart[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                romanized[i] = new LyricPart(parts[i].Time, await this.Romanize(parts[i].Part));
            }

            return romanized;
        }
    }
}
