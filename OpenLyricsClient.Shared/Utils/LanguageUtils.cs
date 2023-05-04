using System;
using System.Linq;

namespace OpenLyricsClient.Shared.Utils
{
    public class LanguageUtils
    {
        public static bool IsKorean(string text)
        {
            return text.Any(IsKoreanChar);
        }

        public static bool IsKoreanChar(char c)
        {
            var unicode = Convert.ToUInt16(c);
            return unicode >= 0xAC00 && unicode <= 0xD7A3;
        }
    }
}
