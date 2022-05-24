using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using DevBase.Generic;
using DevBase.Typography;
using Kawazu;

namespace LyricsWPF.Backend.Utils
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
