using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.Typography;
using Kawazu;

namespace LyricsWPF.Backend.Utils
{
    public class LanguageUtils
    {
        public static bool IsJapanese(string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (Utilities.IsJapanese(text[i]))
                    return true;
            }

            return true;
        }
    }
}
