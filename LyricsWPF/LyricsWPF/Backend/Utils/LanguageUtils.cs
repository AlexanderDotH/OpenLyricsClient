using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using DevBase.Generic;
using DevBase.Typography;
using Kawazu;
using KoreanRomanisation;

namespace LyricsWPF.Backend.Utils
{
    public class LanguageUtils
    {
        public static bool IsKorean(string text)
            => text.Any<char>(new Func<char, bool>(IsKorean));

        public static bool IsKoreanChar(char c)
        {
            return c >= 0xAC00 && c <= 0xD7A3;
        }
    }
}
